// Importación de las librerías necesarias de Windows Presentation Foundation (WPF) para la gestión de ventanas y controles
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TillasDesktop.UI.Modelos;

// Declaración del espacio de nombres que agrupa las vistas de la interfaz de usuario del proyecto
namespace TillasDesktop.UI.Vistas
{
    // Definición de la clase parcial 'LoginWindow' que representa la ventana de inicio de sesión
    public partial class LoginWindow : Window
    {
        // Variable booleana de control para saber si la contraseña se encuentra visible (true) u oculta (false)
        private bool esPasswordVisible = false;
        private LoginViewModel _viewModel;

        // Constructor de la ventana: Se ejecuta automáticamente al inicializar la clase
        public LoginWindow()
        {
            // Método generado por WPF que carga y conecta los elementos visuales definidos en el archivo XAML
            InitializeComponent();
            _viewModel = new LoginViewModel();
            this.DataContext = _viewModel;
        }

        // 1. Permite arrastrar la ventana al mantener el clic presionado sobre la barra
        private void BarraSuperior_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        // 2. Botón de minimizar
        private void BtnMinimizar_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        // 3. Botón de cerrar
        private void BtnCerrar_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        // Método que se ejecuta al hacer clic en el botón del "ojito" para alternar la visibilidad de la clave
        private void BtnVerPassword_Click(object sender, RoutedEventArgs e)
        {
            // Invierte el estado actual (si era falso pasa a verdadero, y viceversa)
            esPasswordVisible = !esPasswordVisible;

            // Condicional cuando el usuario decide MOSTRAR la contraseña en texto plano
            if (esPasswordVisible)
            {
                // Copia el texto secreto del PasswordBox al TextBox normal para que sea legible
                txtPasswordVisible.Text = txtPassword.Password;
                // Hace visible el TextBox de texto plano
                txtPasswordVisible.Visibility = Visibility.Visible;
                // Oculta el PasswordBox cifrado
                txtPassword.Visibility = Visibility.Collapsed;
                // Cambia el icono del botón para reflejar que la contraseña está visible
                txtIconoOjo.Text = "👁️‍🗨️";
            }
            // Condicional cuando el usuario decide OCULTAR nuevamente la contraseña
            else
            {
                // Copia el texto plano del TextBox de regreso al PasswordBox para ocultarlo con asteriscos
                txtPassword.Password = txtPasswordVisible.Text;
                // Hace visible nuevamente el PasswordBox cifrado
                txtPassword.Visibility = Visibility.Visible;
                // Oculta el TextBox de texto plano
                txtPasswordVisible.Visibility = Visibility.Collapsed;
                // Restaura el icono original del botón del ojito
                txtIconoOjo.Text = "👁️";
            }
        }

        // Método que se ejecuta al hacer clic en el botón principal para intentar iniciar sesión
        private void BtnIniciarSesion_Click(object sender, RoutedEventArgs e)
        {
            // Obtiene el texto ingresado en el campo de usuario
            string usuario = txtUsuario.Text;

            // Obtiene la contraseña evaluando qué control está activo en ese momento (si está visible u oculto)
            string password = esPasswordVisible ? txtPasswordVisible.Text : txtPassword.Password;

            string rolConfirmado = _viewModel.Autenticar(password);

            // Validación básica para verificar si alguno de los campos obligatorios está vacío o en blanco
            if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(password))
            // Muestra una ventana emergente de advertencia al usuario si falta información
            {
                MessageBox.Show("Por favor, complete todos los campos.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                // Detiene la ejecución para evitar que continúe el inicio de sesión
                return;
            }

            if (rolConfirmado != null)
            {
                // Instancia la ventana del menú principal del sistema
                MenuInicioView ventanaPrincipal = new MenuInicioView(rolConfirmado);
                // Muestra la ventana principal en pantalla
                ventanaPrincipal.Show();

                // Cierra de forma limpia la ventana de inicio de sesión actual
                this.Close();
            }
            else
            {
                MessageBox.Show("Usuario o contraseña incorrectos. Intente con vendedor, gerente o admin (clave: 123).", "Error de Acceso", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}