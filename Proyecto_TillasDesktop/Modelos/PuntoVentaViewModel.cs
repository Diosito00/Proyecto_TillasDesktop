// Importa clases base y de soporte para metadatos y conversión de colecciones en WPF
using System;
using System.ComponentModel;
using System.Windows.Data;
// Importa colecciones genéricas y observables para mantener sincronizadas las grillas en tiempo real
using System.Collections.ObjectModel;
// Importa extensiones de LINQ para consultas, filtrado y proyecciones en memoria
using System.Linq;
// Importa clases base de WPF para la gestión de la aplicación y cuadros de mensaje interactivos
using System.Windows;
// Importa las utilidades de comandos y eventos de entrada de la interfaz de usuario en WPF
using System.Windows.Input;
// Importa los servicios de la Capa de Negocios y utilidades de ventas y clientes
using TillasDesktop.BLL.Services;
using TillasDesktop.Entities.Facturacion;
using TillasDesktop.BLL;
using TillasDesktop.Entities.Clientes;
using TillasDesktop.UI.Vistas;
using TillasDesktop.BLL.Servicios;

// Define el espacio de nombres (namespace) correspondiente a los modelos de vista de la interfaz gráfica del sistema
namespace TillasDesktop.UI.Modelos
{
    // Declara la clase pública PuntoVentaViewModel que hereda de ViewModelBase, encargada de administrar el punto de venta (POS), catálogo izquierdo, carrito de compras, reservas de stock y cobro/facturación.
    public class PuntoVentaViewModel : ViewModelBase
    {
        // Instancias de los servicios de negocio encargados de procesar la lógica de ventas y la gestión de clientes.
        private readonly VentasService _ventasService;
        private readonly ClienteService _clienteService;

        // Identificador numérico único del usuario (cajero) autenticado actualmente, obtenido de las variables globales de la aplicación.
        private readonly int _idUsuarioActual = Proyecto_TillasDesktop.App.IdUsuarioActual;

        // Colección observable del catálogo izquierdo: muestra todos los talles de zapatillas disponibles para vender en el punto de venta.
        public ObservableCollection<ProductoDisponibleViewModel> ListaCatalogo { get; set; }

        // Colección observable del carrito derecho: muestra los ítems y cantidades seleccionadas que el cliente va a llevarse.
        public ObservableCollection<LineaCarritoViewModel> Carrito { get; set; }

        // Lista de clientes disponibles cargados desde la base de datos para asociar a la factura.
        public ObservableCollection<Cliente> ClientesTotales { get; set; }

        // Colección con las formas de pago habilitadas en el sistema (ej. Efectivo, Tarjeta, Transferencia).
        public ObservableCollection<TipoPago> MetodosPago { get; set; }

        // Vista especial de colección (ICollectionView) para filtrar el catálogo en tiempo real sin alterar la lista original.
        public ICollectionView VistaFiltroCatalogo { get; set; }

        // Campo privado que almacena el texto que el cajero escribe en el buscador rápido.
        private string _busquedaRapida;

        // Propiedad pública vinculada a la barra de búsqueda del catálogo con notificación de cambios.
        public string BusquedaRapida
        {
            get => _busquedaRapida;
            set
            {
                _busquedaRapida = value;
                OnPropertyChanged();

                // Cada vez que el vendedor tipea una letra, le avisamos a la vista que vuelva a evaluar el método de filtrado.
                VistaFiltroCatalogo?.Refresh();
            }
        }

        // Datos informativos y descriptivos para el encabezado del ticket o factura.
        public string FechaActual { get; set; }
        public string VendedorActual { get; set; }

        // Cliente seleccionado actualmente en el ComboBox de facturación.
        private Cliente _clienteSeleccionado;
        public Cliente ClienteSeleccionado
        {
            get => _clienteSeleccionado;
            set { _clienteSeleccionado = value; OnPropertyChanged(); }
        }

        // Método de pago seleccionado actualmente en la interfaz.
        private TipoPago _metodoPagoSeleccionado;
        public TipoPago MetodoPagoSeleccionado
        {
            get => _metodoPagoSeleccionado;
            set { _metodoPagoSeleccionado = value; OnPropertyChanged(); }
        }

