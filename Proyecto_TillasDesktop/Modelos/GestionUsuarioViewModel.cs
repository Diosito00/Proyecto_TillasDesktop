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
    public class GestionUsuarioViewModel : ViewModelBase
    {
        // Declarar una instancia del servicio de usuarios
        private readonly UsuariosService _usuarioService;
        private ObservableCollection<UsuarioViewModel> _listaUsuarios;
        public ObservableCollection<UsuarioViewModel> ListaUsuarios
        {
            get => _listaUsuarios;
            set { _listaUsuarios = value; OnPropertyChanged(); }
        }

        private string _textoBusqueda = string.Empty;
        public string TextoBusqueda
        {
            get => _textoBusqueda;
            set
            {
                _textoBusqueda = value;
                OnPropertyChanged();
                VistaFiltroUsuarios?.Refresh();
            }
        }

        public ICollectionView VistaFiltroUsuarios { get; set; }

        public ICommand NuevoUsuarioCommand { get; }
        public ICommand EditarCommand { get; }
        public ICommand EliminarCommand { get; }

        public GestionUsuarioViewModel()
        {
            // Inicializar el servicio
            _usuarioService = new UsuariosService();

            CargarDatos();

            VistaFiltroUsuarios = CollectionViewSource.GetDefaultView(ListaUsuarios);
            VistaFiltroUsuarios.Filter = FiltrarCriteriosUsuarios;

            // Se asume que los botones de la grilla (Editar/Eliminar) envían el CommandParameter="{Binding}"
            NuevoUsuarioCommand = new RelayCommand(EjecutarNuevo);
            EditarCommand = new RelayCommand(EjecutarEditar);
            EliminarCommand = new RelayCommand(EjecutarEliminar);
        }

        // CARGAR DATOS DESDE LA BASE DE DATOS (REEMPLAZA USER1 Y USER2)
        private void CargarDatos()
        {
            ListaUsuarios = new ObservableCollection<UsuarioViewModel>();

            try
            {
                // Llamamos al servicio para traer la lista de la BD
                var usuariosDeDb = _usuarioService.ObtenerTodos();

                // Envolvemos cada entidad pura de la BD en un UsuarioViewModel y la agregamos a la lista observable
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

        // EL FILTRO AHORA EVALÚA EL ENVOLTORIO
        private bool FiltrarCriteriosUsuarios(object obj)
        {
            if (obj is UsuarioViewModel usuarioFila)
            {
                if (string.IsNullOrWhiteSpace(TextoBusqueda)) return true;

                string filtro = TextoBusqueda.ToLower();
                return (usuarioFila.Nombre != null && usuarioFila.Nombre.ToLower().Contains(filtro)) ||
                       (usuarioFila.Apellido != null && usuarioFila.Apellido.ToLower().Contains(filtro)) ||
                       (usuarioFila.Nombre_Usuario != null && usuarioFila.Nombre_Usuario.ToLower().Contains(filtro)) ||
                       (usuarioFila.Email != null && usuarioFila.Email.ToLower().Contains(filtro)) ||
                       (usuarioFila.Dni != null && usuarioFila.Dni.ToLower().Contains(filtro)); // Asegúrate que la propiedad se llame Dni o DNI según tu Wrapper
            }
            return false;
        }

        // NUEVO USUARIO (INSERCIÓN EN LA BASE DE DATOS)
        private void EjecutarNuevo(object obj)
        {
            var ventana = new FormularioUsuarioWindow();
            var viewModel = new FormularioUsuarioViewModel();

            viewModel.CerrarVentana = () => ventana.Close();
            viewModel.OnUsuarioGuardado = () =>
            {
                // Ya no calculamos nada. La entidad ya trae su ID real de SQL Server.
                ListaUsuarios.Add(viewModel.UsuarioActual);
                VistaFiltroUsuarios.Refresh();
            };

            ventana.DataContext = viewModel;
            ventana.ShowDialog();
        }

        // 4. EDICIÓN (ACTUALIZACIÓN EN LA BASE DE DATOS)
        private void EjecutarEditar(object obj)
        {
            // El obj llega desde el botón del DataGrid mediante CommandParameter="{Binding}"
            if (obj is UsuarioViewModel usuarioFila)
            {
                // Extraemos la entidad original de la fila seleccionada
                var entidadOriginal = usuarioFila.ObtenerEntidadPura();

                // Creamos un "clon" desconectado en otra dirección de memoria
                var entidadClonada = new Usuario
                {
                    Id_Usuario = entidadOriginal.Id_Usuario,
                    Nombre = entidadOriginal.Nombre,
                    Apellido = entidadOriginal.Apellido,
                    Dni = entidadOriginal.Dni,
                    Email = entidadOriginal.Email,
                    Nombre_Usuario = entidadOriginal.Nombre_Usuario,
                    Password = entidadOriginal.Password, // Mantenemos el hash intacto
                    Fecha_Nacimiento = entidadOriginal.Fecha_Nacimiento,
                    Rol = entidadOriginal.Rol,
                    Activo = entidadOriginal.Activo
                };

                var ventana = new FormularioUsuarioWindow();

                // Le pasamos el CLON al formulario, protegiendo los datos reales
                var viewModel = new FormularioUsuarioViewModel(entidadClonada);

                viewModel.CerrarVentana = () => ventana.Close();

                viewModel.OnUsuarioGuardado = () =>
                {
                    // Si el guardado en BD fue exitoso, volcamos los cambios del clon a la fila original.
                    // Usamos las propiedades de 'UsuarioViewModel' para que disparen automáticamente 
                    // el evento OnPropertyChanged() y la tabla se redibuje al instante.
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

        // 5. ELIMINACIÓN (BORRADO EN LA BASE DE DATOS)
        private void EjecutarEliminar(object obj)
        {
            if (obj is UsuarioViewModel usuarioFila)
            {
               var respuesta = MessageBox.Show($"¿Estás seguro de que deseas eliminar el usuario '{usuarioFila.Nombre} {usuarioFila.Apellido}'?",
                                                "Confirmar Eliminación",
                                                MessageBoxButton.YesNo,
                                                MessageBoxImage.Warning);

                if (respuesta == MessageBoxResult.Yes)
                {
                    try
                    {
                        // Llamamos al repositorio pasándole el ID del usuario seleccionado
                        bool eliminado = _usuarioService.EliminarUsuario(usuarioFila.Id_Usuario);

                        if (eliminado)
                        {
                            // Si se borró de la BD, lo quitamos de la lista visual
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