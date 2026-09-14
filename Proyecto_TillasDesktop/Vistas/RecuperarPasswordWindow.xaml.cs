using System.Text.RegularExpressions;
using System.Windows;

namespace TillasDesktop.UI.Vistas
{
    public partial class RecuperarPasswordWindow : Window
    {
        public RecuperarPasswordWindow()
        {
            InitializeComponent();
        }

        private void BtnEnviar_Click(object sender, RoutedEventArgs e)
        {
            string email = txtEmailRecuperacion.Text.Trim();

            // Validar que no esté vacío
            if (string.IsNullOrWhiteSpace(email))
            {
                MessageBox.Show("Por favor, ingrese su correo electrónico.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Validar formato de email
            string patronEmail = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            if (!Regex.IsMatch(email, patronEmail))
            {
                MessageBox.Show("El formato del correo electrónico no es válido.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Simulación de envío exitoso
            MessageBox.Show("Se han enviado las instrucciones de recuperación a su correo electrónico.", "Recuperación Exitosa", MessageBoxButton.OK, MessageBoxImage.Information);
            this.Close();
        }

        private void BtnVolver_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}