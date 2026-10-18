using System.Windows;
using TillasDesktop.BLL.Services;

namespace TillasDesktop.UI.Vistas
{
    public partial class AutorizacionWindow : Window
    {
        public bool EstaAutorizado { get; private set; } = false;

        public AutorizacionWindow()
        {
            InitializeComponent();
            txtUsuario.Focus();
        }

        private void BtnAutorizar_Click(object sender, RoutedEventArgs e)
        {
            string usuario = txtUsuario.Text.Trim();
            string password = txtPassword.Password;

            if (string.IsNullOrEmpty(usuario) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Por favor ingresa usuario y contraseña.", "Datos Incompletos", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var usuarioService = new UsuariosService(); 
            
            var usuarioAutenticado = usuarioService.AutenticarUsuario(usuario, password); 

            if (usuarioAutenticado != null) 
            {
                if (usuarioAutenticado.Id_Usuario == Proyecto_TillasDesktop.App.IdUsuarioActual)
                {
                    MessageBox.Show("Por motivos de seguridad, no puedes autorizarte a ti mismo.\n\nSe requiere que un SEGUNDO Gerente o Administrador apruebe esta acción.",
                                    "Validación Cruzada Requerida", MessageBoxButton.OK, MessageBoxImage.Warning);

                    txtPassword.Clear();
                    txtPassword.Focus();
                    return;
                }

                string rol = usuarioAutenticado.Rol.ToLower(); 

                if (rol == "admin" || rol == "gerente")
                {
                    EstaAutorizado = true;
                    DialogResult = true;
                    Close();
                }
                else
                {
                    MessageBox.Show("El usuario ingresado no tiene los privilegios suficientes para realizar una restauración. Se requiere rol de Gerente o Admin.", "Acceso Denegado", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Usuario o contraseña incorrectos.", "Credenciales Inválidas", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}