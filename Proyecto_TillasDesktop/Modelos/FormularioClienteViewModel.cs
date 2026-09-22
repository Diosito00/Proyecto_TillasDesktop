using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace TillasDesktop.UI.Modelos
{
    // ViewModel encargado de manejar la lógica del formulario para crear o editar un cliente.
    public class FormularioClienteViewModel : ViewModelBase
    {
        // Campos privados y propiedades públicas para cada input del formulario
        private string _nombre = string.Empty;
        public string Nombre
        {
            get => _nombre;
            set { _nombre = value; OnPropertyChanged(); }
        }

        private string _apellido = string.Empty;
        public string Apellido
        {
            get => _apellido;
            set { _apellido = value; OnPropertyChanged(); }
        }

        private string _cuit = string.Empty;
        public string Cuit
        {
            get => _cuit;
            set { _cuit = value; OnPropertyChanged(); }
        }

        private string _telefono = string.Empty;
        public string Telefono
        {
            get => _telefono;
            set { _telefono = value; OnPropertyChanged(); }
        }

        private string _email = string.Empty;
        public string Email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(); }
        }

        // Almacena el resultado final (el cliente creado o modificado) que será devuelto a la ventana principal.
        public ClienteViewModel ClienteResultado { get; private set; }

        // Comando que se dispara al hacer clic en el botón de guardar.
        public ICommand GuardarCommand { get; }

        // Acción (delegado) para cerrar la ventana devolviendo un booleano (true si guardó con éxito).
        public Action<bool?> CerrarVentanaAccion { get; set; }

        // Constructor por defecto: se usa cuando queremos dar de alta un cliente nuevo.
        public FormularioClienteViewModel()
        {
            GuardarCommand = new RelayCommand(EjecutarGuardar);
        }

        // Sobrecarga del constructor: se usa cuando pasamos un cliente existente para editarlo. 
        // El ': this()' asegura que primero se ejecute el constructor base para inicializar el comando.
        public FormularioClienteViewModel(ClienteViewModel clienteAEditar) : this()
        {
            if (clienteAEditar != null)
            {
                ClienteResultado = clienteAEditar;
                Nombre = clienteAEditar.Nombre;
                Apellido = clienteAEditar.Apellido;
                Cuit = clienteAEditar.CUIT;
                Telefono = clienteAEditar.Telefono;
                Email = clienteAEditar.Email;
            }
        }

        // Método que valida  los datos antes de aceptar el guardado.
        private void EjecutarGuardar(object obj)
        {
            // Validar que no queden campos vacíos o en blanco.
            if (string.IsNullOrWhiteSpace(Nombre) ||
                string.IsNullOrWhiteSpace(Apellido) ||
                string.IsNullOrWhiteSpace(Cuit) ||
                string.IsNullOrWhiteSpace(Telefono) ||
                string.IsNullOrWhiteSpace(Email))
            {
                MessageBox.Show("Todos los campos son obligatorios. Por favor, complete la información faltante.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            //  Validar que el CUIT contenga exactamente 11 dígitos numéricos (ignorando guiones).
            string cuitLimpio = Cuit.Replace("-", "").Trim();
            if (cuitLimpio.Length != 11 || !cuitLimpio.All(char.IsDigit))
            {
                MessageBox.Show("El CUIT ingresado no es válido (debe contener exactamente 11 números).", "Validación de CUIT", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Limpiamos el teléfono de símbolos comunes (+, -, espacios) para contar únicamente los dígitos reales
            string telefonoLimpio = new string(Telefono.Where(char.IsDigit).ToArray());

            if (telefonoLimpio.Length < 8 || telefonoLimpio.Length > 15)
            {
                MessageBox.Show("El teléfono ingresado no es válido (debe contener entre 8 y 15 dígitos).", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (Telefono.Trim().Length < 7)
            {
                MessageBox.Show("El teléfono ingresado es demasiado corto.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Validar el formato del correo electrónico mediante Expresiones Regulares (Regex).
            string patronEmail = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            if (!Regex.IsMatch(Email.Trim(), patronEmail))
            {
                MessageBox.Show("El formato del correo electrónico no es válido (ejemplo: usuario@dominio.com).", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Validar que el nombre y el apellido no tengan números ni símbolos raros.
            if (!Nombre.All(c => char.IsLetter(c) || char.IsWhiteSpace(c)) ||
                !Apellido.All(c => char.IsLetter(c) || char.IsWhiteSpace(c)))
            {
                MessageBox.Show("El nombre y el apellido no deben contener números ni símbolos.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Si es un cliente nuevo, instanciamos el objeto contenedor. Si estabamos editando, reutilizamos el existente.
            if (ClienteResultado == null)
            {
                ClienteResultado = new ClienteViewModel();
            }

            // Volcamos los datos validados al resultado final con los espacios limpiados.
            ClienteResultado.Nombre = Nombre.Trim();
            ClienteResultado.Apellido = Apellido.Trim();
            ClienteResultado.CUIT = Cuit.Trim();
            ClienteResultado.Telefono = Telefono.Trim();
            ClienteResultado.Email = Email.Trim();

            // Damos la orden de cerrar la ventana indicando que la operación fue exitosa (true).
            CerrarVentanaAccion?.Invoke(true);
        }
    }
}