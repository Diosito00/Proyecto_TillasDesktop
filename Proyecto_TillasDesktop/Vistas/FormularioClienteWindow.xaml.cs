using System.Windows; // Importa los componentes esenciales de la interfaz y ventanas de WPF.
using System.Windows.Input;
using TillasDesktop.Entities.Usuarios;
using TillasDesktop.UI.Modelos;// Importa los modelos del proyecto (como la clase Cliente).
using System.Text.RegularExpressions;


namespace TillasDesktop.UI.Vistas
{
    // Clase parcial que maneja la lógica de la ventana emergente para registrar o editar un cliente.
    public partial class FormularioClienteWindow : Window
    {
        // Propiedad pública que almacena el cliente creado o editado para ser devuelto a la ventana principal.
        public ClienteViewModel? NuevoCliente { get; set; }

        // Constructor vacío: se utiliza cuando se quiere dar de alta/crear un nuevo cliente desde cero.
        public FormularioClienteWindow()
        {
            InitializeComponent(); // Carga y dibuja los componentes visuales definidos en el archivo XAML.
        }

        // Constructor con parámetros: se utiliza cuando se quiere editar un cliente existente, recibiendo sus datos actuales.
        public FormularioClienteWindow(ClienteViewModel clienteAEditar)
        {
            InitializeComponent(); // Inicializa los componentes de la interfaz.

            // Rellena los cuadros de texto del formulario con la información del cliente que se va a editar.
            txtNombre.Text = clienteAEditar.Nombre;
            txtApellido.Text = clienteAEditar.Apellido;
            txtDNI.Text = clienteAEditar.DNI;
            txtCUIT.Text = clienteAEditar.CUIT;
            txtTelefono.Text = clienteAEditar.Telefono;
            txtEmail.Text = clienteAEditar.Email;

            // Asigna la referencia del cliente existente para modificarlo directamente al guardar.
            NuevoCliente = clienteAEditar;
        }

        // Evento que se ejecuta al hacer clic en el botón "GUARDAR CLIENTE".
        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            // 1. Validar que ningún campo obligatorio esté vacío
            if (string.IsNullOrWhiteSpace(txtNombre.Text) ||
                string.IsNullOrWhiteSpace(txtApellido.Text) ||
                string.IsNullOrWhiteSpace(txtDNI.Text) ||
                string.IsNullOrWhiteSpace(txtCUIT.Text) ||
                string.IsNullOrWhiteSpace(txtTelefono.Text) ||
                string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                MessageBox.Show("Todos los campos son obligatorios. Por favor, complete la información faltante.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 2. Validar que el DNI contenga solo números y una longitud lógica
            if (!txtDNI.Text.All(char.IsDigit) || txtDNI.Text.Length < 7)
            {
                MessageBox.Show("El DNI ingresado no es válido (debe contener al menos 7 números).", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 3. Validar longitud mínima del teléfono
            if (txtTelefono.Text.Trim().Length < 7)
            {
                MessageBox.Show("El teléfono ingresado es demasiado corto.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 4. Validar formato de correo electrónico
            string patronEmail = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            if (!Regex.IsMatch(txtEmail.Text.Trim(), patronEmail))
            {
                MessageBox.Show("El formato del correo electrónico no es válido (ejemplo: usuario@dominio.com).", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Validar que Nombre y Apellido contengan solo letras y espacios
            if (!txtNombre.Text.All(c => char.IsLetter(c) || char.IsWhiteSpace(c)) ||
                !txtApellido.Text.All(c => char.IsLetter(c) || char.IsWhiteSpace(c)))
            {
                MessageBox.Show("El nombre y el apellido no deben contener números ni símbolos.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Si pasa las validaciones, continúa con la asignación normal...
            if (NuevoCliente == null)
            {
                NuevoCliente = new ClienteViewModel();
            }

            NuevoCliente.Nombre = txtNombre.Text;
            NuevoCliente.Apellido = txtApellido.Text;
            NuevoCliente.DNI = txtDNI.Text;
            NuevoCliente.CUIT = txtCUIT.Text;
            NuevoCliente.Telefono = txtTelefono.Text;
            NuevoCliente.Email = txtEmail.Text;

            DialogResult = true;
        }

        private void SoloNumeros_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Expresión regular que solo permite dígitos numéricos
            e.Handled = !e.Text.All(char.IsDigit);
        }

        private void Telefono_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !e.Text.All(c => char.IsDigit(c) || c == '+' || c == '-' || c == ' ');
        }

        private void SoloLetras_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Permite solo letras (incluyendo vocales con tilde y espacios)
            e.Handled = !e.Text.All(c => char.IsLetter(c) || char.IsWhiteSpace(c));
        }
    }
}