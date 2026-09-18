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
        public ObservableCollection<ProductoDisponibleViewModel> ListaCatalogo { get; set; }
        public ObservableCollection<LineaCarritoViewModel> Carrito { get; set; }
        public ObservableCollection<string> ClientesTotales { get; set; }
        public ICollectionView VistaFiltroCatalogo { get; set; }
        private string _busquedaRapida;
        public string BusquedaRapida
        {
            get => _busquedaRapida;
            set
            {
                _busquedaRapida = value;
                OnPropertyChanged();
                // Actualizamos la tabla cada vez que el usuario teclea una letra
                VistaFiltroCatalogo?.Refresh();
            }
        }
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
        private decimal _totalCobrar;
        public decimal TotalCobrar
        {
            get => _totalCobrar;
            set { _totalCobrar = value; OnPropertyChanged(); }
        }

        // === COMANDOS ===
        public ICommand LimpiarBusquedaCommand { get; }
        public ICommand AgregarAlCarritoCommand { get; }
        public ICommand QuitarDelCarritoCommand { get; }
        public ICommand CobrarCommand { get; }

        public PuntoVentaViewModel()
        {
            _ventasService = new VentasService();

            FechaActual = DateTime.Now.ToString("dd/MM/yyyy");
            // FUTURO: Aquí leeremos el usuario que inició sesión. Por ahora lo simulamos.
            VendedorActual = "Usuario: Admin";

            MetodosPago = new ObservableCollection<string> { "Efectivo", "Tarjeta de Débito", "Tarjeta de Crédito", "Transferencia", "MercadoPago" };

            ClienteSeleccionado = "Consumidor Final";

            ListaCatalogo = new ObservableCollection<ProductoDisponibleViewModel>();
            Carrito = new ObservableCollection<LineaCarritoViewModel>();

            VistaFiltroCatalogo = CollectionViewSource.GetDefaultView(ListaCatalogo);
            VistaFiltroCatalogo.Filter = FiltrarCatalogo;

            ClientesTotales = new ObservableCollection<string> { "Consumidor Final", "Juan Pérez", "María Gómez" };

            LimpiarBusquedaCommand = new RelayCommand(LimpiarBusqueda);
            AgregarAlCarritoCommand = new RelayCommand(AgregarAlCarrito);
            QuitarDelCarritoCommand = new RelayCommand(QuitarDelCarrito);
            CobrarCommand = new RelayCommand(ConfirmarCobro);

            CargarDatosDePrueba();
        }

        private void LimpiarBusqueda(object parametro)
        {
            // Al vaciar la variable, el XAML se actualiza y la tabla vuelve a mostrar todo el catálogo
            BusquedaRapida = string.Empty;
        }

        private bool FiltrarCatalogo(object obj)
        {
            if (obj is ProductoDisponibleViewModel producto)
            {
                // Si la barra está vacía, mostramos todo
                if (string.IsNullOrWhiteSpace(BusquedaRapida)) return true;

                string filtro = BusquedaRapida.ToLower();

                // Buscamos coincidencias en Nombre, Código o Marca
                return (producto.Nombre != null && producto.Nombre.ToLower().Contains(filtro)) ||
                       (producto.Codigo_Modelo != null && producto.Codigo_Modelo.ToLower().Contains(filtro)) ||
                       (producto.NombreMarca != null && producto.NombreMarca.ToLower().Contains(filtro));
            }
            return false;
        }

        // === LÓGICA DE CARRITO ===
        private void AgregarAlCarrito(object parametro)
        {
            if (parametro is ProductoDisponibleViewModel productoCatalogo)
            {
                if (productoCatalogo.Stock_Actual <= 0)
                {
                    MessageBox.Show("No hay stock suficiente de este talle.", "Sin Stock", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var itemEnCarrito = Carrito.FirstOrDefault(c => c.ProductoID == productoCatalogo.ProductoID && c.Talle == productoCatalogo.Talle);

                if (itemEnCarrito != null)
                {
                    itemEnCarrito.Cantidad++;
                }
                else
                {
                    Carrito.Add(new LineaCarritoViewModel
                    {
                        ProductoID = productoCatalogo.ProductoID,
                        Nombre = productoCatalogo.Nombre,
                        Talle = productoCatalogo.Talle,
                        PrecioUnitario = productoCatalogo.Precio_Venta,
                        Cantidad = 1
                    });
                }

                productoCatalogo.Stock_Actual--;
                RecalcularTotal();
            }
        }

        private void QuitarDelCarrito(object parametro)
        {
            if (parametro is LineaCarritoViewModel itemCarrito)
            {
                var productoCatalogo = ListaCatalogo.FirstOrDefault(p => p.ProductoID == itemCarrito.ProductoID && p.Talle == itemCarrito.Talle);

                if (productoCatalogo != null)
                {
                    productoCatalogo.Stock_Actual++;
                }

                itemCarrito.Cantidad--;

                if (itemCarrito.Cantidad == 0)
                {
                    Carrito.Remove(itemCarrito);
                }

                RecalcularTotal();
            }
        }

        private void RecalcularTotal()
        {
            var detallesVenta = Carrito.Select(c => new DetalleVenta
            {
                Producto_Talle_ID = c.ProductoID,
                Cantidad = c.Cantidad,
                Precio_Unitario = c.PrecioUnitario
            });

            TotalCobrar = _ventasService.CalcularTotalVenta(detallesVenta);
        }

        private void ConfirmarCobro(object parametro)
        {
            // Validación 1: Carrito vacío
            if (!Carrito.Any())
            {
                MessageBox.Show("El carrito está vacío. Agrega productos antes de cobrar.", "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Validación 2: Cliente vacío (por si el usuario borró el Consumidor Final sin querer)
            if (string.IsNullOrWhiteSpace(ClienteSeleccionado))
            {
                MessageBox.Show("Debes seleccionar un cliente válido para la facturación.", "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Validación 3: Método de pago sin seleccionar
            if (string.IsNullOrWhiteSpace(MetodoPagoSeleccionado))
            {
                MessageBox.Show("Por favor, selecciona un método de pago antes de confirmar el cobro.", "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Si pasa todas las validaciones, procedemos a facturar
            var detallesVenta = Carrito.Select(c => new DetalleVenta
            {
                Producto_Talle_ID = c.ProductoID,
                Cantidad = c.Cantidad,
                Precio_Unitario = c.PrecioUnitario
            }).ToList();

            bool exito = _ventasService.RegistrarVenta(detallesVenta, out string mensaje);

            if (exito)
            {
                MessageBox.Show($"Cobro por {TotalCobrar:C} procesado con éxito.\nCliente: {ClienteSeleccionado}\nPago: {MetodoPagoSeleccionado}\n\n{mensaje}",
                                "Venta Registrada", MessageBoxButton.OK, MessageBoxImage.Information);

                // Limpiamos la pantalla para el siguiente cliente y restauramos los valores por defecto
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

        // === DATOS SIMULADOS PARA EL CATÁLOGO VISUAL ===
        private void CargarDatosDePrueba()
        {
            ListaCatalogo.Add(new ProductoDisponibleViewModel { ProductoID = 1, Codigo_Modelo = "NK-AF1-01", Nombre = "Nike Air Force 1", NombreMarca = "Nike", Talle = 40, Stock_Actual = 5, Precio_Venta = 125000 });
            ListaCatalogo.Add(new ProductoDisponibleViewModel { ProductoID = 1, Codigo_Modelo = "NK-AF1-01", Nombre = "Nike Air Force 1", NombreMarca = "Nike", Talle = 42, Stock_Actual = 2, Precio_Venta = 125000 });
            ListaCatalogo.Add(new ProductoDisponibleViewModel { ProductoID = 2, Codigo_Modelo = "AD-SM-02", Nombre = "Adidas Samba OG", NombreMarca = "Adidas", Talle = 39, Stock_Actual = 1, Precio_Venta = 110000 });
        }
    }
}