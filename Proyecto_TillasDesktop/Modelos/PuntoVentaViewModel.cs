using System;
using System.ComponentModel;
using System.Windows.Data;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using TillasDesktop.BLL.Services;
using TillasDesktop.Entities.Facturacion;

namespace TillasDesktop.UI.Modelos
{
    // Hereda de ViewModelBase para mantener la pantalla sincronizada con los datos mediante notificaciones automáticas.
    public class PuntoVentaViewModel : ViewModelBase
    {
        // Instancia del servicio que maneja la lógica de negocio y las transacciones de venta.
        private readonly VentasService _ventasService;

        private readonly int _idUsuarioActual = Proyecto_TillasDesktop.App.IdUsuarioActual;

        // Colección observable del catálogo izquierdo: muestra todos los talles de zapatillas disponibles para vender.
        public ObservableCollection<ProductoDisponibleViewModel> ListaCatalogo { get; set; }

        // Colección observable del carrito derecho: muestra los ítems que el cliente va a llevarse.
        public ObservableCollection<LineaCarritoViewModel> Carrito { get; set; }

        // Lista de clientes disponibles para asociar a la factura.
        public ObservableCollection<string> ClientesTotales { get; set; }

        // Colección con las formas de pago habilitadas en el sistema.
        public ObservableCollection<string> MetodosPago { get; set; }

        // Vista especial de colección para filtrar el catálogo en tiempo real sin romper la lista original.
        public ICollectionView VistaFiltroCatalogo { get; set; }

        // Campo privado que almacena el texto que el cajero escribe en el buscador rápido.
        private string _busquedaRapida;

        // Propiedad pública vinculada a la barra de búsqueda del catálogo.
        public string BusquedaRapida
        {
            get => _busquedaRapida;
            set
            {
                _busquedaRapida = value;
                OnPropertyChanged();

                // Cada vez que el vendedor tipea una letra, le avisamos a la vista que vuelva a evaluar el filtro.
                VistaFiltroCatalogo?.Refresh(); 
            }
        }

        // Datos informativos para el encabezado del ticket o factura.
        public string FechaActual { get; set; }
        public string VendedorActual { get; set; }

        // Cliente seleccionado actualmente en el ComboBox de facturación.
        private string _clienteSeleccionado;
        public string ClienteSeleccionado
        {
            get => _clienteSeleccionado;
            set { _clienteSeleccionado = value; OnPropertyChanged(); }
        }

        // Método de pago seleccionado actualmente (efectivo, transferencia, tarjeta, etc.).
        private string _metodoPagoSeleccionado;
        public string MetodoPagoSeleccionado
        {
            get => _metodoPagoSeleccionado;
            set { _metodoPagoSeleccionado = value; OnPropertyChanged(); }
        }

        // Monto monetario total a cobrar por todos los productos del carrito.
        private decimal _totalCobrar;
        public decimal TotalCobrar
        {
            get => _totalCobrar;
            set { _totalCobrar = value; OnPropertyChanged(); }
        }

        // Comandos asociados a los botones de la interfaz (limpiar búsqueda, agregar, quitar y cobrar).
        public ICommand LimpiarBusquedaCommand { get; }
        public ICommand AgregarAlCarritoCommand { get; }
        public ICommand QuitarDelCarritoCommand { get; }
        public ICommand CobrarCommand { get; }

        // Constructor principal: inicializa servicios, fechas, catálogos, comandos y datos de prueba.
        public PuntoVentaViewModel()
        {
            _ventasService = new VentasService();

            // Liberamos reservas colgadas al abrir la caja.
            _ventasService.LimpiarReservasPendientes();

            FechaActual = DateTime.Now.ToString("dd/MM/yyyy");

            // Conectamos con el nombre del usuario logueado almacenado en las variables globales de la aplicación.
            VendedorActual = $"Vendedor: {Proyecto_TillasDesktop.App.NombreUsuarioActual}"; // Conectar con el usuario logueado en App.xaml.

            // Inicializamos las listas desplegables con las opciones estándar de pago y clientes.
            MetodosPago = new ObservableCollection<string> { "Efectivo", "Tarjeta de Débito", "Tarjeta de Crédito", "Transferencia", "MercadoPago" };
            ClientesTotales = new ObservableCollection<string> { "Consumidor Final", "Juan Pérez", "María Gómez" };

            // Establecemos "Consumidor Final" por defecto por cuestiones fiscales obligatorias.
            ClienteSeleccionado = "Consumidor Final"; 

            ListaCatalogo = new ObservableCollection<ProductoDisponibleViewModel>();
            Carrito = new ObservableCollection<LineaCarritoViewModel>();

            // Configuramos la vista filtrada del catálogo basándonos en la lista de productos disponibles.
            VistaFiltroCatalogo = CollectionViewSource.GetDefaultView(ListaCatalogo);
            VistaFiltroCatalogo.Filter = FiltrarCatalogo;

            // Vinculamos cada comando con su método ejecutable correspondiente.
            LimpiarBusquedaCommand = new RelayCommand(LimpiarBusqueda);
            AgregarAlCarritoCommand = new RelayCommand(AgregarAlCarrito);
            QuitarDelCarritoCommand = new RelayCommand(QuitarDelCarrito);
            CobrarCommand = new RelayCommand(ConfirmarCobro);

            CargarCatalogo();
        }

