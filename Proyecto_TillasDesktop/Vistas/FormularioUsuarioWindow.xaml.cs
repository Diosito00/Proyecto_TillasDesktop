
using System.Windows;
using System.Windows.Input;
using TillasDesktop.Entities.Usuarios;
using TillasDesktop.UI.Modelos;

namespace TillasDesktop.UI.Vistas
{
    // Declaración de la clase parcial FormularioUsuarioWindow, que hereda de Window (ventana WPF).
    // Funciona como la vista emergente para dar de alta un usuario nuevo o modificar uno existente.
    public partial class FormularioUsuarioWindow : Window
    {
        private FormularioUsuarioViewModel _viewModel;
        public Usuario? NuevoUsuario => _viewModel?.UsuarioResultado;

        // Constructor vacío: Se ejecuta cuando la ventana se abre para CREAR un usuario desde cero.
        public FormularioUsuarioWindow()
        {
            InitializeComponent(); // Carga y dibuja los componentes visuales definidos en el archivo XAML.
            _viewModel = new FormularioUsuarioViewModel();
            ConfigurarViewModel();

        }

        // Constructor con parámetros: Se ejecuta cuando la ventana se abre para EDITAR un usuario, recibiendo los datos actuales.
        public FormularioUsuarioWindow(Usuario usuarioAEditar)
        {
            InitializeComponent(); // Inicializa los controles visuales de la interfaz.
            _viewModel = new FormularioUsuarioViewModel(usuarioAEditar);
            ConfigurarViewModel();

            // Asignación explícita inicial para el control de contraseña seguro
            if (usuarioAEditar != null)
            {
                txtPassword.Password = usuarioAEditar.Password;
            }
        }

        private void ConfigurarViewModel()
        {
            DataContext = _viewModel;
            // Vinculamos la acción de cierre para que el ViewModel ordene cerrar la ventana con éxito
            _viewModel.CerrarVentanaAccion = (resultado) =>
            {
                DialogResult = resultado;
                Close();
            };
        }

        private bool esPasswordVisible = false;

        private void BtnVerPassword_Click(object sender, RoutedEventArgs e)
        {
            esPasswordVisible = !esPasswordVisible;

            if (esPasswordVisible)
            {
                txtPasswordVisible.Text = txtPassword.Password;
                txtPasswordVisible.Visibility = Visibility.Visible;
                txtPassword.Visibility = Visibility.Collapsed;
                btnVerPassword.Content = "👁️‍🗨️";
            }
            else
            {
                txtPassword.Password = txtPasswordVisible.Text;
                txtPassword.Visibility = Visibility.Visible;
                txtPasswordVisible.Visibility = Visibility.Collapsed;
                btnVerPassword.Content = "👁️";
            }
        }

        // Se dispara cuando escribes con el PasswordBox oculto
        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (_viewModel != null)
            {
                _viewModel.Password = txtPassword.Password;
            }
        }

        // Se dispara cuando escribes con el TextBox visible (cuando el ojito está abierto)
        private void TxtPasswordVisible_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (_viewModel != null)
            {
                _viewModel.Password = txtPasswordVisible.Text;
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