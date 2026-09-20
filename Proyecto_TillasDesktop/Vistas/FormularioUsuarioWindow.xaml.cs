
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TillasDesktop.Entities.Usuarios;
using TillasDesktop.UI.Modelos;

namespace TillasDesktop.UI.Vistas
{
    // Declaración de la clase parcial FormularioUsuarioWindow, que hereda de Window (ventana WPF).
    // Funciona como la vista emergente para dar de alta un usuario nuevo o modificar uno existente.
    public partial class FormularioUsuarioWindow : Window
    {
        // Esta variable evita un bucle infinito cuando una caja actualiza a la otra
        private bool _estaSincronizando = false;

        public FormularioUsuarioWindow()
        {
            InitializeComponent();
        }

        // =======================================================
        // 1. ALTERNAR VISIBILIDAD (BOTÓN DEL OJO)
        // =======================================================
        private void BtnVerPassword_Click(object sender, RoutedEventArgs e)
        {
            if (txtPassword.Visibility == Visibility.Visible)
            {
                // Cambiar a texto visible
                txtPassword.Visibility = Visibility.Collapsed;
                txtPasswordVisible.Visibility = Visibility.Visible;
                btnVerPassword.Content = "👁️‍🗨️";

                // Mover el cursor al final de la caja de texto
                txtPasswordVisible.Focus();
                txtPasswordVisible.CaretIndex = txtPasswordVisible.Text.Length;
            }
            else
            {
                // Cambiar a asteriscos
                txtPasswordVisible.Visibility = Visibility.Collapsed;
                txtPassword.Visibility = Visibility.Visible;
                btnVerPassword.Content = "👁️";

                txtPassword.Focus();
            }
        }

        // =======================================================
        // 2. CUANDO EL USUARIO ESCRIBE EN LOS ASTERISCOS
        // =======================================================
        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (_estaSincronizando) return;
            _estaSincronizando = true;

            // Copiamos el texto al TextBox oculto
            txtPasswordVisible.Text = txtPassword.Password;

            // Lo enviamos a nuestro ViewModel (MVVM)
            ActualizarViewModel(txtPassword.Password);

            _estaSincronizando = false;
        }

        // =======================================================
        // 3. CUANDO EL USUARIO ESCRIBE EN EL TEXTO VISIBLE
        // =======================================================
        private void TxtPasswordVisible_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_estaSincronizando) return;
            _estaSincronizando = true;

            // Copiamos el texto al PasswordBox oculto
            txtPassword.Password = txtPasswordVisible.Text;

            // Lo enviamos a nuestro ViewModel (MVVM)
            ActualizarViewModel(txtPasswordVisible.Text);

            _estaSincronizando = false;
        }

        // =======================================================
        // 4. PUENTE HACIA EL VIEWMODEL
        // =======================================================
        private void ActualizarViewModel(string claveActual)
        {
            // Verificamos si la ventana ya tiene cargado nuestro UsuarioFormViewModel
            if (this.DataContext is FormularioUsuarioViewModel viewModel)
            {
                viewModel.NuevaPassword = claveActual;
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