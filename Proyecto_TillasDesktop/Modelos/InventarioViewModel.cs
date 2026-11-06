// Importa las colecciones observables de WPF para actualizar automáticamente las grillas (DataGrid) al añadir o quitar elementos
using System.Collections.ObjectModel;
// Importa las interfaces de componentes y soporte de metadatos (necesario para ICollectionView)
using System.ComponentModel;
// Importa clases base de WPF para la gestión de la aplicación y cuadros de diálogo interactivos
using System.Windows;
// Importa las vistas de colección de WPF para permitir filtrado y ordenamiento avanzado sin romper los datos originales
using System.Windows.Data;
// Importa las utilidades de comandos y eventos de entrada de la interfaz de usuario en WPF
using System.Windows.Input;
// Importa los servicios de la Capa de Negocios encargados de gestionar las reglas de inventario, productos, marcas y categorías
using TillasDesktop.BLL.Services;
// Importa las entidades del dominio de inventario (como Producto)
using TillasDesktop.Entities.Inventario;

// Define el espacio de nombres (namespace) correspondiente a los modelos de vista de la interfaz gráfica del sistema
namespace TillasDesktop.UI.Modelos
{
    // Declara la clase pública InventarioViewModel que hereda de ViewModelBase, encargada de administrar el catálogo de productos, existencias, filtros y estados de actividad.
    public class InventarioViewModel : ViewModelBase
    {
        // Instancia de solo lectura del servicio que contiene la lógica de negocio y se comunica con la base de datos de inventario.
        private readonly InventarioService _inventarioService;

        // Campo privado que almacena el texto que va escribiendo el usuario en la barra de búsqueda en tiempo real.
        private string _textoBusqueda = string.Empty;

        // Bandera booleana privada para controlar si se están visualizando los productos inactivos (dados de baja) o activos
        private bool _mostrandoInactivos = false;

        // Campo privado que almacena el texto dinámico que tendrá el botón alternador de estado de inactivos
        private string _textoBotonInactivos = "Mostrar Inactivos";
        // Propiedad pública vinculada al botón para cambiar su texto según el modo activo/inactivo
        public string TextoBotonInactivos
        {
            get => _textoBotonInactivos;
            set { _textoBotonInactivos = value; OnPropertyChanged(); }
        }

        // Colección observable tipada con ProductoViewModel que notifica a la tabla visual (DataGrid) cuando se agregan, editan o eliminan productos.
        public ObservableCollection<ProductoViewModel> ListaProductos { get; set; }

        // Vista de colección especial (ICollectionView) que envuelve la lista para filtrar los datos en tiempo real sin romper ni modificar la colección original.
        public ICollectionView VistaFiltroProductos { get; set; }

        // Propiedad pública vinculada al cuadro de texto de búsqueda de la pantalla.
        public string TextoBusqueda
        {
            get => _textoBusqueda;
            set
            {
                _textoBusqueda = value;
                OnPropertyChanged();
                // Cada vez que el usuario teclea una letra, se le pide a la vista que vuelva a evaluar el método de filtrado.
                VistaFiltroProductos.Refresh();
            }
        }

        // Comandos públicos asociados a los botones principales de la interfaz (nuevo modelo, ver stock, modificar, eliminar, marcas, categorías y alternar inactivos).
        public ICommand NuevoModeloCommand { get; }
        public ICommand VerDetalleStockCommand { get; }
        public ICommand ModificarModeloCommand { get; }
        public ICommand EliminarModeloCommand { get; }
        public ICommand AdministrarMarcasCommand { get; }
        public ICommand AdministrarCategoriasCommand { get; }
        public ICommand ToggleInactivosCommand { get; }

        // Constructor principal: inicializa el servicio, levanta los datos iniciales, configura los filtros de colección y arma los comandos.
        public InventarioViewModel()
        {
            // Instancia el servicio de inventario de la capa de negocios
            _inventarioService = new InventarioService();
            // Inicializa la colección observable principal
            ListaProductos = new ObservableCollection<ProductoViewModel>();

            // Vinculamos cada comando con su respectivo método delegado de acción
            NuevoModeloCommand = new RelayCommand(AbrirVentanaNuevoModelo);
            VerDetalleStockCommand = new RelayCommand(AbrirVentanaDetalleStock);
            ModificarModeloCommand = new RelayCommand(AbrirVentanaModificar);
            EliminarModeloCommand = new RelayCommand(EliminarModelo);
            AdministrarMarcasCommand = new RelayCommand(AbrirVentanaMarcas);
            AdministrarCategoriasCommand = new RelayCommand(AbrirVentanaCategorias);
            ToggleInactivosCommand = new RelayCommand(AlternarInactivos);

            // Configura la vista de filtrado basándose en la lista observable de productos
            VistaFiltroProductos = CollectionViewSource.GetDefaultView(ListaProductos);
            VistaFiltroProductos.Filter = FiltrarCriterios;

            // Carga los productos desde la base de datos al iniciar
            CargarProductos();
        }

