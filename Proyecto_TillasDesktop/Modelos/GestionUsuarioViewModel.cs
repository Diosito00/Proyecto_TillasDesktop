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
// Importa los servicios de la Capa de Negocios encargados de gestionar las reglas y operaciones de usuarios
using TillasDesktop.BLL.Services;
// Importa las vistas de la interfaz de usuario de la aplicación (formularios de usuario)
using TillasDesktop.UI.Vistas;
// Importa las entidades del dominio de usuarios
using TillasDesktop.Entities.Usuarios;

// Define el espacio de nombres (namespace) correspondiente al modelo de vistas de la interfaz gráfica del sistema
namespace TillasDesktop.UI.Modelos
{
    // Declara la clase pública GestionUsuarioViewModel que hereda de ViewModelBase, encargada de administrar la lista, filtrado, altas, ediciones, bajas lógicas y bajas físicas de operadores.
    public class GestionUsuarioViewModel : ViewModelBase
    {
        // Instancia de solo lectura del servicio que contiene las reglas de negocio y conexión a datos de usuarios.
        private readonly UsuariosService _usuarioService;

        // Campo privado que almacena la colección observable de usuarios envueltos en ViewModels.
        private ObservableCollection<UsuarioViewModel> _listaUsuarios;
        // Propiedad pública ObservableCollection: notifica automáticamente a la grilla de la interfaz cuando un elemento se añade o se elimina.
        public ObservableCollection<UsuarioViewModel> ListaUsuarios
        {
            get => _listaUsuarios;
            set { _listaUsuarios = value; OnPropertyChanged(); }
        }

        // Campo privado que almacena el texto ingresado en la barra de búsqueda en tiempo real.
        private string _textoBusqueda = string.Empty;
        // Propiedad pública bindeada a la barra de búsqueda de la interfaz. Al cambiar, refresca la vista filtrada automáticamente.
        public string TextoBusqueda
        {
            get => _textoBusqueda;
            set
            {
                _textoBusqueda = value;
                OnPropertyChanged();
                VistaFiltroUsuarios?.Refresh(); // Dispara la reevaluación del método de filtrado por cada fila en tiempo real.
            }
        }

        // Campo privado para controlar el estado del interruptor (switch) o botón de mostrar registros inactivos
        private bool _mostrarInactivos;
        // Propiedad pública para la gestión de visualización de usuarios dados de baja
        public bool MostrarInactivos
        {
            get => _mostrarInactivos;
            set
            {
                // Solo recargamos si el usuario realmente cambió el estado del interruptor
                if (_mostrarInactivos != value)
                {
                    _mostrarInactivos = value;
                    OnPropertyChanged();
                    CargarDatos(); // Recarga la tabla automáticamente al cambiar el switch de inactivos
                }
            }
        }

        // Una vista especial (ICollectionView) que envuelve nuestra lista para poder filtrar y ordenar dinámicamente sin alterar los datos originales subyacentes.
        public ICollectionView VistaFiltroUsuarios { get; set; }

        // Comandos públicos vinculados a los botones de la interfaz para las acciones principales.
        public ICommand NuevoUsuarioCommand { get; }
        public ICommand EditarCommand { get; }
        public ICommand EliminarCommand { get; }

        // Constructor principal que inicializa el ViewModel, servicios, vistas de filtrado y comandos.
        public GestionUsuarioViewModel()
        {
            // Instancia el servicio de usuarios de la capa de negocios
            _usuarioService = new UsuariosService();

            // Configura la vista de filtrado basándose en la lista observable principal.
            ListaUsuarios = new ObservableCollection<UsuarioViewModel>();
            VistaFiltroUsuarios = CollectionViewSource.GetDefaultView(ListaUsuarios);
            VistaFiltroUsuarios.Filter = FiltrarCriteriosUsuarios; // Asigna el método delegado que evalúa fila por fila.

            // Carga los datos iniciales desde la base de datos
            CargarDatos();

            // Vinculamos cada comando con su respectivo método delegado de acción.
            NuevoUsuarioCommand = new RelayCommand(EjecutarNuevo);
            EditarCommand = new RelayCommand(EjecutarEditar);
            EliminarCommand = new RelayCommand(EjecutarEliminar);
        }

