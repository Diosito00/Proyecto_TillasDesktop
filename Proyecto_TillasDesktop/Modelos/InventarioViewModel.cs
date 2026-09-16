using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using TillasDesktop.BLL.Services;
using TillasDesktop.Entities.Inventario;

namespace TillasDesktop.UI.Modelos
{
    public class InventarioViewModel : ViewModelBase
    {
        private readonly InventarioService _inventarioService;
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
        public ICommand VerDetalleStockCommand { get; }
        public ICommand ModificarModeloCommand { get; }
        public ICommand EliminarModeloCommand { get; }

        public InventarioViewModel()
        {
            _inventarioService = new InventarioService();
            ListaProductos = new ObservableCollection<ProductoViewModel>();
            VistaFiltroProductos = CollectionViewSource.GetDefaultView(ListaProductos);
            VistaFiltroProductos.Filter = FiltrarCriterios;

            IngresarStockCommand = new RelayCommand(AbrirVentanaStock);
            NuevoModeloCommand = new RelayCommand(AbrirVentanaNuevoModelo);
            VerDetalleStockCommand = new RelayCommand(AbrirVentanaDetalleStock);
            ModificarModeloCommand = new RelayCommand(AbrirVentanaModificar);
            EliminarModeloCommand = new RelayCommand(EliminarModelo);

            // Primero creamos la entidad pura, luego la envolvemos en el ViewModel.
            var producto1 = new Producto { Codigo_Modelo = "NK-AF1-01", Nombre = "Air Force 1", Precio_Venta = 125000 };
            var producto2 = new Producto { Codigo_Modelo = "AD-SM-02", Nombre = "Samba OG", Precio_Venta = 110000 };

            ListaProductos.Add(new ProductoViewModel(producto1) { NombreMarca = "Nike", NombreCategoria = "Deportivo", StockTotal = 45 });
            ListaProductos.Add(new ProductoViewModel(producto2) { NombreMarca = "Adidas", NombreCategoria = "Deportivo", StockTotal = 12 });
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

        private void AbrirVentanaDetalleStock(object parametro)
        {
            // El 'parametro' es exactamente el ProductoViewModel de la fila que clickeó el usuario
            if (parametro is ProductoViewModel productoFila)
            {
                // Instanciamos el nuevo ViewModel de detalle que crearemos en el paso 3
                var detalleViewModel = new DetalleStockViewModel(productoFila);

                var ventana = new Vistas.DetalleStockWindow();
                detalleViewModel.CerrarVentana = ventana.Close;
                ventana.DataContext = detalleViewModel;
                ventana.ShowDialog();

                // Al cerrar la ventana, forzamos que se refresque el stock total en caso de que hayan ingresado más
                VistaFiltroProductos.Refresh();
            }
        }

        private void AbrirVentanaModificar(object parametro)
        {
            if (parametro is ProductoViewModel productoFila)
            {
                // Le pasamos el producto al nuevo constructor que acabamos de crear
                var formViewModel = new NuevoModeloViewModel(productoFila);

                var ventana = new Vistas.NuevoModeloWindow();
                formViewModel.CerrarVentana = ventana.Close;
                ventana.DataContext = formViewModel;
                ventana.ShowDialog();

                // Al cerrar la ventana, forzamos un refresco por si cambió el nombre o el código
                VistaFiltroProductos.Refresh();
            }
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

        private void EliminarModelo(object parametro)
        {
            if (parametro is ProductoViewModel productoFila)
            {
                // Confirmación de seguridad crucial antes de borrar
                var respuesta = MessageBox.Show($"¿Estás seguro de que deseas eliminar permanentemente el modelo '{productoFila.Nombre}'?",
                                                "Confirmar Eliminación",
                                                MessageBoxButton.YesNo,
                                                MessageBoxImage.Warning);

                if (respuesta == MessageBoxResult.Yes)
                {
                    // Extraemos el ID y lo mandamos a la BLL
                    int idReal = productoFila.ObtenerEntidadPura().ID;
                    bool exito = _inventarioService.EliminarProducto(idReal, out string mensaje);

                    if (exito)
                    {
                        // Si la BD lo borró, lo quitamos de la lista visual
                        ListaProductos.Remove(productoFila);
                    }
                    else
                    {
                        MessageBox.Show(mensaje, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
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