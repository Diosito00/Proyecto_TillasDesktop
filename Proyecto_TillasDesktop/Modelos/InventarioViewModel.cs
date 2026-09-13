using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows;
using TillasDesktop.Entities.Inventario;

namespace TillasDesktop.UI.Modelos
{
    public class InventarioViewModel : ViewModelBase
    {
        private string _textoBusqueda = string.Empty;
        private ProductoViewModel _productoSeleccionado;

        public ObservableCollection<ProductoViewModel> ListaProductos { get; set; }
        public ICollectionView VistaFiltroProductos { get; set; }

        public string TextoBusqueda
        {
            get => _textoBusqueda;
            set
            {
                _textoBusqueda = value;
                OnPropertyChanged();
                VistaFiltroProductos.Refresh();
            }
        }

        public ProductoViewModel ProductoSeleccionado
        {
            get => _productoSeleccionado;
            set { _productoSeleccionado = value; OnPropertyChanged(); }
        }

        public ICommand IngresarStockCommand { get; }
        public ICommand NuevoModeloCommand { get; }

        public InventarioViewModel()
        {
            ListaProductos = new ObservableCollection<ProductoViewModel>();
            VistaFiltroProductos = CollectionViewSource.GetDefaultView(ListaProductos);
            VistaFiltroProductos.Filter = FiltrarCriterios;

            IngresarStockCommand = new RelayCommand(AbrirVentanaStock);
            NuevoModeloCommand = new RelayCommand(AbrirVentanaNuevoModelo);

            // Primero creamos la entidad pura, luego la envolvemos en el ViewModel.
            var producto1 = new Producto { Codigo_Modelo = "NK-AF1-01", Nombre = "Air Force 1", Precio_Venta = 125000 };
            var producto2 = new Producto { Codigo_Modelo = "AD-SM-02", Nombre = "Samba OG", Precio_Venta = 110000 };

            ListaProductos.Add(new ProductoViewModel(producto1) { NombreMarca = "Nike", NombreCategoria = "Sneakers", StockTotal = 45 });
            ListaProductos.Add(new ProductoViewModel(producto2) { NombreMarca = "Adidas", NombreCategoria = "Sneakers", StockTotal = 12 });
        }

        private void AbrirVentanaStock(object parametro)
        {
            if (ProductoSeleccionado == null)
            {
                MessageBox.Show("Selecciona un producto primero.", "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var formViewModel = new IngresoStockViewModel(ProductoSeleccionado);
            formViewModel.OnStockIngresado = (cantidadAgregada) =>
            {
                ProductoSeleccionado.StockTotal += cantidadAgregada;
            };

            var ventana = new Vistas.IngresoStockWindow();
            formViewModel.CerrarVentana = ventana.Close;
            ventana.DataContext = formViewModel;
            ventana.ShowDialog();
        }

        private void AbrirVentanaNuevoModelo(object parametro)
        {
            var formViewModel = new NuevoModeloViewModel();
            formViewModel.OnModeloGuardado = (nuevoProducto) =>
            {
                ListaProductos.Add(nuevoProducto);
            };

            var ventana = new Vistas.NuevoModeloWindow();
            formViewModel.CerrarVentana = ventana.Close;
            ventana.DataContext = formViewModel;
            ventana.ShowDialog();
        }

        private bool FiltrarCriterios(object obj)
        {
            if (obj is ProductoViewModel producto)
            {
                if (string.IsNullOrWhiteSpace(TextoBusqueda)) return true;

                string filtro = TextoBusqueda.ToLower();
                return (producto.Nombre != null && producto.Nombre.ToLower().Contains(filtro)) ||
                       (producto.Codigo_Modelo != null && producto.Codigo_Modelo.ToLower().Contains(filtro)) ||
                       (producto.NombreMarca != null && producto.NombreMarca.ToLower().Contains(filtro));
            }
            return false;
        }
    }
}