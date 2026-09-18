using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System;
using TillasDesktop.Entities.Usuarios;

namespace TillasDesktop.UI.Modelos
{
    // Declaración de la clase FormularioUsuarioViewModel, la cual hereda de ViewModelBase.
    // Actúa como el ViewModel dedicado exclusivamente a gestionar la lógica de datos y validaciones del formulario de usuario.
    public class FormularioUsuarioViewModel : ViewModelBase
    {
        // Campo privado _nombre que almacena internamente el valor escrito en el campo de nombre.
        private string _nombre = string.Empty;

        // Propiedad pública Nombre vinculada al campo de texto correspondiente en la interfaz gráfica.
        // Al cambiar su valor, notifica a la interfaz y también revisa si el formulario ya está completo.
        public string Nombre
        {
            get => _nombre;
            set { _nombre = value; OnPropertyChanged(); OnPropertyChanged(nameof(FormularioValido)); }
        }

        private string _apellido = string.Empty;
        public string Apellido
        {
            get => _apellido;
            set { _apellido = value; OnPropertyChanged(); OnPropertyChanged(nameof(FormularioValido)); }
        }

        // Campo privado _dni que almacena internamente el número de documento del usuario.
        private string _dni = string.Empty;

        // Propiedad pública para el DNI. Avisa si cambia y vuelve a revisar si el formulario está listo.
        public string Dni
        {
            get => _dni;
            set { _dni = value; OnPropertyChanged(); OnPropertyChanged(nameof(FormularioValido)); }
        }

        // Campo privado _email que almacena el correo electrónico ingresado.
        private string _email = string.Empty;

        // Propiedad pública para el correo. Controla cambios y revisa la validez general.
        public string Email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(); OnPropertyChanged(nameof(FormularioValido)); }
        }

        private string _nombreUsuario = string.Empty;
        public string NombreUsuario
        {
            get => _nombreUsuario;
            set { _nombreUsuario = value; OnPropertyChanged(); OnPropertyChanged(nameof(FormularioValido)); }
        }


        // Campo privado _password que almacena la contraseña ingresada.
        private string _password = string.Empty;

        // Propiedad pública Password vinculada al control de contraseña en la interfaz.
        public string Password
        {
            get => _password;
            set { _password = value; OnPropertyChanged(); OnPropertyChanged(nameof(FormularioValido)); }
        }

        private DateTime _fechaNacimiento = DateTime.Today;
        public DateTime FechaNacimiento
        {
            get => _fechaNacimiento;
            set { _fechaNacimiento = value; OnPropertyChanged(); }
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
                    string.IsNullOrWhiteSpace(Apellido) ||
                    string.IsNullOrWhiteSpace(Dni) ||
                    string.IsNullOrWhiteSpace(Email) ||
                    string.IsNullOrWhiteSpace(Password) ||
                    string.IsNullOrWhiteSpace(RolSeleccionado))
                {
                    return false;
                }

                // DNI numérico y de longitud mínima lógica (ej. 7 dígitos)
                if (Dni.Length < 7 || !Regex.IsMatch(Dni, @"^\d+$"))
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

        public ICommand VerPasswordCommand { get; }

        // Acción (del tipo Action) que se invoca para ordenar el cierre de la ventana.
        public Action<bool?> CerrarVentanaAccion { get; set; }

        

        // Constructor de la clase: Se ejecuta al inicializar el ViewModel del formulario.
        public FormularioUsuarioViewModel()
        {
            // Inicializa la lista de opciones que aparecerán en el menú desplegable de roles.
            RolesDisponibles = new ObservableCollection<string> { "Admin", "Gerente", "Vendedor" };

            // Configura el comando de guardado vinculándolo a su método de ejecución y a su regla de habilitación (CanEjecutarGuardar).
            GuardarCommand = new RelayCommand(EjecutarGuardar);

           
        }

        // Constructor para EDITAR
        public FormularioUsuarioViewModel(Usuario usuarioAEditar) : this()
        {
            if (usuarioAEditar != null)
            {
                UsuarioResultado = usuarioAEditar;
                Nombre = usuarioAEditar.Nombre;
                Apellido = usuarioAEditar.Apellido;
                Dni = usuarioAEditar.DNI;
                Email = usuarioAEditar.Email;
                NombreUsuario = usuarioAEditar.Nombre_Usuario;
                Password = usuarioAEditar.Password;
                FechaNacimiento = usuarioAEditar.FechaNacimiento == default ? DateTime.Today : usuarioAEditar.FechaNacimiento;
                RolSeleccionado = usuarioAEditar.Rol;
                Activo = usuarioAEditar.Activo;
            }
        }

        // Método de validación del comando:// Esta regla decide si el botón de guardar se puede presionar (devuelve true solo si el formulario está completamente válido).
        private bool CanEjecutarGuardar(object obj) => FormularioValido;


        // Método que se ejecuta al presionar el botón de guardar cuando el comando está habilitado.
        private void EjecutarGuardar(object obj)
        {


            // Validación general unificada para todos los campos obligatorios
            if (string.IsNullOrWhiteSpace(Nombre) ||
                string.IsNullOrWhiteSpace(Apellido) ||
                string.IsNullOrWhiteSpace(Dni) ||
                string.IsNullOrWhiteSpace(Email) ||
                string.IsNullOrWhiteSpace(NombreUsuario) ||
                string.IsNullOrWhiteSpace(Password) ||
                string.IsNullOrWhiteSpace(RolSeleccionado))
            {
                MessageBox.Show("Todos los campos son obligatorios. Por favor, complete la información faltante.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!Dni.All(char.IsDigit) || Dni.Length < 7)
            {
                MessageBox.Show("El DNI ingresado no es válido (debe contener al menos 7 números).", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string patronEmail = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            if (!Regex.IsMatch(Email.Trim(), patronEmail))
            {
                MessageBox.Show("El formato del correo electrónico no es válido (ejemplo: usuario@dominio.com).", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!Nombre.All(c => char.IsLetter(c) || char.IsWhiteSpace(c)) || !Apellido.All(c => char.IsLetter(c) || char.IsWhiteSpace(c)))
            {
                MessageBox.Show("El nombre y el apellido no deben contener números ni símbolos.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (UsuarioResultado == null)
            {
                UsuarioResultado = new Usuario();
            }

            // Instancia un nuevo objeto de tipo Usuario y le asigna las propiedades recopiladas en los campos del formulario.

            UsuarioResultado.Nombre = Nombre.Trim();
            UsuarioResultado.Apellido = Apellido.Trim();
            UsuarioResultado.DNI = Dni.Trim();
            UsuarioResultado.Email = Email.Trim();
            UsuarioResultado.Nombre_Usuario = NombreUsuario.Trim();
            UsuarioResultado.Password = Password;
            UsuarioResultado.FechaNacimiento = FechaNacimiento;
            UsuarioResultado.Rol = RolSeleccionado;
            UsuarioResultado.Activo = Activo;

            // Ejecuta la acción asignada externamente para cerrar la ventana.
            CerrarVentanaAccion?.Invoke(true);
        }




    }

}