        // Limpia el cuadro de texto de búsqueda para volver a mostrar todo el catálogo completo.
        private void LimpiarBusqueda(object parametro)
        {
            // Restablecer el texto de búsqueda hace que el filtro se anule y se muestre todo el inventario.
            BusquedaRapida = string.Empty;
        }

        // Método que evalúa renglón por renglón si el producto del catálogo coincide con lo escrito en la búsqueda rápida.
        private bool FiltrarCatalogo(object obj)
        {
            if (obj is ProductoDisponibleViewModel producto)
            {
                if (string.IsNullOrWhiteSpace(BusquedaRapida)) return true;

                string filtro = BusquedaRapida.ToLower();

                // Devuelve true si el filtro coincide con el Nombre, el Código SKU o la Marca.
                return (producto.Nombre != null && producto.Nombre.ToLower().Contains(filtro)) ||
                       (producto.Codigo_Modelo != null && producto.Codigo_Modelo.ToLower().Contains(filtro)) ||
                       (producto.NombreMarca != null && producto.NombreMarca.ToLower().Contains(filtro));
            }
            return false;
        }



        // Evento disparado al presionar el botón "+"
        private void AgregarAlCarrito(object parametro)
        {
            // Verificamos que el parámetro recibido sea realmente un producto de la lista visual
            if (parametro is ProductoDisponibleViewModel productoCatalogo)
            {
                // Intentamos bloquear 1 unidad física en SQL Server ANTES de sumarla a la pantalla.
                // Le pasamos el ID del cajero, el ID del talle físico (ProductoID) y la cantidad requerida (1).
                bool reservaExitosa = _ventasService.AgregarAlCarrito(_idUsuarioActual, productoCatalogo.ProductoID, 1);

                // Si SQL rechaza la reserva (ej: alguien más lo reservó en este milisegundo o no hay stock real)
                if (!reservaExitosa)
                {
                    MessageBox.Show("Stock insuficiente o producto retenido por otro cajero en este momento.", "Sin Stock", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return; // Cortamos la ejecución, el carrito visual queda intacto.
                }

                // Si la base de datos nos da luz verde, procedemos a actualizar el carrito visual.
                // Buscamos si el cliente ya tenía este mismo producto y talle en su lista.
                var itemEnCarrito = Carrito.FirstOrDefault(c => c.ProductoID == productoCatalogo.ProductoID && c.Talle == productoCatalogo.Talle);

                if (itemEnCarrito != null)
                {
                    // Si ya existía, simplemente le sumamos 1 a la cantidad que ya iba a llevar
                    itemEnCarrito.Cantidad++;

                    // Reemplazamos la fila en su misma posición para forzar a la grilla a repintar el número actualizado
                    int index = Carrito.IndexOf(itemEnCarrito);
                    Carrito[index] = itemEnCarrito;
                }
                else
                {
                    // Si es un producto completamente nuevo en el carrito, creamos un renglón desde cero
                    Carrito.Add(new LineaCarritoViewModel
                    {
                        ProductoID = productoCatalogo.ProductoID,
                        Nombre = productoCatalogo.Nombre,
                        Talle = productoCatalogo.Talle,
                        PrecioUnitario = productoCatalogo.Precio_Venta,
                        Cantidad = 1
                    });
                }

                // Descontamos "visualmente" el stock del catálogo izquierdo para que el vendedor sepa que queda 1 menos
                productoCatalogo.Stock_Actual--;

                // Finalmente, recalculamos el total monetario a cobrar
                RecalcularTotal();
            }
        }

        // Evento disparado al presionar el botón "-" en el renglón del carrito.
        private void QuitarDelCarrito(object parametro)
        {
            // Verificamos que el parámetro recibido sea un renglón válido de nuestro carrito
            if (parametro is LineaCarritoViewModel itemCarrito)
            {
                // Le pedimos a SQL Server que devuelva 1 unidad a la estantería virtual
                bool liberado = _ventasService.QuitarDelCarrito(_idUsuarioActual, itemCarrito.ProductoID, 1);

                // Solo si la base de datos confirmó la liberación sin errores, actualizamos la pantalla
                if (liberado)
                {
                    // Buscamos el producto original en el catálogo de la izquierda para devolverle su número
                    var productoCatalogo = ListaCatalogo.FirstOrDefault(p => p.ProductoID == itemCarrito.ProductoID && p.Talle == itemCarrito.Talle);

                    if (productoCatalogo != null)
                    {
                        // Le devolvemos "visualmente" el stock que el cliente acaba de soltar
                        productoCatalogo.Stock_Actual++;
                    }

                    // Restamos 1 a la cantidad que el cliente llevaba en este renglón
                    itemCarrito.Cantidad--;

                    if (itemCarrito.Cantidad == 0)
                    {
                        // Si la cantidad llega a 0, significa que el cliente ya no lleva este talle en absoluto, borramos la fila.
                        Carrito.Remove(itemCarrito);
                    }
                    else
                    {
                        // Si aún quedan unidades (ej: llevaba 2 pares iguales y ahora lleva 1), forzamos a la grilla a repintar.
                        int index = Carrito.IndexOf(itemCarrito);
                        Carrito[index] = itemCarrito;
                    }

                    // Recalculamos el monto final con la nueva cantidad de productos
                    RecalcularTotal();
                }
            }
        }

        // Calcula el monto monetario total sumando los subtotales de cada renglón del carrito.
        private void RecalcularTotal()
        {
            // Transformamos los ítems del carrito en una colección de detalles que la capa de negocios (BLL) pueda procesar.
            var detallesVenta = Carrito.Select(c => new DetalleVenta
            {
                Producto_Talle_ID = c.ProductoID,
                Cantidad = c.Cantidad,
                Precio_Unitario = c.PrecioUnitario
            });

            // Delegamos el cálculo matemático final al servicio de ventas.
            TotalCobrar = _ventasService.CalcularTotalVenta(detallesVenta);
        }

        // Valida los datos finales y procesa el cobro y registro definitivo de la venta en la base de datos
        private void ConfirmarCobro(object parametro)
        {
            // Evita enviar peticiones inútiles a la base de datos si falta información básica.
            if (!Carrito.Any())
            {
                MessageBox.Show("El carrito está vacío. Agrega productos antes de cobrar.", "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(ClienteSeleccionado) || string.IsNullOrWhiteSpace(MetodoPagoSeleccionado))
            {
                MessageBox.Show("Selecciona un cliente y un método de pago válidos.", "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Traducimos los datos visuales de la pantalla a las "Entidades" que entiende la Capa de Negocios y Datos.
            var nuevaVenta = new Venta
            {
                Fecha_Hora = DateTime.Now,
                Usuario_ID = _idUsuarioActual, // El cajero que está facturando
                Cliente_ID = 1, // MOCK
                Total = TotalCobrar
            };

            var nuevoPago = new Pago
            {
                Tipo_Pago_ID = 1, // MOCK
                Monto = TotalCobrar,
                Fecha_Pago = DateTime.Now,
                Activo = true
            };

            // Convertimos nuestra lista visual 'LineaCarritoViewModel' a una lista de 'DetalleVenta'
            var detallesVenta = Carrito.Select(c => new DetalleVenta
            {
                Producto_Talle_ID = c.ProductoID,
                Cantidad = c.Cantidad,
                Precio_Unitario = c.PrecioUnitario,
                Subtotal = c.Cantidad * c.PrecioUnitario // Calculamos el subtotal que guardará el registro
            }).ToList();

            // EJECUCIÓN DE LA TRANSACCIÓN
            // Delegamos todo el paquete a la Capa de Negocios. El repositorio abrirá una SqlTransaction que:
            // - Guardará el Pago
            // - Guardará la Venta
            // - Guardará los Detalles
            // - Descontará el stock REAL y eliminará las reservas temporales
            bool exito = _ventasService.RegistrarVenta(nuevaVenta, nuevoPago, detallesVenta, out string mensaje);

            if (exito)
            {
                MessageBox.Show($"Cobro por {TotalCobrar:C} procesado con éxito.\n\n{mensaje}",
                                "Venta Registrada", MessageBoxButton.OK, MessageBoxImage.Information);

                // Si la base de datos confirmó todo, limpiamos la pantalla para el siguiente cliente
                Carrito.Clear();
                TotalCobrar = 0;
                BusquedaRapida = string.Empty;
                MetodoPagoSeleccionado = null;
                ClienteSeleccionado = "Consumidor Final";
            }
            else
            {
                // Si falló (ej. se cortó la conexión en medio del cobro), mostramos el error y el carrito queda intacto
                MessageBox.Show(mensaje, "Error al registrar la venta", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CargarCatalogo()
        {
            ListaCatalogo.Clear();

            // 1. Pedimos el catálogo fresco a la base de datos
            var productosDb = _ventasService.ObtenerCatalogoPuntoVenta();

            // 2. Transformamos la entidad pura al ViewModel que entiende el XAML de tu pantalla
            foreach (var p in productosDb)
            {
                ListaCatalogo.Add(new ProductoDisponibleViewModel
                {
                    ProductoID = p.ProductoTalle_ID, // Es crítico que este sea el ID de ProductoTalles
                    Codigo_Modelo = p.Codigo_Modelo,
                    Nombre = p.Nombre,
                    NombreMarca = p.NombreMarca,
                    NombreCategoria = p.NombreCategoria,
                    Talle = p.Talle,
                    Stock_Actual = p.Stock_Disponible, // Asignamos el stock ya restado con las reservas
                    Precio_Venta = p.Precio_Venta
                });
            }

            // Refrescamos el filtro visual por si había texto escrito en el buscador
            VistaFiltroCatalogo?.Refresh();
        }
    }
}