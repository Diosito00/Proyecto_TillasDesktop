using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using TillasDesktop.Entities.Usuarios;
using TillasDesktop.UI.Vistas;
using TillasDesktop.DAL.Repositorios; // <-- 1. Importar el repositorio

namespace TillasDesktop.UI.Modelos
{
    public class GestionUsuarioViewModel : ViewModelBase
    {
        // 2. Declarar una instancia del repositorio de usuarios
        private readonly UsuarioRepository _usuarioRepository;
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
            // 3. Inicializar el repositorio
            _usuarioRepository = new UsuarioRepository();

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
                // Llamamos al repositorio para traer la lista de la BD
                var usuariosDeDb = _usuarioRepository.ObtenerTodos();

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

        // 2. EL FILTRO AHORA EVALÚA EL ENVOLTORIO
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

        // 3. NUEVO USUARIO (INSERCIÓN EN LA BASE DE DATOS)
        private void EjecutarNuevo(object obj)
        {
            var ventana = new FormularioUsuarioWindow();
            var viewModel = new FormularioUsuarioViewModel();

            viewModel.CerrarVentana = () => ventana.Close();
            viewModel.OnUsuarioGuardado = () =>
            {
                try
                {
                    // Obtenemos la entidad pura desde el formulario
                    var nuevaEntidad = viewModel.UsuarioActual.ObtenerEntidadPura();

                    // Guardamos físicamente en la base de datos usando el repositorio
                    bool insertado = _usuarioRepository.Insertar(nuevaEntidad);

                    if (insertado)
                    {
                        // Si se guardó con éxito en la BD, lo agregamos a la interfaz visual
                        ListaUsuarios.Add(viewModel.UsuarioActual);
                        VistaFiltroUsuarios.Refresh();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error al Guardar", MessageBoxButton.OK, MessageBoxImage.Error);
                }
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
                var ventana = new FormularioUsuarioWindow();

                // Le pasamos la entidad pura al formulario
                var viewModel = new FormularioUsuarioViewModel(usuarioFila.ObtenerEntidadPura());

                viewModel.CerrarVentana = () => ventana.Close();
                viewModel.OnUsuarioGuardado = () =>
                {
                    try
                    {
                        // Extraemos la entidad modificada
                        var entidadModificada = usuarioFila.ObtenerEntidadPura();

                        // Actualizamos en la base de datos a través del repositorio
                        bool actualizado = _usuarioRepository.Actualizar(entidadModificada);

                        if (actualizado)
                        {
                            VistaFiltroUsuarios.Refresh();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error al Actualizar", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
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
               var respuesta = MessageBox.Show($"¿Estás seguro de que deseas eliminar permanentemente el usuario '{usuarioFila.Nombre} {usuarioFila.Apellido}'?",
                                                "Confirmar Eliminación",
                                                MessageBoxButton.YesNo,
                                                MessageBoxImage.Warning);

                if (respuesta == MessageBoxResult.Yes)
                {
                    try
                    {
                        // Llamamos al repositorio pasándole el ID del usuario seleccionado
                        bool eliminado = _usuarioRepository.Eliminar(usuarioFila.Id_Usuario);

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