        // Método privado que consulta los productos, marcas y categorías desde la capa de negocios y los prepara en la pantalla.
        private void CargarProductos()
        {
            // Limpia la colección existente por seguridad
            ListaProductos.Clear();

            // Pasamos '!_mostrandoInactivos' para cumplir con el contrato de la BLL: si estamos mostrando inactivos, solicita activos = false
            var productosDesdeDb = _inventarioService.ObtenerProductos(!_mostrandoInactivos);
            var marcasDb = _inventarioService.ObtenerMarcasActivas();
            var categoriasDb = _inventarioService.ObtenerCategoriasActivas();

            // Recorremos cada producto obtenido de la base de datos
            foreach (var prod in productosDesdeDb)
            {
                // Resuelve el nombre de la marca mediante LINQ o asigna un valor por defecto si no existe
                string nombreMarca = marcasDb.FirstOrDefault(m => m.ID == prod.Marca_ID)?.Nombre ?? "Sin Marca";
                // Resuelve el nombre de la categoría mediante LINQ o asigna un valor por defecto
                string nombreCategoria = categoriasDb.FirstOrDefault(c => c.ID == prod.Categoria_ID)?.Nombre ?? "Sin Categoría";

                // Consulta los talles asociados y calcula el stock total acumulado sumando las existencias de cada talle
                var talles = _inventarioService.ObtenerTallesPorProducto(prod.ID);
                int stockCalculado = talles.Sum(t => t.Stock_Actual);

                // Envolvemos el producto en un ProductoViewModel asignando sus propiedades calculadas y descriptivas
                ListaProductos.Add(new ProductoViewModel(prod)
                {
                    NombreMarca = nombreMarca,
                    NombreCategoria = nombreCategoria,
                    StockTotal = stockCalculado
                });
            }

            // Refresca la vista de filtrado para actualizar la grilla visual
            VistaFiltroProductos?.Refresh();
        }

        // Método privado que alterna el estado de visualización entre productos activos e inactivos
        private void AlternarInactivos(object parametro)
        {
            // Invierte el valor booleano del indicador de inactivos
            _mostrandoInactivos = !_mostrandoInactivos;
            // Actualiza el texto del botón según el estado actual
            TextoBotonInactivos = _mostrandoInactivos ? "Mostrar Activos" : "Mostrar Inactivos";
            // Recarga la lista de productos aplicando el nuevo filtro de estado
            CargarProductos();
        }

        // Método privado que abre el formulario en blanco para dar de alta un nuevo modelo de zapatilla
        private void AbrirVentanaNuevoModelo(object parametro)
        {
            var ventana = new Vistas.NuevoModeloWindow();
            var formViewModel = new NuevoModeloViewModel();

            // Usamos delegados (Actions) para que el formulario hijo pueda cerrar su propia ventana 
            // y avisarle a este ViewModel padre que recargue la tabla si el guardado fue exitoso.
            formViewModel.CerrarVentana = () => ventana.Close();
            formViewModel.OnModeloGuardado = () =>
            {
                CargarProductos();
            };

            ventana.DataContext = formViewModel;
            ventana.ShowDialog();
        }

        // Método privado que abre el formulario cargando los datos del modelo seleccionado en la grilla para permitir su modificación.
        private void AbrirVentanaModificar(object parametro)
        {
            // Verificamos que el botón clickeado haya enviado un producto válido desde la grilla.
            if (parametro is ProductoViewModel productoFila)
            {
                // Extraemos la entidad original pura que está en esa fila.
                var entidadOriginal = productoFila.ObtenerEntidadPura();

                // TÉCNICA DE CLONACIÓN: Se crea una copia exacta. Esto asegura que si el usuario 
                // abre la ventana de edición, modifica datos y luego presiona la "X" sin guardar, 
                // la grilla principal no refleje alteraciones fantasmas en memoria.
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
                    // Al tocar las propiedades del ViewModel (productoFila), salta el OnPropertyChanged y la pantalla se actualiza de inmediato.
                    productoFila.Codigo_Modelo = entidadClonada.Codigo_Modelo;
                    productoFila.Nombre = entidadClonada.Nombre;
                    productoFila.Precio_Venta = entidadClonada.Precio_Venta;
                    productoFila.Marca_ID = entidadClonada.Marca_ID;
                    productoFila.Categoria_ID = entidadClonada.Categoria_ID;
                    productoFila.Activo = entidadClonada.Activo;

                    // También actualizamos los nombres en texto de la marca y categoría para que se reflejen correctamente en la tabla.
                    productoFila.NombreMarca = formViewModel.MarcaSeleccionada.Nombre;
                    productoFila.NombreCategoria = formViewModel.CategoriaSeleccionada.Nombre;
                    CargarProductos();
                };

                ventana.DataContext = formViewModel;
                ventana.ShowDialog();
            }
        }

