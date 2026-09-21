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

        public ICommand NuevoModeloCommand { get; }
        public ICommand VerDetalleStockCommand { get; }
        public ICommand ModificarModeloCommand { get; }
        public ICommand EliminarModeloCommand { get; }

        public InventarioViewModel()
        {
            _inventarioService = new InventarioService();
            ListaProductos = new ObservableCollection<ProductoViewModel>();

            // TODO: Reemplazar por _inventarioService.ObtenerTodos() cuando conectes la BD real
            var producto1 = new Producto { ID = 1, Codigo_Modelo = "NK-AF1-01", Nombre = "Air Force 1", Precio_Venta = 125000, Marca_ID = 1, Categoria_ID = 1, Activo = true };
            var producto2 = new Producto { ID = 2, Codigo_Modelo = "AD-SM-02", Nombre = "Samba OG", Precio_Venta = 110000, Marca_ID = 2, Categoria_ID = 1, Activo = true };

            ListaProductos.Add(new ProductoViewModel(producto1) { NombreMarca = "Nike", NombreCategoria = "Deportivo", StockTotal = 45 });
            ListaProductos.Add(new ProductoViewModel(producto2) { NombreMarca = "Adidas", NombreCategoria = "Deportivo", StockTotal = 12 });

            VistaFiltroProductos = CollectionViewSource.GetDefaultView(ListaProductos);
            VistaFiltroProductos.Filter = FiltrarCriterios;

            NuevoModeloCommand = new RelayCommand(AbrirVentanaNuevoModelo);
            VerDetalleStockCommand = new RelayCommand(AbrirVentanaDetalleStock);
            ModificarModeloCommand = new RelayCommand(AbrirVentanaModificar);
            EliminarModeloCommand = new RelayCommand(EliminarModelo);
        }

        private void AbrirVentanaNuevoModelo(object parametro)
        {
            var ventana = new Vistas.NuevoModeloWindow();
            var formViewModel = new NuevoModeloViewModel();

            formViewModel.CerrarVentana = () => ventana.Close();
            formViewModel.OnModeloGuardado = () =>
            {
                ListaProductos.Add(formViewModel.ProductoActual);
                VistaFiltroProductos.Refresh();
            };

            ventana.DataContext = formViewModel;
            ventana.ShowDialog();
        }

        private void AbrirVentanaModificar(object parametro)
        {
            if (parametro is ProductoViewModel productoFila)
            {
                // 1. Extraer la original
                var entidadOriginal = productoFila.ObtenerEntidadPura();

                // 2. Crear clon para proteger la tabla base
                var entidadClonada = new Producto
                {
                    ID = entidadOriginal.ID,
                    Codigo_Modelo = entidadOriginal.Codigo_Modelo,
                    Nombre = entidadOriginal.Nombre,
                    Marca_ID = entidadOriginal.Marca_ID,
                    Categoria_ID = entidadOriginal.Categoria_ID,
                    Precio_Venta = entidadOriginal.Precio_Venta,
                    Activo = entidadOriginal.Activo
                };

                var ventana = new Vistas.NuevoModeloWindow();
                var formViewModel = new NuevoModeloViewModel(entidadClonada); // Pasamos el clon

                formViewModel.CerrarVentana = () => ventana.Close();
                formViewModel.OnModeloGuardado = () =>
                {
                    // 3. Volcar los datos al confirmar
                    productoFila.Codigo_Modelo = entidadClonada.Codigo_Modelo;
                    productoFila.Nombre = entidadClonada.Nombre;
                    productoFila.Precio_Venta = entidadClonada.Precio_Venta;
                    productoFila.Marca_ID = entidadClonada.Marca_ID;
                    productoFila.Categoria_ID = entidadClonada.Categoria_ID;
                    productoFila.Activo = entidadClonada.Activo;

                    // Extraer los nombres de los combos actualizados
                    productoFila.NombreMarca = formViewModel.MarcaSeleccionada.Nombre;
                    productoFila.NombreCategoria = formViewModel.CategoriaSeleccionada.Nombre;

                    VistaFiltroProductos.Refresh();
                };

                ventana.DataContext = formViewModel;
                ventana.ShowDialog();
            }
        }

        private void AbrirVentanaDetalleStock(object parametro)
        {
            if (parametro is ProductoViewModel productoFila)
            {
                var detalleViewModel = new DetalleStockViewModel(productoFila);
                var ventana = new Vistas.DetalleStockWindow();

                detalleViewModel.CerrarVentana = () => ventana.Close();
                ventana.DataContext = detalleViewModel;
                ventana.ShowDialog();

                VistaFiltroProductos.Refresh();
            }
        }

        private void EliminarModelo(object parametro)
        {
            if (parametro is ProductoViewModel productoFila)
            {
                var respuesta = MessageBox.Show($"¿Estás seguro de que deseas eliminar permanentemente el modelo '{productoFila.Nombre}'?",
                                                "Confirmar Eliminación",
                                                MessageBoxButton.YesNo,
                                                MessageBoxImage.Warning);

                if (respuesta == MessageBoxResult.Yes)
                {
                    int idReal = productoFila.ObtenerEntidadPura().ID;
                    bool exito = _inventarioService.EliminarProducto(idReal, out string mensaje);

                    if (exito)
                    {
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