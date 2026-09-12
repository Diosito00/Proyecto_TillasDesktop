using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using TillasDesktop.Entities.Usuarios;

namespace TillasDesktop.UI.Vistas
{
    // Declaración de la clase parcial FormularioUsuarioWindow, que hereda de Window (ventana WPF).
    // Funciona como la vista emergente para dar de alta un usuario nuevo o modificar uno existente.
    public partial class FormularioUsuarioWindow : Window
    {
        // Propiedad pública que almacena la instancia del usuario (creado o editado) para transferirla de vuelta a la vista principal.
        public Usuario? NuevoUsuario { get; set; }

        // Variable de control booleana para determinar el estado visual del botón "ojito" (contraseña visible u oculta).
        private bool _passwordVisible = false;

        // Constructor vacío: Se ejecuta cuando la ventana se abre para CREAR un usuario desde cero.
        public FormularioUsuarioWindow()
        {
            InitializeComponent(); // Carga y dibuja los componentes visuales definidos en el archivo XAML.
            CargarRoles();         // Llena el menú desplegable de roles.
        }

        // Constructor con parámetros: Se ejecuta cuando la ventana se abre para EDITAR un usuario, recibiendo los datos actuales.
        public FormularioUsuarioWindow(Usuario usuarioAEditar)
        {
            InitializeComponent(); // Inicializa los controles visuales de la interfaz.
            CargarRoles();         // Llena el menú desplegable de roles.

            // Rellena los cuadros de texto y controles de la ventana con la información del usuario recibido.
            txtNombre.Text = usuarioAEditar.Nombre;
            txtDNI.Text = usuarioAEditar.DNI;
            txtEmail.Text = usuarioAEditar.Email;
            txtPassword.Password = usuarioAEditar.Password;
            txtPasswordVisible.Text = usuarioAEditar.Password;
            cmbRol.SelectedItem = usuarioAEditar.Rol;
            chkActivo.IsChecked = usuarioAEditar.Activo;

            // Asigna la referencia del usuario recibido para conservar su rastro de edición.
            NuevoUsuario = usuarioAEditar;
        }

        // Método privado para poblar las opciones disponibles dentro del ComboBox de roles.
        private void CargarRoles()
        {
            cmbRol.ItemsSource = new[] { "Admin", "Gerente", "Vendedor" };
        }

        // Evento que se ejecuta al hacer clic en el botón "GUARDAR".
        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            // Determina de qué control extraer la contraseña actual según si el "ojito" está activo o no.
            string passwordActual = _passwordVisible ? txtPasswordVisible.Text : txtPassword.Password;

            // 1. Validación de campos obligatorios: Comprueba que ningún campo esté vacío o compuesto solo por espacios.
            if (string.IsNullOrWhiteSpace(txtNombre.Text) ||
                string.IsNullOrWhiteSpace(txtDNI.Text) ||
                string.IsNullOrWhiteSpace(txtEmail.Text) ||
                string.IsNullOrWhiteSpace(passwordActual) ||
                cmbRol.SelectedItem == null)
            {
                // Muestra un aviso de advertencia por pantalla y detiene la ejecución si falta información.
                MessageBox.Show("Todos los campos son obligatorios. Por favor, complete la información faltante.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 2. Validación del DNI: Verifica que todos los caracteres sean estrictamente numéricos y que tenga al menos 7 dígitos.
            if (!txtDNI.Text.All(char.IsDigit) || txtDNI.Text.Length < 7)
            {
                MessageBox.Show("El DNI ingresado no es válido (debe contener al menos 7 números).", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 3. Validación del formato de correo electrónico mediante una Expresión Regular (Regex).
            string patronEmail = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            if (!Regex.IsMatch(txtEmail.Text.Trim(), patronEmail))
            {
                MessageBox.Show("El formato del correo electrónico no es válido (ejemplo: usuario@dominio.com).", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 4. Validación del nombre: Comprueba que el texto contenga exclusivamente letras y espacios (sin números ni símbolos).
            if (!txtNombre.Text.All(c => char.IsLetter(c) || char.IsWhiteSpace(c)))
            {
                MessageBox.Show("El nombre no debe contener números ni símbolos.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Bloque condicional para instanciar o actualizar el objeto Usuario antes de cerrar la ventana.
            if (NuevoUsuario == null)
            {
                // Si es nulo, significa que es una creación nueva: se instancia un objeto vacío.
                NuevoUsuario = new Usuario();
            }
            else
            {
                // Si ya tenía datos (modo edición), se extrae su ID original y se instancia uno nuevo preservando dicho identificador.
                int idTemporal = NuevoUsuario.ID;
                NuevoUsuario = new Usuario { ID = idTemporal };
            }

            // Asigna los valores limpios y formateados de los controles visuales a las propiedades del objeto Usuario.
            NuevoUsuario.Nombre = txtNombre.Text.Trim();
            NuevoUsuario.DNI = txtDNI.Text.Trim();
            NuevoUsuario.Email = txtEmail.Text.Trim();
            NuevoUsuario.Password = passwordActual;
            NuevoUsuario.Rol = cmbRol.SelectedItem.ToString();
            NuevoUsuario.Activo = chkActivo.IsChecked ?? true;

            // Establece el resultado del diálogo en true, indicando éxito y ordenando el cierre de la ventana modal.
            DialogResult = true;
        }

        // Evento que gestiona la lógica interactiva del botón "ojito" para mostrar u ocultar la contraseña.
        private void BtnVerPassword_Click(object sender, RoutedEventArgs e)
        {
            _passwordVisible = !_passwordVisible; // Invierte el estado booleano de visibilidad.
            if (_passwordVisible)
            {
                // Si se activa, pasa el texto al TextBox visible, oculta el PasswordBox y cambia el icono.
                txtPasswordVisible.Text = txtPassword.Password;
                txtPassword.Visibility = Visibility.Collapsed;
                txtPasswordVisible.Visibility = Visibility.Visible;
                btnVerPassword.Content = "🙈";
            }
            else
            {
                // Si se desactiva, devuelve el texto al PasswordBox cifrado, oculta el TextBox y restaura el icono.
                txtPassword.Password = txtPasswordVisible.Text;
                txtPasswordVisible.Visibility = Visibility.Collapsed;
                txtPassword.Visibility = Visibility.Visible;
                btnVerPassword.Content = "👁";
            }
        }

        // Filtro de entrada en tiempo real: Cancela (marca como manejado) cualquier tecla que no sea un número en el DNI.
        private void SoloNumeros_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !e.Text.All(char.IsDigit);
        }

        // Filtro de entrada en tiempo real: Cancela cualquier caracter que no sea una letra o espacio en el Nombre.
        private void SoloLetras_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !e.Text.All(c => char.IsLetter(c) || char.IsWhiteSpace(c));
        }
    }
}