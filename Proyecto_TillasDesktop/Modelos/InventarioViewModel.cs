using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.ComponentModel;
using System.Windows.Data;

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
                // Cada vez que el usuario presiona una tecla, le pedimos a la tabla que se refresque
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

            // Datos de prueba temporales
            ListaProductos.Add(new ProductoViewModel { Codigo_Modelo = "NK-AF1-01", Nombre = "Air Force 1", Marca = "Nike", Categoria = "Sneakers", Precio_Venta = 125000, StockTotal = 45 });
            ListaProductos.Add(new ProductoViewModel { Codigo_Modelo = "AD-SM-02", Nombre = "Samba OG", Marca = "Adidas", Categoria = "Sneakers", Precio_Venta = 110000, StockTotal = 12 });
        }

        private void AbrirVentanaStock(object parametro)
        {
            // Validamos que el usuario haya hecho clic en una fila primero
            if (ProductoSeleccionado == null)
            {
                MessageBox.Show("Por favor, selecciona un producto de la lista haciendo clic en su fila.", "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var formViewModel = new IngresoStockViewModel(ProductoSeleccionado);

            // Le enseñamos qué hacer cuando se confirme el stock
            formViewModel.OnStockIngresado = (cantidadAgregada) =>
            {
                // Al sumar la cantidad, el DataGrid se actualiza automáticamente gracias al INotifyPropertyChanged
                ProductoSeleccionado.StockTotal += cantidadAgregada;
            };

            var ventana = new Vistas.IngresoStockWindow();
            formViewModel.CerrarVentana = ventana.Close;
            ventana.DataContext = formViewModel;
            ventana.ShowDialog();
        }

        private void AbrirVentanaNuevoModelo(object parametro)
        {
            // Creamos el ViewModel del formulario
            var formViewModel = new NuevoModeloViewModel();

            // Le decimos qué hacer cuando el formulario guarde un modelo
            formViewModel.OnModeloGuardado = (nuevoProducto) =>
            {
                // Al agregarlo a esta ObservableCollection, la tabla de WPF se actualiza sola al instante
                ListaProductos.Add(nuevoProducto);
            };

            // Creamos la ventana y le asignamos el ViewModel ya configurado
            var ventana = new Vistas.NuevoModeloWindow();
            formViewModel.CerrarVentana = ventana.Close;
            ventana.DataContext = formViewModel;

            // Mostramos la ventana
            ventana.ShowDialog();
        }

        // El motor de búsqueda: retorna TRUE si el producto debe mostrarse, FALSE si debe ocultarse
        private bool FiltrarCriterios(object obj)
        {
            if (obj is ProductoViewModel producto)
            {
                // Si la barra está vacía, mostramos todos los productos
                if (string.IsNullOrWhiteSpace(TextoBusqueda))
                    return true;

                // Pasamos todo a minúsculas para que la búsqueda no sea sensible a mayúsculas/minúsculas
                string filtro = TextoBusqueda.ToLower();

                // Buscamos coincidencias en el Nombre, el Código o la Marca
                return (producto.Nombre != null && producto.Nombre.ToLower().Contains(filtro)) ||
                       (producto.Codigo_Modelo != null && producto.Codigo_Modelo.ToLower().Contains(filtro)) ||
                       (producto.Marca != null && producto.Marca.ToLower().Contains(filtro));
            }
            return false;
        }
    }
}
