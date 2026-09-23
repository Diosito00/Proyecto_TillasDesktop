using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using TillasDesktop.BLL.Services;
using TillasDesktop.UI.Vistas;
using TillasDesktop.Entities.Usuarios;

namespace TillasDesktop.UI.Modelos
{
    // Hereda de ViewModelBase para mantener la interfaz conectada y reactiva (avisando cuando cambian los datos).
    public class GestionUsuarioViewModel : ViewModelBase
    {
        // Instancia del servicio que contiene las reglas de negocio y conexión a datos.
        private readonly UsuariosService _usuarioService;

        // ObservableCollection notifica automáticamente a la grilla de la interfaz cuando un elemento se añade o elimina.
        private ObservableCollection<UsuarioViewModel> _listaUsuarios;
        public ObservableCollection<UsuarioViewModel> ListaUsuarios
        {
            get => _listaUsuarios;
            set { _listaUsuarios = value; OnPropertyChanged(); }
        }

        // Propiedad bindeada (Binding) a la barra de búsqueda. Al cambiar, refresca la vista filtrada automáticamente.
        private string _textoBusqueda = string.Empty;
        public string TextoBusqueda
        {
            get => _textoBusqueda;
            set
            {
                _textoBusqueda = value;
                OnPropertyChanged();
                VistaFiltroUsuarios?.Refresh(); // Dispara la reevaluación del filtro.
            }
        }

        // Una vista especial que envuelve nuestra lista para poder filtrar y ordenar sin romper los datos originales.
        public ICollectionView VistaFiltroUsuarios { get; set; }

        // Comandos vinculados a los botones de la interfaz.
        public ICommand NuevoUsuarioCommand { get; }
        public ICommand EditarCommand { get; }
        public ICommand EliminarCommand { get; }

        public GestionUsuarioViewModel()
        {
            _usuarioService = new UsuariosService();

            // Cargamos todos los usuarios apenas se abre la ventana.
            CargarDatos();

            // Configura la vista de filtrado basándose en la lista observable principal.
            VistaFiltroUsuarios = CollectionViewSource.GetDefaultView(ListaUsuarios);
            VistaFiltroUsuarios.Filter = FiltrarCriteriosUsuarios; // Asigna el método que evalúa cada fila.

            // Vinculamos cada comando con su respectivo método.
            NuevoUsuarioCommand = new RelayCommand(EjecutarNuevo);
            EditarCommand = new RelayCommand(EjecutarEditar);
            EliminarCommand = new RelayCommand(EjecutarEliminar);
        }

