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

        /// Campo privado que almacena el texto que va escribiendo el usuario en el buscador.
        private string _textoBusqueda = string.Empty;

        // Colección observable que notifica a la tabla visual (DataGrid) cuando se agregan o eliminan productos.
        public ObservableCollection<ProductoViewModel> ListaProductos { get; set; }

        // Vista de colección especial que envuelve la lista para filtrar los datos en tiempo real sin romper la colección original.
        public ICollectionView VistaFiltroProductos { get; set; }

        // Propiedad pública vinculada al cuadro de texto de búsqueda de la pantalla.
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

        // Comandos asociados a los botones principales de la interfaz (nuevo, ver stock, modificar, eliminar).
        public ICommand NuevoModeloCommand { get; }
        public ICommand VerDetalleStockCommand { get; }
        public ICommand ModificarModeloCommand { get; }
        public ICommand EliminarModeloCommand { get; }

        // Constructor principal: inicializa el servicio, levanta los datos y arma los comandos.
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

            // Configuramos la vista de filtrado tomando como base la lista observable de productos.
            VistaFiltroProductos = CollectionViewSource.GetDefaultView(ListaProductos);

            // Asignamos el método que decide qué filas se muestran y cuáles se ocultan según la búsqueda.
            VistaFiltroProductos.Filter = FiltrarCriterios;

            // Inicialización de comandos vinculados a sus respectivos métodos.
            NuevoModeloCommand = new RelayCommand(AbrirVentanaNuevoModelo);
            VerDetalleStockCommand = new RelayCommand(AbrirVentanaDetalleStock);
            ModificarModeloCommand = new RelayCommand(AbrirVentanaModificar);
            EliminarModeloCommand = new RelayCommand(EliminarModelo);
        }

        // Abre el formulario en blanco para dar de alta un nuevo modelo de zapatilla
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

        // Abre el formulario cargando los datos del modelo que seleccionamos en la grilla para modificarlo.
        private void AbrirVentanaModificar(object parametro)
        {
            // Verificamos que el botón clickeado haya enviado un producto válido desde la grilla.
            if (parametro is ProductoViewModel productoFila)
            {
                // Extraemos la entidad original pura que está en esa fila.
                var entidadOriginal = productoFila.ObtenerEntidadPura();

                //  Hacemos una copia exacta. Esto asegura que si el usuario 
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
                // Le pasamos el clon al ViewModel del formulario de edición.
                var formViewModel = new NuevoModeloViewModel(entidadClonada);


                formViewModel.CerrarVentana = () => ventana.Close();
                formViewModel.OnModeloGuardado = () =>
                {
                    // Si se guardó bien, pasamos los datos modificados del clon de regreso a la fila original de la tabla.
                    // Al tocar las propiedades del ViewModel (productoFila), salta el OnPropertyChanged y la pantalla se actualiza al toque.
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

        // Abre la ventana secundaria para administrar los talles y stock físico de un modelo específico.
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

                // Al cerrar la ventana de stock, refrescamos la tabla principal por si hubo cambios en las cantidades totales.
                VistaFiltroProductos.Refresh();
            }
        }

        // Da de baja un modelo de zapatilla tras pedir confirmación por seguridad.
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
                        /// Si la base de datos confirmó la baja, lo sacamos de nuestra lista y la tabla se actualiza sola.
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
                // Si la barra está vacía, dejamos pasar a todos los productos sin filtrar.
                if (string.IsNullOrWhiteSpace(TextoBusqueda)) return true;

                // Pasamos el texto a minúsculas para que no importe si buscan con mayúsculas o minúsculas.
                string filtro = TextoBusqueda.ToLower();

                // Devuelve true (muestra la fila) si el filtro coincide con el Nombre, el Código o la Marca.
                return (producto.Nombre != null && producto.Nombre.ToLower().Contains(filtro)) ||
                       (producto.Codigo_Modelo != null && producto.Codigo_Modelo.ToLower().Contains(filtro)) ||
                       (producto.NombreMarca != null && producto.NombreMarca.ToLower().Contains(filtro));
            }
            return false; // Si no es un producto válido, lo ocultamos.
        }
    }
}