        // Monto monetario total a cobrar por todos los productos acumulados en el carrito.
        private decimal _totalCobrar;
        public decimal TotalCobrar
        {
            get => _totalCobrar;
            set { _totalCobrar = value; OnPropertyChanged(); }
        }

        // Comandos públicos asociados a los botones de la interfaz (limpiar búsqueda, agregar, quitar, editar cantidad y cobrar).
        public ICommand LimpiarBusquedaCommand { get; }
        public ICommand AgregarAlCarritoCommand { get; }
        public ICommand QuitarDelCarritoCommand { get; }
        public ICommand CobrarCommand { get; }
        public ICommand EditarCantidadCommand { get; }

        // Constructor principal: inicializa servicios, fechas, catálogos, comandos, recupera carritos abiertos y configura valores por defecto.
        public PuntoVentaViewModel()
        {
            _ventasService = new VentasService();
            _clienteService = new ClienteService();

            // Establece la fecha actual formateada para la cabecera
            FechaActual = DateTime.Now.ToString("dd/MM/yyyy");

            // Conectamos con el nombre del usuario logueado almacenado en las variables globales de la aplicación.
            VendedorActual = $"Vendedor: {Proyecto_TillasDesktop.App.NombreUsuarioActual}";

            // Inicializa las colecciones observables principales
            ListaCatalogo = new ObservableCollection<ProductoDisponibleViewModel>();
            Carrito = new ObservableCollection<LineaCarritoViewModel>();

            // Configuramos la vista filtrada del catálogo basándonos en la lista de productos disponibles.
            VistaFiltroCatalogo = CollectionViewSource.GetDefaultView(ListaCatalogo);
            VistaFiltroCatalogo.Filter = FiltrarCatalogo;

            // Vinculamos cada comando con su método ejecutable correspondiente mediante RelayCommand.
            LimpiarBusquedaCommand = new RelayCommand(LimpiarBusqueda);
            AgregarAlCarritoCommand = new RelayCommand(AgregarAlCarrito);
            QuitarDelCarritoCommand = new RelayCommand(QuitarDelCarrito);
            CobrarCommand = new RelayCommand(ConfirmarCobro);
            EditarCantidadCommand = new RelayCommand(EditarCantidad);

            // Carga los datos iniciales y recupera sesiones pendientes
            CargarCatalogo();
            RecuperarCarritoLocal();
            CargarListasDesplegables();
        }

        // Método privado que limpia el cuadro de texto de búsqueda para volver a mostrar todo el catálogo completo de productos.
        private void LimpiarBusqueda(object parametro)
        {
            BusquedaRapida = string.Empty;
        }

        // Método delegado que evalúa renglón por renglón si el producto del catálogo coincide con lo escrito en la búsqueda rápida.
        private bool FiltrarCatalogo(object obj)
        {
            if (obj is ProductoDisponibleViewModel producto)
            {
                if (string.IsNullOrWhiteSpace(BusquedaRapida)) return true;

                string filtro = BusquedaRapida.ToLower();

                // Devuelve true si el filtro coincide parcialmente con el Nombre, el Código SKU, la Marca o el Talle.
                return (producto.Nombre != null && producto.Nombre.ToLower().Contains(filtro)) ||
                       (producto.Codigo_Modelo != null && producto.Codigo_Modelo.ToLower().Contains(filtro)) ||
                       (producto.NombreMarca != null && producto.NombreMarca.ToLower().Contains(filtro)) ||
                       (producto.Talle.ToString().Contains(filtro));
            }
            return false;
        }

