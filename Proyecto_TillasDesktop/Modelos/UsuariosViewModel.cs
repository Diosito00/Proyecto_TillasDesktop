using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using TillasDesktop.Entities;
using TillasDesktop.Entities.Usuarios;
using TillasDesktop.UI.Vistas;
using System.Linq;
using System.Windows.Data;
using System.ComponentModel;

namespace TillasDesktop.UI.Modelos
{
    // Declaración de la clase UsuariosViewModel, la cual hereda de ViewModelBase.
    // Esta clase actúa como el intermediario (ViewModel en MVVM) entre la vista de la tabla de usuarios y los datos del sistema.
    public class UsuariosViewModel : ViewModelBase
    {
        // Campo privado _listaUsuarios que almacena internamente la colección de objetos de tipo Usuario.
        private ObservableCollection<Usuario> _listaUsuarios;

        // Propiedad pública ListaUsuarios vinculada directamente al DataGrid de la interfaz gráfica.
        // Utiliza ObservableCollection para que cualquier cambio (agregar/quitar elementos) se refleje automáticamente en la vista.
        public ObservableCollection<Usuario> ListaUsuarios
        {
            get => _listaUsuarios;
            set { _listaUsuarios = value; OnPropertyChanged(); }
        }

        // Campo privado _usuarioSeleccionado que almacena la fila o usuario que el usuario ha seleccionado en la tabla.
        private Usuario _usuarioSeleccionado;

        // Propiedad pública UsuarioSeleccionado para mantener sincronizado el elemento activo con los comandos de la interfaz.
        public Usuario UsuarioSeleccionado
        {
            get => _usuarioSeleccionado;
            set { _usuarioSeleccionado = value; OnPropertyChanged(); }
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



        // Comando público para gestionar la acción de crear o agregar un nuevo usuario.
        public ICommand NuevoUsuarioCommand { get; }

        // Comando público para gestionar la acción de editar el usuario seleccionado.
        public ICommand EditarCommand { get; }

        // Comando público para gestionar la acción de eliminar el usuario seleccionado.
        public ICommand EliminarCommand { get; }

        // Constructor de la clase: Se ejecuta automáticamente al inicializar la vista de usuarios y roles.
        public UsuariosViewModel()
        {
            // Llama al método para poblar la lista con los datos iniciales.
            CargarDatos();

            // Configuración del filtro de búsqueda
            VistaFiltroUsuarios = CollectionViewSource.GetDefaultView(ListaUsuarios);
            VistaFiltroUsuarios.Filter = FiltrarCriteriosUsuarios;

            // Inicializa los comandos vinculándolos a sus métodos correspondientes y a sus reglas de validación (canExecute).
            NuevoUsuarioCommand = new RelayCommand(EjecutarNuevo);
            EditarCommand = new RelayCommand(EjecutarEditar, CanExecuteSeleccionado);
            EliminarCommand = new RelayCommand(EjecutarEliminar, CanExecuteSeleccionado);
        }

        // Método privado para inicializar la lista con registros de prueba precargados.
        private void CargarDatos()
        {
            // Instancia una nueva ObservableCollection asignándole dos usuarios iniciales con sus respectivas propiedades.
            ListaUsuarios = new ObservableCollection<Usuario>
            {
               new Usuario { ID = 1, Nombre = "Juan Perez", DNI = "12345678", Email = "admin@tillas.com", Password = "123", Rol = "Admin", Activo = true },
                new Usuario { ID = 2, Nombre = "María García", DNI = "87654321", Email = "gerente@tillas.com", Password = "123", Rol = "Gerente", Activo = true }
            };
        }

        private bool FiltrarCriteriosUsuarios(object obj)
        {
            if (obj is Usuario usuario)
            {
                if (string.IsNullOrWhiteSpace(TextoBusqueda)) return true;

                string filtro = TextoBusqueda.ToLower();
                return (usuario.Nombre != null && usuario.Nombre.ToLower().Contains(filtro)) ||
                       (usuario.Email != null && usuario.Email.ToLower().Contains(filtro)) ||
                       (usuario.DNI != null && usuario.DNI.ToLower().Contains(filtro));
            }
            return false;
        }

        // Método que se ejecuta al activar el comando de un nuevo usuario (botón "+ NUEVO USUARIO").
        private void EjecutarNuevo(object obj)
        {
            // Crea una nueva instancia de la ventana del formulario de usuario en modo de creación.
            FormularioUsuarioWindow ventana = new FormularioUsuarioWindow();

            // Abre la ventana en modo modal (ShowDialog). Si el usuario guarda correctamente, devuelve true.
            if (ventana.ShowDialog() == true)
            {
                // Verifica que la propiedad NuevoUsuario del formulario contenga un objeto con datos válidos.
                if (ventana.NuevoUsuario != null)
                {
                    // Calcula de forma dinámica un nuevo ID autoincremental analizando el ID más alto existente en la lista.
                    int nuevoId = ListaUsuarios.Count > 0 ? ListaUsuarios.Max(u => u.ID) + 1 : 1;
                    ventana.NuevoUsuario.ID = nuevoId;

                    // Agrega el nuevo usuario a la colección observable, haciendo que aparezca inmediatamente en el DataGrid.
                    ListaUsuarios.Add(ventana.NuevoUsuario);
                    VistaFiltroUsuarios.Refresh();
                }
            }
        }

        // Método que se ejecuta al activar el comando de edición (al hacer clic en editar sobre un usuario seleccionado).
        private void EjecutarEditar(object obj)
        {
            // Abre el formulario de usuario pasándole como parámetro el objeto UsuarioSeleccionado actual para rellenar los campos.
            FormularioUsuarioWindow ventana = new FormularioUsuarioWindow(UsuarioSeleccionado);

            // Si la ventana se cierra con éxito (true) y el objeto devuelto no es nulo...
            if (ventana.ShowDialog() == true && ventana.NuevoUsuario != null)
            {
                // Busca la posición numérica (índice) que ocupa el usuario original dentro de la colección observable.
                int index = ListaUsuarios.IndexOf(UsuarioSeleccionado);

                // Si el índice es válido (existe en la lista)...
                if (index != -1)
                {
                    // Reemplaza el elemento antiguo por el nuevo en esa misma posición, forzando al DataGrid a refrescar la fila.
                    ListaUsuarios[index] = ventana.NuevoUsuario;
                    VistaFiltroUsuarios.Refresh();
                }
            }

        }

        // Método que se ejecuta al activar el comando para eliminar un usuario.
        private void EjecutarEliminar(object obj)
        {
            // Valida que el usuario seleccionado no sea nulo antes de intentar removerlo.
            if (UsuarioSeleccionado != null)
            {
                // Remueve el objeto seleccionado de la colección, eliminando la fila visualmente de la tabla.
                ListaUsuarios.Remove(UsuarioSeleccionado);
                VistaFiltroUsuarios.Refresh();
            }
        }

        // Regla lógica compartida: Determina si los botones de editar o eliminar pueden presionarse (retorna true solo si hay un usuario seleccionado).
        private bool CanExecuteSeleccionado(object obj)
        {
            return UsuarioSeleccionado != null;
        }
    }
}