        // Método privado que abre la ventana secundaria para administrar los talles y stock físico de un modelo específico.
        private void AbrirVentanaDetalleStock(object parametro)
        {
            // Valida que el parámetro recibido corresponda a un producto válido de la grilla
            if (parametro is ProductoViewModel productoFila)
            {
                var detalleViewModel = new DetalleStockViewModel(productoFila);
                var ventana = new Vistas.DetalleStockWindow();

                detalleViewModel.CerrarVentana = () => ventana.Close();
                ventana.DataContext = detalleViewModel;
                ventana.ShowDialog();

                // Recarga los productos al cerrar la ventana de stock para reflejar las sumas actualizadas
                CargarProductos();
            }
        }

        // Método privado que abre la ventana de gestión independiente de marcas.
        private void AbrirVentanaMarcas(object parametro)
        {
            var ventana = new Vistas.GestionMarcasWindow();
            ventana.DataContext = new GestionMarcasViewModel();
            ventana.ShowDialog();
            CargarProductos();
        }

        // Método privado que abre la ventana de gestión independiente de categorías.
        private void AbrirVentanaCategorias(object parametro)
        {
            var ventana = new Vistas.GestionCategoriasWindow();
            ventana.DataContext = new GestionCategoriasViewModel();
            ventana.ShowDialog();
            CargarProductos();
        }

        // Método privado que gestiona la baja lógica o la eliminación permanente de un modelo de producto según su estado actual.
        private void EliminarModelo(object parametro)
        {
            if (parametro is ProductoViewModel productoFila)
            {
                int idReal = productoFila.ObtenerEntidadPura().ID;

                // Si el producto está ACTIVO -> Hacemos BAJA LÓGICA (Desactivar)
                if (productoFila.Activo)
                {
                    var respuesta = MessageBox.Show(
                        $"¿Estás seguro de dar de baja el modelo '{productoFila.Nombre}'?\nEl producto dejará de estar disponible en el sistema.",
                        "Confirmar Baja Lógica",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (respuesta == MessageBoxResult.Yes)
                    {
                        bool exito = _inventarioService.EliminarProducto(idReal, out string mensaje);

                        if (exito)
                        {
                            MessageBox.Show(mensaje, "Operación Exitosa", MessageBoxButton.OK, MessageBoxImage.Information);
                            CargarProductos();
                        }
                        else
                        {
                            MessageBox.Show(mensaje, "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                }
                // Si el producto está INACTIVO -> Hacemos BAJA FÍSICA (Eliminar Permanentemente de la base de datos)
                else
                {
                    var respuesta = MessageBox.Show(
                        $"¿Estás seguro de ELIMINAR PERMANENTEMENTE el modelo '{productoFila.Nombre}'?\nEsta acción no se puede deshacer.",
                        "Confirmar Eliminación Permanente",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);

                    if (respuesta == MessageBoxResult.Yes)
                    {
                        bool exito = _inventarioService.EliminarPermanenteProducto(idReal, out string mensaje);

                        if (exito)
                        {
                            MessageBox.Show(mensaje, "Eliminación Completada", MessageBoxButton.OK, MessageBoxImage.Information);
                            CargarProductos();
                        }
                        else
                        {
                            MessageBox.Show(mensaje, "No se pudo eliminar", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
        }

        // Método delegado que evalúa fila por fila del DataGrid para verificar si coincide con el texto escrito en la barra de búsqueda.
        private bool FiltrarCriterios(object obj)
        {
            if (obj is ProductoViewModel producto)
            {
                // Si la barra de búsqueda está vacía, dejamos pasar a todos los productos sin restricciones.
                if (string.IsNullOrWhiteSpace(TextoBusqueda)) return true;

                // Pasamos el texto a minúsculas para garantizar que la búsqueda no distinga entre mayúsculas y minúsculas.
                string filtro = TextoBusqueda.ToLower();

                // Devuelve true (muestra la fila en la grilla) si el filtro coincide parcialmente con el Nombre, el Código de Modelo o el Nombre de la Marca.
                return (producto.Nombre != null && producto.Nombre.ToLower().Contains(filtro)) ||
                       (producto.Codigo_Modelo != null && producto.Codigo_Modelo.ToLower().Contains(filtro)) ||
                       (producto.NombreMarca != null && producto.NombreMarca.ToLower().Contains(filtro));
            }
            return false; // Si el objeto evaluado no es un producto válido, lo ocultamos de la grilla.
        }
    }
}