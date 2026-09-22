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
    public class PuntoVentaViewModel : ViewModelBase
    {
        private readonly VentasService _ventasService;

        // === COLECCIONES PARA LAS TABLAS ===

        // 1. Catálogo Izquierdo: Muestra todos los talles disponibles para vender
        public ObservableCollection<ProductoDisponibleViewModel> ListaCatalogo { get; set; }

        // 2. Carrito Derecho: Muestra lo que el cliente está a punto de llevarse
        public ObservableCollection<LineaCarritoViewModel> Carrito { get; set; }

        // 3. ComboBox de Clientes: Permite asociar la venta a una persona
        public ObservableCollection<string> ClientesTotales { get; set; }

        // Interfaz de filtrado reactivo para la barra de búsqueda del catálogo
        public ICollectionView VistaFiltroCatalogo { get; set; }

        private string _busquedaRapida;
        public string BusquedaRapida
        {
            get => _busquedaRapida;
            set
            {
                _busquedaRapida = value;
                OnPropertyChanged();
                VistaFiltroCatalogo?.Refresh(); // Re-evalúa el filtro cada vez que el vendedor escribe una letra.
            }
        }

        // Datos informativos del encabezado de facturación
        public string FechaActual { get; set; }
        public string VendedorActual { get; set; }

        private string _clienteSeleccionado;
        public string ClienteSeleccionado
        {
            get => _clienteSeleccionado;
            set { _clienteSeleccionado = value; OnPropertyChanged(); }
        }

        private string _metodoPagoSeleccionado;
        public string MetodoPagoSeleccionado
        {
            get => _metodoPagoSeleccionado;
            set { _metodoPagoSeleccionado = value; OnPropertyChanged(); }
        }
        public ObservableCollection<string> MetodosPago { get; set; }

        // Valor monetario de todo el carrito
        private decimal _totalCobrar;
        public decimal TotalCobrar
        {
            get => _totalCobrar;
            set { _totalCobrar = value; OnPropertyChanged(); }
        }

        // === COMANDOS ENLAZADOS A LOS BOTONES DE LA UI ===
        public ICommand LimpiarBusquedaCommand { get; }
        public ICommand AgregarAlCarritoCommand { get; }
        public ICommand QuitarDelCarritoCommand { get; }
        public ICommand CobrarCommand { get; }

        public PuntoVentaViewModel()
        {
            _ventasService = new VentasService();

            FechaActual = DateTime.Now.ToString("dd/MM/yyyy");
            VendedorActual = $"Vendedor: {Proyecto_TillasDesktop.App.NombreUsuarioActual}"; // Conectar con el usuario logueado en App.xaml.

            // Inicialización de colecciones (Listas desplegables).
            MetodosPago = new ObservableCollection<string> { "Efectivo", "Tarjeta de Débito", "Tarjeta de Crédito", "Transferencia", "MercadoPago" };
            ClientesTotales = new ObservableCollection<string> { "Consumidor Final", "Juan Pérez", "María Gómez" };
            ClienteSeleccionado = "Consumidor Final"; // Valor por defecto obligatorio por AFIP/SRI.

            ListaCatalogo = new ObservableCollection<ProductoDisponibleViewModel>();
            Carrito = new ObservableCollection<LineaCarritoViewModel>();

            VistaFiltroCatalogo = CollectionViewSource.GetDefaultView(ListaCatalogo);
            VistaFiltroCatalogo.Filter = FiltrarCatalogo;

            LimpiarBusquedaCommand = new RelayCommand(LimpiarBusqueda);
            AgregarAlCarritoCommand = new RelayCommand(AgregarAlCarrito);
            QuitarDelCarritoCommand = new RelayCommand(QuitarDelCarrito);
            CobrarCommand = new RelayCommand(ConfirmarCobro);

            CargarDatosDePrueba();
        }

        private void LimpiarBusqueda(object parametro)
        {
            // Restablecer el texto de búsqueda hace que el filtro se anule y se muestre todo el inventario.
            BusquedaRapida = string.Empty;
        }

        private bool FiltrarCatalogo(object obj)
        {
            if (obj is ProductoDisponibleViewModel producto)
            {
                if (string.IsNullOrWhiteSpace(BusquedaRapida)) return true;

                string filtro = BusquedaRapida.ToLower();
                return (producto.Nombre != null && producto.Nombre.ToLower().Contains(filtro)) ||
                       (producto.Codigo_Modelo != null && producto.Codigo_Modelo.ToLower().Contains(filtro)) ||
                       (producto.NombreMarca != null && producto.NombreMarca.ToLower().Contains(filtro));
            }
            return false;
        }

        // === LÓGICA CORE: CARRITO ===

        // Evento disparado al hacer doble clic en una zapatilla del catálogo o al presionar su botón "+".
        private void AgregarAlCarrito(object parametro)
        {
            // El parámetro trae la zapatilla seleccionada desde el XAML.
            if (parametro is ProductoDisponibleViewModel productoCatalogo)
            {
                // Defensa contra venta "en negativo".
                if (productoCatalogo.Stock_Actual <= 0)
                {
                    MessageBox.Show("No hay stock suficiente de este talle.", "Sin Stock", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Busca si el cliente ya había agregado un par exactamente igual (mismo modelo y talle) al carrito.
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

        // Calcula el valor monetario a cobrar cada vez que el carrito se modifica.
        private void RecalcularTotal()
        {
            // Transforma la lista del carrito en una lista de Detalles que la BLL pueda entender.
            var detallesVenta = Carrito.Select(c => new DetalleVenta
            {
                Producto_Talle_ID = c.ProductoID,
                Cantidad = c.Cantidad,
                Precio_Unitario = c.PrecioUnitario
            });

            // Delega el cálculo matemático a la BLL.
            TotalCobrar = _ventasService.CalcularTotalVenta(detallesVenta);
        }

        // ====================================================================
        // FINALIZAR VENTA (Botón "COBRAR")
        // ====================================================================
        private void ConfirmarCobro(object parametro)
        {
            // 1. Validaciones preventivas para evitar tickets corruptos.
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

            // 2. Transforma el carrito final a entidades puras.
            var detallesVenta = Carrito.Select(c => new DetalleVenta
            {
                Producto_Talle_ID = c.ProductoID,
                Cantidad = c.Cantidad,
                Precio_Unitario = c.PrecioUnitario
            }).ToList();

            // 3. Ejecuta la transacción (que debe restar los stocks físicos reales y generar la factura (en teoria)).
            bool exito = _ventasService.RegistrarVenta(detallesVenta, out string mensaje);

            // 4. Resolución.
            if (exito)
            {
                MessageBox.Show($"Cobro por {TotalCobrar:C} procesado con éxito.\nCliente: {ClienteSeleccionado}\nPago: {MetodoPagoSeleccionado}\n\n{mensaje}",
                                "Venta Registrada", MessageBoxButton.OK, MessageBoxImage.Information);

                // Limpia el campo de busqueda.
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