        // Método que se ejecuta al presionar el botón "+" para agregar un producto al carrito de compras.
        private void AgregarAlCarrito(object parametro)
        {
            // Verificamos que el parámetro recibido sea realmente un producto válido del catálogo visual izquierdo.
            if (parametro is ProductoDisponibleViewModel productoCatalogo)
            {
                // Intentamos bloquear/reservar 1 unidad física en SQL Server ANTES de sumarla a la pantalla (control de concurrencia y stock).
                bool reservaExitosa = _ventasService.AgregarAlCarrito(_idUsuarioActual, productoCatalogo.ProductoID, 1);

                // Si SQL rechaza la reserva (ej: otro cajero lo reservó simultáneamente o no hay stock real disponible)...
                if (!reservaExitosa)
                {
                    MessageBox.Show("Stock insuficiente o producto retenido por otro cajero en este momento.", "Sin Stock", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return; // Cortamos la ejecución aquí; el carrito visual permanece intacto.
                }

                // Si la base de datos nos da luz verde, procedemos a actualizar el carrito visual en memoria.
                var itemEnCarrito = Carrito.FirstOrDefault(c => c.ProductoID == productoCatalogo.ProductoID && c.Talle == productoCatalogo.Talle);

                if (itemEnCarrito != null)
                {
                    // Si ya existía una línea para este producto y talle, incrementamos su cantidad.
                    itemEnCarrito.Cantidad++;

                    // Reemplazamos la fila en su misma posición para forzar a la grilla a repintar el número actualizado.
                    int index = Carrito.IndexOf(itemEnCarrito);
                    Carrito[index] = itemEnCarrito;
                }
                else
                {
                    // Si es un producto completamente nuevo en el carrito, creamos un renglón desde cero.
                    Carrito.Add(new LineaCarritoViewModel
                    {
                        ProductoID = productoCatalogo.ProductoID,
                        Nombre = productoCatalogo.Nombre,
                        Talle = productoCatalogo.Talle,
                        PrecioUnitario = productoCatalogo.Precio_Venta,
                        Cantidad = 1
                    });
                }

                // Descontamos visualmente el stock del catálogo izquierdo para reflejar la disponibilidad en tiempo real.
                productoCatalogo.Stock_Actual--;

                // Recalculamos el monto monetario total a cobrar.
                RecalcularTotal();
            }
        }

        // Método que se ejecuta al presionar el botón "-" para quitar un ítem o decrementar su cantidad en el carrito.
        private void QuitarDelCarrito(object parametro)
        {
            // Verificamos que el parámetro recibido sea un renglón válido de nuestro carrito
            if (parametro is LineaCarritoViewModel itemCarrito)
            {
                // Le pedimos a SQL Server que libere y devuelva 1 unidad a la estantería virtual.
                bool liberado = _ventasService.QuitarDelCarrito(_idUsuarioActual, itemCarrito.ProductoID, 1);

                // Solo si la base de datos confirmó la liberación sin errores, actualizamos la pantalla.
                if (liberado)
                {
                    // Buscamos el producto original en el catálogo de la izquierda para reasignarle su stock.
                    var productoCatalogo = ListaCatalogo.FirstOrDefault(p => p.ProductoID == itemCarrito.ProductoID && p.Talle == itemCarrito.Talle);

                    if (productoCatalogo != null)
                    {
                        productoCatalogo.Stock_Actual++;
                    }

                    // Decrementamos la cantidad en el renglón del carrito.
                    itemCarrito.Cantidad--;

                    if (itemCarrito.Cantidad == 0)
                    {
                        // Si la cantidad llega a 0, eliminamos la fila por completo del carrito.
                        Carrito.Remove(itemCarrito);
                    }
                    else
                    {
                        // Si aún quedan unidades, forzamos a la grilla a repintar el renglón.
                        int index = Carrito.IndexOf(itemCarrito);
                        Carrito[index] = itemCarrito;
                    }

                    // Recalculamos el monto final.
                    RecalcularTotal();
                }
            }
        }

        // Método privado que calcula el monto monetario total sumando los subtotales de cada renglón del carrito.
        private void RecalcularTotal()
        {
            // Transformamos los ítems del carrito en una colección de detalles que la capa de negocios pueda procesar.
            var detallesVenta = Carrito.Select(c => new DetalleVenta
            {
                Producto_Talle_ID = c.ProductoID,
                Cantidad = c.Cantidad,
                Precio_Unitario = c.PrecioUnitario
            });

            // Delegamos el cálculo matemático final al servicio de ventas.
            TotalCobrar = _ventasService.CalcularTotalVenta(detallesVenta);
        }

        // Método privado que valida los datos finales, procesa el cobro y registra definitivamente la venta en la base de datos.
        private void ConfirmarCobro(object parametro)
        {
            // Evita enviar peticiones si el carrito está vacío.
            if (!Carrito.Any())
            {
                MessageBox.Show("El carrito está vacío. Agrega productos antes de cobrar.", "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Valida que se haya seleccionado un cliente y un método de pago.
            if (ClienteSeleccionado == null || MetodoPagoSeleccionado == null)
            {
                MessageBox.Show("Selecciona un cliente y un método de pago válidos.", "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Traducimos los datos visuales a las entidades del dominio para la capa de negocios.
            var nuevaVenta = new Venta
            {
                Fecha_Hora = DateTime.Now,
                Usuario_ID = _idUsuarioActual,
                Cliente_ID = ClienteSeleccionado.ID,
                Total = TotalCobrar
            };

            // Preparamos la entidad de pago asociada.
            var nuevoPago = new Pago
            {
                Tipo_Pago_ID = MetodoPagoSeleccionado.ID,
                Monto = TotalCobrar,
                Fecha_Pago = DateTime.Now,
                Activo = true
            };

            // Convertimos la lista visual a una lista de detalles de venta con sus respectivos subtotales.
            var detallesVenta = Carrito.Select(c => new DetalleVenta
            {
                Producto_Talle_ID = c.ProductoID,
                Cantidad = c.Cantidad,
                Precio_Unitario = c.PrecioUnitario,
                Subtotal = c.Cantidad * c.PrecioUnitario
            }).ToList();

            // EJECUCIÓN DE LA TRANSACCIÓN: Delegamos la operación al servicio, el cual abre una transacción SQL para registrar pago, venta, detalles y descontar stock real.
            bool exito = _ventasService.RegistrarVenta(nuevaVenta, nuevoPago, detallesVenta, out string mensaje);

            if (exito)
            {
                // Generamos una copia del carrito para enviarla al servicio de impresión de tickets PDF.
                var copiaCarrito = Carrito.Select(c => new ItemCarritoDTO
                {
                    Nombre = c.Nombre,
                    Talle = c.Talle,
                    Cantidad = c.Cantidad,
                    Precio_Unitario = c.PrecioUnitario
                }).ToList();

                string nombreCliente = ClienteSeleccionado.NombreCompleto;
                string nombrePago = MetodoPagoSeleccionado.Nombre;
                string nombreVendedor = Proyecto_TillasDesktop.App.NombreUsuarioActual;
                decimal totalFacturado = TotalCobrar;

                // Si la base de datos confirmó el éxito, limpiamos y reseteamos la pantalla para el siguiente cliente.
                Carrito.Clear();
                VistaFiltroCatalogo.Refresh();
                TotalCobrar = 0;
                BusquedaRapida = string.Empty;
                MetodoPagoSeleccionado = (MetodosPago.FirstOrDefault(m => m.Nombre.Contains("Efectivo")) ?? MetodosPago.FirstOrDefault())!;
                ClienteSeleccionado = (ClientesTotales.FirstOrDefault(c => c.Nombre.Contains("Consumidor Final")) ?? ClientesTotales.FirstOrDefault())!;

                try
                {
                    // Invoca al servicio de generación de comprobantes en PDF.
                    var ticketPdfService = new TicketPdfService();
                    ticketPdfService.GenerarYGuardarTicket(nombreVendedor, nombreCliente, nombrePago, totalFacturado, copiaCarrito);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("La venta se cobró correctamente, pero hubo un error al generar o mostrar el ticket PDF.\n\nDetalle: " + ex.Message,
                                    "Error de Impresión", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            else
            {
                // Si la transacción falló, mostramos el mensaje de error y el carrito permanece intacto.
                MessageBox.Show(mensaje, "Error al registrar la venta", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Método privado que permite modificar interactivamente la cantidad de unidades de un renglón del carrito mediante una ventana modal.
        private void EditarCantidad(object parametro)
        {
            if (parametro is LineaCarritoViewModel itemCarrito)
            {
                // Muestra la ventana modal de edición de cantidad.
                var ventana = new EditarCantidadWindow(itemCarrito.Nombre, itemCarrito.Cantidad);
                bool? resultado = ventana.ShowDialog();

                if (resultado != true) return;

                int nuevaCantidad = ventana.NuevaCantidad;
                if (nuevaCantidad == itemCarrito.Cantidad) return; // Si no hay cambios, no hacemos nada.

                // Calcula la diferencia de unidades.
                int diferencia = nuevaCantidad - itemCarrito.Cantidad;
                var productoCatalogo = ListaCatalogo.FirstOrDefault(p => p.ProductoID == itemCarrito.ProductoID && p.Talle == itemCarrito.Talle);

                // Comprobamos y validamos la diferencia contra la base de datos.
                if (diferencia > 0)
                {
                    bool reservaExitosa = _ventasService.AgregarAlCarrito(_idUsuarioActual, itemCarrito.ProductoID, diferencia);
                    if (!reservaExitosa)
                    {
                        MessageBox.Show($"No hay stock suficiente para agregar {diferencia} unidades más.", "Stock Insuficiente", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    if (productoCatalogo != null) productoCatalogo.Stock_Actual -= diferencia;
                }
                else if (diferencia < 0)
                {
                    int cantidadADevolver = Math.Abs(diferencia);
                    bool liberado = _ventasService.QuitarDelCarrito(_idUsuarioActual, itemCarrito.ProductoID, cantidadADevolver);
                    if (liberado && productoCatalogo != null)
                    {
                        productoCatalogo.Stock_Actual += cantidadADevolver;
                    }
                }

                // Actualizamos la cantidad en el carrito visual.
                itemCarrito.Cantidad = nuevaCantidad;

                if (itemCarrito.Cantidad == 0)
                {
                    Carrito.Remove(itemCarrito);
                }
                else
                {
                    int index = Carrito.IndexOf(itemCarrito);
                    Carrito[index] = itemCarrito;
                }

                RecalcularTotal();
            }
        }

        // Método privado que carga y puebla el catálogo de productos disponibles desde la capa de servicios.
        private void CargarCatalogo()
        {
            ListaCatalogo.Clear();

            var productosDb = _ventasService.ObtenerCatalogoPuntoVenta();

            foreach (var p in productosDb)
            {
                ListaCatalogo.Add(new ProductoDisponibleViewModel
                {
                    ProductoID = p.ProductoTalle_ID,
                    Codigo_Modelo = p.Codigo_Modelo,
                    Nombre = p.Nombre,
                    NombreMarca = p.NombreMarca,
                    NombreCategoria = p.NombreCategoria,
                    Talle = p.Talle,
                    Stock_Actual = p.Stock_Disponible, // Asignamos el stock descontando las reservas temporales activas.
                    Precio_Venta = p.Precio_Venta
                });
            }

            VistaFiltroCatalogo?.Refresh();
        }

        // Método privado que recupera carritos pendientes o sesiones abiertas dejadas a medias por el usuario actual.
        private void RecuperarCarritoLocal()
        {
            var itemsRecuperados = _ventasService.RecuperarCarritoAbierto(_idUsuarioActual);

            foreach (var item in itemsRecuperados)
            {
                var itemExistente = Carrito.FirstOrDefault(c => c.ProductoID == item.Producto_Talle_ID && c.Talle == item.Talle);

                if (itemExistente != null)
                {
                    itemExistente.Cantidad += item.Cantidad;

                    int index = Carrito.IndexOf(itemExistente);
                    Carrito[index] = itemExistente;
                }
                else
                {
                    Carrito.Add(new LineaCarritoViewModel
                    {
                        ProductoID = item.Producto_Talle_ID,
                        Nombre = item.Nombre,
                        Talle = item.Talle,
                        PrecioUnitario = item.Precio_Unitario,
                        Cantidad = item.Cantidad
                    });
                }
            }

            RecalcularTotal();
        }

        // Método privado que carga las listas desplegables de clientes y métodos de pago, configurando selecciones predeterminadas.
        private void CargarListasDesplegables()
        {
            // Cargar Clientes
            var listaClientes = _clienteService.ObtenerClientes();
            ClientesTotales = new ObservableCollection<Cliente>(listaClientes);

            // Establece "Consumidor Final" por defecto buscando en la lista o tomando el primero disponible.
            ClienteSeleccionado = (ClientesTotales.FirstOrDefault(c => c.Nombre.Contains("Consumidor Final"))
                                   ?? ClientesTotales.FirstOrDefault())!;

            // Cargar Métodos de Pago
            var listaPagos = _ventasService.ObtenerTiposPago();
            MetodosPago = new ObservableCollection<TipoPago>(listaPagos);

            // Establece "Efectivo" por defecto o toma el primer método disponible.
            MetodoPagoSeleccionado = (MetodosPago.FirstOrDefault(m => m.Nombre.Contains("Efectivo"))
                                      ?? MetodosPago.FirstOrDefault())!;
        }
    }
}