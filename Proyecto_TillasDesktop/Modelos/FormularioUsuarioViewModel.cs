using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows.Input;
using TillasDesktop.Entities.Usuarios;

namespace TillasDesktop.UI.Modelos
{
    // Declaración de la clase FormularioUsuarioViewModel, la cual hereda de ViewModelBase.
    // Actúa como el ViewModel dedicado exclusivamente a gestionar la lógica de datos y validaciones del formulario de usuario.
    public class FormularioUsuarioViewModel : ViewModelBase
    {
        // Campo privado _nombre que almacena internamente el valor escrito en el campo de nombre.
        private string _nombre;

        // Propiedad pública Nombre vinculada al campo de texto correspondiente en la interfaz gráfica.
        // Al cambiar su valor, notifica a la interfaz y también revisa si el formulario ya está completo.
        public string Nombre
        {
            get => _nombre;
            set { _nombre = value; OnPropertyChanged(); OnPropertyChanged(nameof(FormularioValido)); }
        }

        // Campo privado _dni que almacena internamente el número de documento del usuario.
        private string _dni;

        // Propiedad pública para el DNI. Avisa si cambia y vuelve a revisar si el formulario está listo.
        public string Dni
        {
            get => _dni;
            set { _dni = value; OnPropertyChanged(); OnPropertyChanged(nameof(FormularioValido)); }
        }

        // Campo privado _email que almacena el correo electrónico ingresado.
        private string _email;

        // Propiedad pública para el correo. Controla cambios y revisa la validez general.
        public string Email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(); OnPropertyChanged(nameof(FormularioValido)); }
        }

        // Campo privado _password que almacena la contraseña ingresada.
        private string _password;

        // Propiedad pública Password vinculada al control de contraseña en la interfaz.
        public string Password
        {
            get => _password;
            set { _password = value; OnPropertyChanged(); OnPropertyChanged(nameof(FormularioValido)); }
        }

        // Campo privado _rolSeleccionado que almacena el rol elegido dentro de las opciones disponibles.
        private string _rolSeleccionado;

        // Propiedad pública RolSeleccionado vinculada al ComboBox de roles en la interfaz.
        public string RolSeleccionado
        {
            get => _rolSeleccionado;
            set { _rolSeleccionado = value; OnPropertyChanged(); OnPropertyChanged(nameof(FormularioValido)); }
        }

        // Campo privado _activo que almacena el estado booleano de activación del usuario (por defecto true).
        private bool _activo = true;

        // Propiedad pública Activo vinculada al CheckBox que define si el usuario está activo o inactivo.
        public bool Activo
        {
            get => _activo;
            set { _activo = value; OnPropertyChanged(); }
        }

        // Propiedad FormularioValido: Evalúa en tiempo real si todos los campos obligatorios están completos 
        // y si el correo electrónico cumple estrictamente con el formato definido por una expresión regular (Regex).
        public bool FormularioValido
        {
            get
            {
                // Si alguno de los campos esenciales está vacío, nulo o compuesto solo por espacios, el formulario no es válido.
                if (string.IsNullOrWhiteSpace(Nombre) ||
                    string.IsNullOrWhiteSpace(Dni) ||
                    string.IsNullOrWhiteSpace(Email) ||
                    string.IsNullOrWhiteSpace(Password) ||
                    string.IsNullOrWhiteSpace(RolSeleccionado))
                {
                    return false;
                }

                // Expresión regular estándar para verificar una estructura de correo válida (ejemplo: texto@texto.texto).
                string patronEmail = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
                return Regex.IsMatch(Email, patronEmail);
            }
        }

        // Propiedad de solo lectura externa que almacenará el objeto Usuario ya construido una vez superadas las validaciones.
        public Usuario UsuarioResultado { get; private set; }

        // Una lista con las opciones de roles ("Admin", "Gerente", "Vendedor") que se muestran en el menú desplegable.
        public ObservableCollection<string> RolesDisponibles { get; set; }

        // Comando público que maneja la acción de guardar los datos del formulario.
        public ICommand GuardarCommand { get; }

        // Acción (del tipo Action) que se invoca para ordenar el cierre de la ventana.
        public Action CerrarVentanaAccion { get; set; }

        // Constructor de la clase: Se ejecuta al inicializar el ViewModel del formulario.
        public FormularioUsuarioViewModel()
        {
            // Inicializa la lista de opciones que aparecerán en el menú desplegable de roles.
            RolesDisponibles = new ObservableCollection<string> { "Admin", "Gerente", "Vendedor" };

            // Configura el comando de guardado vinculándolo a su método de ejecución y a su regla de habilitación (CanEjecutarGuardar).
            GuardarCommand = new RelayCommand(EjecutarGuardar, CanEjecutarGuardar);
        }

        // Método de validación del comando:// Esta regla decide si el botón de guardar se puede presionar (devuelve true solo si el formulario está completamente válido).
        private bool CanEjecutarGuardar(object obj)
        {
            return FormularioValido;
        }

        // Método que se ejecuta al presionar el botón de guardar cuando el comando está habilitado.
        private void EjecutarGuardar(object obj)
        {
            // Instancia un nuevo objeto de tipo Usuario y le asigna las propiedades recopiladas en los campos del formulario.
            UsuarioResultado = new Usuario
            {
                Nombre = this.Nombre,
                DNI = this.Dni,
                Email = this.Email,
                Password = this.Password,
                Rol = this.RolSeleccionado,
                Activo = this.Activo
            };

            // Ejecuta la acción asignada externamente para cerrar la ventana.
            CerrarVentanaAccion?.Invoke();
        }
    }
}