        // Método privado que trae los usuarios desde la base de datos y los envuelve para prepararlos en la pantalla.
        private void CargarDatos()
        {
            try
            {
                // Limpia la colección existente por seguridad antes de poblarla
                ListaUsuarios.Clear();

                // Pide las entidades a la BLL. (!MostrarInactivos invierte el booleano para cumplir con el criterio de filtrado SQL en el servicio)
                var usuariosDeDb = _usuarioService.ObtenerTodos(!MostrarInactivos);

                // Envolvemos cada usuario en un UsuarioViewModel para que la interfaz pueda mostrarlos y enlazarlos correctamente.
                foreach (var usuario in usuariosDeDb)
                {
                    ListaUsuarios.Add(new UsuarioViewModel(usuario));
                }

                // Refresca la vista de filtrado para reflejar los nuevos datos cargados
                VistaFiltroUsuarios?.Refresh();
            }
            catch (Exception ex)
            {
                // Muestra un mensaje de error detallado si falla la conexión o la consulta a la base de datos
                MessageBox.Show($"Error al cargar los usuarios desde la base de datos: {ex.Message}",
                                    "Error de Conexión", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Método delegado ejecutado por la vista de filtrado por cada fila de la grilla de datos.
        private bool FiltrarCriteriosUsuarios(object obj)
        {
            // Verifica que el objeto recibido corresponda a un UsuarioViewModel válido
            if (obj is UsuarioViewModel usuarioFila)
            {
                // Si el buscador está vacío o en blanco, muestra todas las filas sin restricciones.
                if (string.IsNullOrWhiteSpace(TextoBusqueda)) return true;

                // Convierte la cadena de búsqueda a minúsculas para garantizar que no sea sensible a mayúsculas/minúsculas.
                string filtro = TextoBusqueda.ToLower();

                // Retorna true si el texto de búsqueda coincide de forma parcial con el nombre, apellido, nombre de usuario, email o DNI.
                return (usuarioFila.Nombre != null && usuarioFila.Nombre.ToLower().Contains(filtro)) ||
                       (usuarioFila.Apellido != null && usuarioFila.Apellido.ToLower().Contains(filtro)) ||
                       (usuarioFila.Nombre_Usuario != null && usuarioFila.Nombre_Usuario.ToLower().Contains(filtro)) ||
                       (usuarioFila.Email != null && usuarioFila.Email.ToLower().Contains(filtro)) ||
                       (usuarioFila.Dni != null && usuarioFila.Dni.ToLower().Contains(filtro));
            }
            return false; // Oculta la fila de la grilla si no hay coincidencias o el objeto no es válido.
        }

        // Método privado que abre el formulario en blanco para dar de alta un usuario nuevo.
        private void EjecutarNuevo(object obj)
        {
            // Instancia la ventana visual del formulario y el ViewModel correspondiente para un nuevo registro
            var ventana = new FormularioUsuarioWindow();
            var viewModel = new FormularioUsuarioViewModel();

            // Configura acciones (delegados) para que el formulario se pueda cerrar de forma limpia y avise cuando guarde con éxito.
            viewModel.CerrarVentana = () => ventana.Close();
            viewModel.OnUsuarioGuardado = () =>
            {
                // Agrega el nuevo usuario a la colección observable para reflejarlo en la grilla principal.
                ListaUsuarios.Add(viewModel.UsuarioActual);
                VistaFiltroUsuarios.Refresh();
            };

            // Asigna el DataContext y abre la ventana en modalidad modal bloqueante
            ventana.DataContext = viewModel;
            ventana.ShowDialog(); // Abre la ventana y frena la ejecución hasta que la cierren.
        }

        // Método privado que abre el formulario cargando los datos del usuario seleccionado en la grilla para permitir su modificación.
        private void EjecutarEditar(object obj)
        {
            // El objeto llega desde el botón de la fila en la interfaz. Nos aseguramos de que sea un UsuarioViewModel válido.
            if (obj is UsuarioViewModel usuarioFila)
            {
                // Extrae la entidad pura subyacente para obtener los datos originales.
                var entidadOriginal = usuarioFila.ObtenerEntidadPura();

                // TÉCNICA DE CLONACIÓN: Se crea una instancia completamente nueva con los mismos valores de propiedad. 
                // Esto previene la "edición fantasma" en la interfaz si el usuario decide cancelar la operación a mitad de camino.
                var entidadClonada = new Usuario
                {
                    Id_Usuario = entidadOriginal.Id_Usuario,
                    Nombre = entidadOriginal.Nombre,
                    Apellido = entidadOriginal.Apellido,
                    Dni = entidadOriginal.Dni,
                    Email = entidadOriginal.Email,
                    Nombre_Usuario = entidadOriginal.Nombre_Usuario,
                    Password = entidadOriginal.Password,
                    Fecha_Nacimiento = entidadOriginal.Fecha_Nacimiento,
                    Rol = entidadOriginal.Rol,
                    Activo = entidadOriginal.Activo
                };

                // Instancia la ventana de formulario
                var ventana = new FormularioUsuarioWindow();

                // Se le pasa el CLON al ViewModel del formulario en lugar de la entidad original para aislar los cambios temporales.
                var viewModel = new FormularioUsuarioViewModel(entidadClonada);

                // Configura el delegado para cerrar la ventana
                viewModel.CerrarVentana = () => ventana.Close();

                // Configura el callback que se ejecuta al confirmar el guardado exitoso
                viewModel.OnUsuarioGuardado = () =>
                {
                    // Si la base de datos confirma el éxito, copiamos los datos del clon de vuelta a la fila original.
                    // Al usar las propiedades del ViewModel (usuarioFila), se dispara OnPropertyChanged() y la grilla se actualiza en pantalla de inmediato.
                    usuarioFila.Nombre = entidadClonada.Nombre;
                    usuarioFila.Apellido = entidadClonada.Apellido;
                    usuarioFila.Dni = entidadClonada.Dni;
                    usuarioFila.Email = entidadClonada.Email;
                    usuarioFila.Nombre_Usuario = entidadClonada.Nombre_Usuario;
                    usuarioFila.Fecha_Nacimiento = entidadClonada.Fecha_Nacimiento;
                    usuarioFila.Rol = entidadClonada.Rol;
                    usuarioFila.Activo = entidadClonada.Activo;

                    // Refresca la vista de filtrado
                    VistaFiltroUsuarios.Refresh();
                };

                // Asigna el DataContext y abre la ventana modal de edición
                ventana.DataContext = viewModel;
                ventana.ShowDialog();
            }
        }

        // Método privado que gestiona la baja lógica o física del usuario seleccionado según su estado actual, tras pedir confirmación por seguridad.
        private void EjecutarEliminar(object obj)
        {
            // Valida que el parámetro recibido corresponda a un usuario válido de la grilla
            if (obj is UsuarioViewModel usuarioFila)
            {
                // Si el usuario se encuentra Activo -> Ejecutamos Baja Lógica
                if (usuarioFila.Activo)
                {
                    var respuesta = MessageBox.Show($"¿Dar de baja al usuario '{usuarioFila.Nombre} {usuarioFila.Apellido}'?",
                                                   "Baja de Usuario", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (respuesta == MessageBoxResult.Yes)
                    {
                        try
                        {
                            // Mandamos la orden de baja lógica a la base de datos a través del servicio.
                            if (_usuarioService.EliminarUsuario(usuarioFila.Id_Usuario))
                            {
                                // Si se completa con éxito, removemos el elemento de la colección visual local y refrescamos
                                ListaUsuarios.Remove(usuarioFila);
                                VistaFiltroUsuarios.Refresh();
                            }
                        }
                        catch (Exception ex) { MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
                    }
                }
                // Si el usuario ya se encuentra Inactivo -> Ejecutamos Baja Física (Eliminación Definitiva)
                else
                {
                    var respuesta = MessageBox.Show($"ATENCIÓN: ¿Deseas eliminar PERMANENTEMENTE a '{usuarioFila.Nombre}' de la base de datos? Esta acción no se puede deshacer.",
                                                   "Eliminación Definitiva", MessageBoxButton.YesNo, MessageBoxImage.Error);
                    if (respuesta == MessageBoxResult.Yes)
                    {
                        try
                        {
                            // Mandamos la orden de baja física definitiva a la base de datos a través del servicio.
                            if (_usuarioService.EliminarUsuarioFisico(usuarioFila.Id_Usuario))
                            {
                                // Si se elimina físicamente, lo removemos de la colección visual y refrescamos
                                ListaUsuarios.Remove(usuarioFila);
                                VistaFiltroUsuarios.Refresh();
                            }
                        }
                        catch (Exception ex) { MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
                    }
                }
            }
        }
    }
}