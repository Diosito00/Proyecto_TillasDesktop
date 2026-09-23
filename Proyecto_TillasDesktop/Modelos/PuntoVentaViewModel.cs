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

        // Colección observable del catálogo izquierdo: muestra todos los talles de zapatillas disponibles para vender.
        public ObservableCollection<ProductoDisponibleViewModel> ListaCatalogo { get; set; }

        // Colección observable del carrito derecho: muestra los ítems que el cliente va a llevarse.
        public ObservableCollection<LineaCarritoViewModel> Carrito { get; set; }

        // Lista de clientes disponibles para asociar a la factura.
        public ObservableCollection<string> ClientesTotales { get; set; }

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

        // Colección con las formas de pago habilitadas en el sistema.
        public ObservableCollection<string> MetodosPago { get; set; }

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

            CargarDatosDePrueba();
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

        

        // Evento disparado al hacer doble clic en una zapatilla del catálogo o al presionar su botón "+".
        private void AgregarAlCarrito(object parametro)
        {
            // El parámetro trae la zapatilla seleccionada desde el XAML.
            if (parametro is ProductoDisponibleViewModel productoCatalogo)
            {
                // Control de seguridad para evitar vender más de lo que hay en stock físico.
                if (productoCatalogo.Stock_Actual <= 0)
                {
                    MessageBox.Show("No hay stock suficiente de este talle.", "Sin Stock", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Verificamos si el cliente ya había agregado un par exactamente igual (mismo modelo y talle) al carrito.
                var itemEnCarrito = Carrito.FirstOrDefault(c => c.ProductoID == productoCatalogo.ProductoID && c.Talle == productoCatalogo.Talle);

                if (itemEnCarrito != null)
                {
                    // Si ya estaba en el carrito, solo aumenta la cantidad del renglón (Evita líneas duplicadas en el ticket).
                    itemEnCarrito.Cantidad++;
                }
                else
                {
                    // Si es nuevo, lo agrega como renglón independiente.
                    Carrito.Add(new LineaCarritoViewModel
                    {
                        ProductoID = productoCatalogo.ProductoID,
                        Nombre = productoCatalogo.Nombre,
                        Talle = productoCatalogo.Talle,
                        PrecioUnitario = productoCatalogo.Precio_Venta,
                        Cantidad = 1
                    });
                }

                // Descuenta "visualmente" el stock del catálogo para que el vendedor sepa cuántos quedan.
                productoCatalogo.Stock_Actual--;

                // Recalculamos el total a cobrar de la venta.
                RecalcularTotal();
            }
        }

        // Evento disparado al presionar el botón "-" en el renglón del carrito.
        private void QuitarDelCarrito(object parametro)
        {
            if (parametro is LineaCarritoViewModel itemCarrito)
            {
                // Busca el producto original en el catálogo de la izquierda.
                var productoCatalogo = ListaCatalogo.FirstOrDefault(p => p.ProductoID == itemCarrito.ProductoID && p.Talle == itemCarrito.Talle);

                if (productoCatalogo != null)
                {
                    // Le devuelve "visualmente" el stock disponible.
                    productoCatalogo.Stock_Actual++;
                }

                // Resta la cantidad que el cliente iba a llevarse.
                itemCarrito.Cantidad--;

                if (itemCarrito.Cantidad == 0)
                {
                    // Si la cantidad llega a 0, destruye el renglón del carrito.
                    Carrito.Remove(itemCarrito);
                }

                RecalcularTotal();
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


        // Valida los datos y procesa el cobro y registro definitivo de la venta
        private void ConfirmarCobro(object parametro)
        {
            // Validaciones preventivas para asegurar que la venta esté completa y correcta.
            if (!Carrito.Any())
            {
                MessageBox.Show("El carrito está vacío. Agrega productos antes de cobrar.", "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(ClienteSeleccionado))
            {
                MessageBox.Show("Debes seleccionar un cliente válido para la facturación.", "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(MetodoPagoSeleccionado))
            {
                MessageBox.Show("Por favor, selecciona un método de pago antes de confirmar el cobro.", "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Convertimos los ítems del carrito a una lista de entidades puras para enviarlas a la base de datos.
            var detallesVenta = Carrito.Select(c => new DetalleVenta
            {
                Producto_Talle_ID = c.ProductoID,
                Cantidad = c.Cantidad,
                Precio_Unitario = c.PrecioUnitario
            }).ToList();

            //  Ejecuta la transacción (que debe restar los stocks físicos reales y generar la factura (en teoria)).
            bool exito = _ventasService.RegistrarVenta(detallesVenta, out string mensaje);

            // Resolución según el resultado de la operación.
            if (exito)
            {
                MessageBox.Show($"Cobro por {TotalCobrar:C} procesado con éxito.\nCliente: {ClienteSeleccionado}\nPago: {MetodoPagoSeleccionado}\n\n{mensaje}",
                                "Venta Registrada", MessageBoxButton.OK, MessageBoxImage.Information);

                // Limpiamos el carrito y reseteamos los campos para dejar la pantalla lista para la siguiente venta.
                Carrito.Clear();
                TotalCobrar = 0;
                BusquedaRapida = string.Empty;
                MetodoPagoSeleccionado = null;
                ClienteSeleccionado = "Consumidor Final";
            }
            else
            {
                MessageBox.Show(mensaje, "Error al registrar la venta", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CargarDatosDePrueba()
        {
            // Talles iniciales para el Air Force 1 (ID = 1): 39 (Stock: 20) y 42 (Stock: 25)
            ListaCatalogo.Add(new ProductoDisponibleViewModel { ProductoID = 1, Codigo_Modelo = "NK-AF1-01", Nombre = "Nike Air Force 1", NombreMarca = "Nike", Talle = 39, Stock_Actual = 20, Precio_Venta = 125000 });
            ListaCatalogo.Add(new ProductoDisponibleViewModel { ProductoID = 1, Codigo_Modelo = "NK-AF1-01", Nombre = "Nike Air Force 1", NombreMarca = "Nike", Talle = 42, Stock_Actual = 25, Precio_Venta = 125000 });

            // Talles iniciales para el Samba OG (ID = 2): 35 (Stock: 10) y 40 (Stock: 15)
            ListaCatalogo.Add(new ProductoDisponibleViewModel { ProductoID = 2, Codigo_Modelo = "AD-SM-02", Nombre = "Adidas Samba OG", NombreMarca = "Adidas", Talle = 35, Stock_Actual = 10, Precio_Venta = 110000 });
            ListaCatalogo.Add(new ProductoDisponibleViewModel { ProductoID = 2, Codigo_Modelo = "AD-SM-02", Nombre = "Adidas Samba OG", NombreMarca = "Adidas", Talle = 40, Stock_Actual = 15, Precio_Venta = 110000 });
        }
    }
}