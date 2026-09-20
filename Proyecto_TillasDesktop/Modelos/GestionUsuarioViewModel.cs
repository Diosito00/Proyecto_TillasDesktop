using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using TillasDesktop.Entities.Usuarios;
using TillasDesktop.UI.Vistas;

namespace TillasDesktop.UI.Modelos
{
    public class GestionUsuarioViewModel : ViewModelBase
    {
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
            CargarDatos();

            VistaFiltroUsuarios = CollectionViewSource.GetDefaultView(ListaUsuarios);
            VistaFiltroUsuarios.Filter = FiltrarCriteriosUsuarios;

            // Se asume que los botones de la grilla (Editar/Eliminar) envían el CommandParameter="{Binding}"
            NuevoUsuarioCommand = new RelayCommand(EjecutarNuevo);
            EditarCommand = new RelayCommand(EjecutarEditar);
            EliminarCommand = new RelayCommand(EjecutarEliminar);
        }

        private void CargarDatos()
        {
            ListaUsuarios = new ObservableCollection<UsuarioViewModel>();

            // Simulamos la carga envolviendo las entidades puras
            var user1 = new Usuario { Id_Usuario = 1, Nombre = "Juan", Apellido = "Perez", Dni = "12345678", Email = "admin@tillas.com", Nombre_Usuario = "jperez", Password = "123", Fecha_Nacimiento = new System.DateTime(1990, 5, 15), Rol = "Admin", Activo = true };
            var user2 = new Usuario { Id_Usuario = 2, Nombre = "María", Apellido = "García", Dni = "87654321", Email = "gerente@tillas.com", Nombre_Usuario = "mgarcia", Password = "123", Fecha_Nacimiento = new System.DateTime(1995, 8, 25), Rol = "Gerente", Activo = true };

            ListaUsuarios.Add(new UsuarioViewModel(user1));
            ListaUsuarios.Add(new UsuarioViewModel(user2));
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

        // 3. NUEVO USUARIO A TRAVÉS DEL VIEWMODEL
        private void EjecutarNuevo(object obj)
        {
            var ventana = new FormularioUsuarioWindow();
            var viewModel = new FormularioUsuarioViewModel();

            viewModel.CerrarVentana = () => ventana.Close();
            viewModel.OnUsuarioGuardado = () =>
            {
                // Como es nuevo, tomamos el envoltorio creado en el formulario y lo añadimos a la vista
                ListaUsuarios.Add(viewModel.UsuarioActual);
                VistaFiltroUsuarios.Refresh();
            };

            ventana.DataContext = viewModel;
            ventana.ShowDialog();
        }

        // 4. EDICIÓN EXTRAYENDO LA ENTIDAD PURA
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
                    // Como pasamos la misma referencia de memoria de la entidad, 
                    // los cambios impactan automáticamente. Solo forzamos el refresco del DataGrid.
                    VistaFiltroUsuarios.Refresh();
                };

                ventana.DataContext = viewModel;
                ventana.ShowDialog();
            }
        }

        // 5. ELIMINACIÓN DIRECTA
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
                    // TODO: _usuariosService.EliminarUsuario(usuarioFila.ID);
                    ListaUsuarios.Remove(usuarioFila);
                    VistaFiltroUsuarios.Refresh();
                }
            }
        }
    }
}