        // Trae los usuarios de la base de datos y los prepara para la pantalla.
        private void CargarDatos()
        {
            ListaUsuarios = new ObservableCollection<UsuarioViewModel>();

            try
            {
                // Pedimos la lista pura a la capa de servicios.
                var usuariosDeDb = _usuarioService.ObtenerTodos();

                // Envolvemos cada usuario en un ViewModel para que la interfaz pueda mostrarlos y editarlos bien.
                foreach (var usuario in usuariosDeDb)
                {
                    ListaUsuarios.Add(new UsuarioViewModel(usuario));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar los usuarios desde la base de datos: {ex.Message}",
                                "Error de Conexión", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Método ejecutado por la vista de filtrado por cada fila de la grilla.
        private bool FiltrarCriteriosUsuarios(object obj)
        {
            if (obj is UsuarioViewModel usuarioFila)
            {
                // Si el buscador está vacío, muestra todas las filas.
                if (string.IsNullOrWhiteSpace(TextoBusqueda)) return true;

                // Convierte la búsqueda a minúsculas para que no sea sensible a mayúsculas.
                string filtro = TextoBusqueda.ToLower();

                // Retorna true si el texto coincide con el nombre, apellido, usuario, email o DNI.
                return (usuarioFila.Nombre != null && usuarioFila.Nombre.ToLower().Contains(filtro)) ||
                       (usuarioFila.Apellido != null && usuarioFila.Apellido.ToLower().Contains(filtro)) ||
                       (usuarioFila.Nombre_Usuario != null && usuarioFila.Nombre_Usuario.ToLower().Contains(filtro)) ||
                       (usuarioFila.Email != null && usuarioFila.Email.ToLower().Contains(filtro)) ||
                       (usuarioFila.Dni != null && usuarioFila.Dni.ToLower().Contains(filtro));
            }
            return false; // Oculta la fila si no hay coincidencias.
        }

        // Abre el formulario en blanco para dar de alta un usuario nuevo.
        private void EjecutarNuevo(object obj)
        {
            var ventana = new FormularioUsuarioWindow();
            var viewModel = new FormularioUsuarioViewModel();

            // Acciones para que el formulario se pueda cerrar solo y avise cuando guarde con éxito.
            viewModel.CerrarVentana = () => ventana.Close();
            viewModel.OnUsuarioGuardado = () =>
            {
                // Agrega el nuevo usuario a la colección observable.
                ListaUsuarios.Add(viewModel.UsuarioActual);
                VistaFiltroUsuarios.Refresh();
            };

            ventana.DataContext = viewModel;
            ventana.ShowDialog(); // Abre la ventana y frena la ejecución hasta que la cierren.
        }

        // Abre el formulario cargando los datos del usuario que seleccionamos en la grilla para modificarlo.
        private void EjecutarEditar(object obj)
        {
            // El objeto viene del botón de la fila en la interfaz. Nos aseguramos que sea un usuario válido.
            if (obj is UsuarioViewModel usuarioFila)
            {
                // Extrae la entidad pura para tener los datos originales.
                var entidadOriginal = usuarioFila.ObtenerEntidadPura();

                // CLONACIÓN: Se crea una instancia completamente nueva con los mismos datos. 
                // Esto previene la "edición fantasma" en la interfaz si el usuario cancela la operación.
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

                var ventana = new FormularioUsuarioWindow();

                //  Se le pasa el CLON al formulario en lugar de la entidad original.
                var viewModel = new FormularioUsuarioViewModel(entidadClonada);

                viewModel.CerrarVentana = () => ventana.Close();

                viewModel.OnUsuarioGuardado = () =>
                {
                    //  Si la base de datos confirma el éxito, copiamos los datos del clon de vuelta a la fila original.
                    // Al usar las propiedades del ViewModel (usuarioFila), se dispara OnPropertyChanged() y la grilla se actualiza en pantalla de inmediato.
                    usuarioFila.Nombre = entidadClonada.Nombre;
                    usuarioFila.Apellido = entidadClonada.Apellido;
                    usuarioFila.Dni = entidadClonada.Dni;
                    usuarioFila.Email = entidadClonada.Email;
                    usuarioFila.Nombre_Usuario = entidadClonada.Nombre_Usuario;
                    usuarioFila.Fecha_Nacimiento = entidadClonada.Fecha_Nacimiento;
                    usuarioFila.Rol = entidadClonada.Rol;
                    usuarioFila.Activo = entidadClonada.Activo;

                    VistaFiltroUsuarios.Refresh();
                };

                ventana.DataContext = viewModel;
                ventana.ShowDialog();
            }
        }

        // Da de baja al usuario seleccionado tras pedir una confirmación por seguridad.
        private void EjecutarEliminar(object obj)
        {
            if (obj is UsuarioViewModel usuarioFila)
            {
                // Validación de seguridad para evitar borrados accidentales.
                var respuesta = MessageBox.Show($"¿Estás seguro de que deseas eliminar el usuario '{usuarioFila.Nombre} {usuarioFila.Apellido}'?",
                                                 "Confirmar Eliminación",
                                                 MessageBoxButton.YesNo,
                                                 MessageBoxImage.Warning);

                if (respuesta == MessageBoxResult.Yes)
                {
                    try
                    {
                        // Mandamos la orden de eliminar a la base de datos a través del servicio.
                        bool eliminado = _usuarioService.EliminarUsuario(usuarioFila.Id_Usuario);

                        if (eliminado)
                        {
                            // Elimina la fila visualmente de la lista.
                            ListaUsuarios.Remove(usuarioFila);
                            VistaFiltroUsuarios.Refresh();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error al Eliminar", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
    }
}