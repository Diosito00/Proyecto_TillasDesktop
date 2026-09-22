using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using TillasDesktop.BLL.Services;
using TillasDesktop.Entities.Inventario;

namespace TillasDesktop.UI.Modelos
{
    // Hereda de ViewModelBase para mantener la interfaz gráfica sincronizada con los datos.
    public class InventarioViewModel : ViewModelBase
    {
        // Servicio que contiene la lógica de negocio y se comunica con la base de datos.
        private readonly InventarioService _inventarioService;

        // Propiedad privada y pública para la barra de búsqueda.
        private string _textoBusqueda = string.Empty;

        // Colección observable que notifica a la tabla visual (DataGrid) cuando se agregan o eliminan productos.
        public ObservableCollection<ProductoViewModel> ListaProductos { get; set; }

        // Interfaz que envuelve la lista para permitir filtrado sin modificar la colección original.
        public ICollectionView VistaFiltroProductos { get; set; }

        public string TextoBusqueda
        {
            get => _textoBusqueda;
            set
            {
                _textoBusqueda = value;
                OnPropertyChanged();
                // Cada vez que el usuario teclea una letra, se le pide a la vista que vuelva a evaluar el filtro.
                VistaFiltroProductos.Refresh();
            }
        }

        // Comandos que se enlazan a los botones de la interfaz gráfica.
        public ICommand NuevoModeloCommand { get; }
        public ICommand VerDetalleStockCommand { get; }
        public ICommand ModificarModeloCommand { get; }
        public ICommand EliminarModeloCommand { get; }

        public InventarioViewModel()
        {
            _inventarioService = new InventarioService();
            ListaProductos = new ObservableCollection<ProductoViewModel>();

            // Actualmente tiene datos hardcodeados para poder diseñar la pantalla. 
            // En el futuro, esto se reemplazará por una llamada a la base de datos real.
            var producto1 = new Producto { ID = 1, Codigo_Modelo = "NK-AF1-01", Nombre = "Air Force 1", Precio_Venta = 125000, Marca_ID = 1, Categoria_ID = 1, Activo = true };
            var producto2 = new Producto { ID = 2, Codigo_Modelo = "AD-SM-02", Nombre = "Samba OG", Precio_Venta = 110000, Marca_ID = 2, Categoria_ID = 1, Activo = true };

            ListaProductos.Add(new ProductoViewModel(producto1) { NombreMarca = "Nike", NombreCategoria = "Deportivo", StockTotal = 45 });
            ListaProductos.Add(new ProductoViewModel(producto2) { NombreMarca = "Adidas", NombreCategoria = "Deportivo", StockTotal = 25 });

            // Configuramos la vista de filtrado basándonos en la lista observable.
            VistaFiltroProductos = CollectionViewSource.GetDefaultView(ListaProductos);
            VistaFiltroProductos.Filter = FiltrarCriterios; // Le asignamos el método que decide qué filas se ven.

            // Inicialización de comandos vinculados a sus respectivos métodos.
            NuevoModeloCommand = new RelayCommand(AbrirVentanaNuevoModelo);
            VerDetalleStockCommand = new RelayCommand(AbrirVentanaDetalleStock);
            ModificarModeloCommand = new RelayCommand(AbrirVentanaModificar);
            EliminarModeloCommand = new RelayCommand(EliminarModelo);
        }

        private void AbrirVentanaNuevoModelo(object parametro)
        {
            var ventana = new Vistas.NuevoModeloWindow();
            var formViewModel = new NuevoModeloViewModel();

            // Usamos delegados (Actions) para que el formulario hijo pueda cerrar su propia ventana 
            // y avisarle a este ViewModel padre que agregue el producto a la tabla si se guardó con éxito.
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
            // Verificamos que el botón clickeado haya enviado un producto válido desde la grilla.
            if (parametro is ProductoViewModel productoFila)
            {
                // 1. Extraemos la entidad original pura.
                var entidadOriginal = productoFila.ObtenerEntidadPura();

                // 2. Hacemos una copia exacta. Esto asegura que si el usuario 
                // abre la ventana de edición, borra el nombre y luego presiona la "X" (sin guardar), 
                // la grilla principal no muestre el producto con el nombre borrado.
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
                var formViewModel = new NuevoModeloViewModel(entidadClonada); // Le pasamos el clon al formulario.

                formViewModel.CerrarVentana = () => ventana.Close();
                formViewModel.OnModeloGuardado = () =>
                {
                    // 3. Si el guardado fue exitoso, copiamos los datos del clon 
                    // de regreso a la fila original. Como 'productoFila' es un ViewModel, 
                    // disparará 'OnPropertyChanged' y actualizará la UI al instante.
                    productoFila.Codigo_Modelo = entidadClonada.Codigo_Modelo;
                    productoFila.Nombre = entidadClonada.Nombre;
                    productoFila.Precio_Venta = entidadClonada.Precio_Venta;
                    productoFila.Marca_ID = entidadClonada.Marca_ID;
                    productoFila.Categoria_ID = entidadClonada.Categoria_ID;
                    productoFila.Activo = entidadClonada.Activo;

                    // También actualizamos los nombres en texto de la marca y categoría para que se vean en la tabla.
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
            // Abre la ventana secundaria para gestionar los talles y cantidades de un modelo específico.
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
                // Mensaje de advertencia para evitar borrados por accidente.
                var respuesta = MessageBox.Show($"¿Estás seguro de que deseas eliminar permanentemente el modelo '{productoFila.Nombre}'?",
                                                "Confirmar Eliminación",
                                                MessageBoxButton.YesNo,
                                                MessageBoxImage.Warning);

                if (respuesta == MessageBoxResult.Yes)
                {
                    // Delegamos la eliminación física o lógica a la capa de servicios.
                    int idReal = productoFila.ObtenerEntidadPura().ID;
                    bool exito = _inventarioService.EliminarProducto(idReal, out string mensaje);

                    if (exito)
                    {
                        // Si la BD confirmó el borrado, lo quitamos de la UI.
                        ListaProductos.Remove(productoFila);
                    }
                    else
                    {
                        MessageBox.Show(mensaje, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        // Método que evalúa cada fila del DataGrid para ver si coincide con lo escrito en la barra de búsqueda.
        private bool FiltrarCriterios(object obj)
        {
            if (obj is ProductoViewModel producto)
            {
                if (string.IsNullOrWhiteSpace(TextoBusqueda)) return true;

                string filtro = TextoBusqueda.ToLower();
                // Retorna verdadero si el texto coincide con el Nombre, el Código SKU o la Marca del producto.
                return (producto.Nombre != null && producto.Nombre.ToLower().Contains(filtro)) ||
                       (producto.Codigo_Modelo != null && producto.Codigo_Modelo.ToLower().Contains(filtro)) ||
                       (producto.NombreMarca != null && producto.NombreMarca.ToLower().Contains(filtro));
            }
            return false;
        }
    }
}