using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace TillasDesktop.UI.Modelos
{
    public class FormularioClienteViewModel : ViewModelBase
    {
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

        public ClienteViewModel ClienteResultado { get; private set; }

        public ICommand GuardarCommand { get; }
        public Action<bool?> CerrarVentanaAccion { get; set; }

        public FormularioClienteViewModel()
        {
            GuardarCommand = new RelayCommand(EjecutarGuardar);
        }

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

        private void EjecutarGuardar(object obj)
        {
            if (string.IsNullOrWhiteSpace(Nombre) ||
                string.IsNullOrWhiteSpace(Apellido) ||
                string.IsNullOrWhiteSpace(Cuit) ||
                string.IsNullOrWhiteSpace(Telefono) ||
                string.IsNullOrWhiteSpace(Email))
            {
                MessageBox.Show("Todos los campos son obligatorios. Por favor, complete la información faltante.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

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
            
            string patronEmail = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            if (!Regex.IsMatch(Email.Trim(), patronEmail))
            {
                MessageBox.Show("El formato del correo electrónico no es válido (ejemplo: usuario@dominio.com).", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!Nombre.All(c => char.IsLetter(c) || char.IsWhiteSpace(c)) ||
                !Apellido.All(c => char.IsLetter(c) || char.IsWhiteSpace(c)))
            {
                MessageBox.Show("El nombre y el apellido no deben contener números ni símbolos.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (ClienteResultado == null)
            {
                ClienteResultado = new ClienteViewModel();
            }

            ClienteResultado.Nombre = Nombre.Trim();
            ClienteResultado.Apellido = Apellido.Trim();
            ClienteResultado.CUIT = Cuit.Trim();
            ClienteResultado.Telefono = Telefono.Trim();
            ClienteResultado.Email = Email.Trim();

            CerrarVentanaAccion?.Invoke(true);
        }
    }
}