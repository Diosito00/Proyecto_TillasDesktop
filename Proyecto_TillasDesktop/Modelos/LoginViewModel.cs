using System;
using TillasDesktop.BLL.Services;
using TillasDesktop.Entities.Usuarios;

namespace TillasDesktop.UI.Modelos
{
    // Hereda de ViewModelBase para mantener el estándar MVVM, permitiendo actualizar la UI si es necesario.
    public class LoginViewModel : ViewModelBase
    {
        // Instancia del servicio que se comunicará con la capa de datos (DAL) para verificar las credenciales.
        private readonly UsuariosService _usuarioService;

        // Propiedad bindeada (Binding) al TextBox del nombre de usuario o email en el XAML.
        private string _usuario;
        public string Usuario
        {
            get => _usuario;
            set { _usuario = value; OnPropertyChanged(); }
        }

        public LoginViewModel()
        {
            // Inicializa el servicio al momento de abrir la ventana de Login.
            _usuarioService = new UsuariosService();
        }

        // Método principal de autenticación. Recibe la contraseña directamente desde el Code-Behind (LoginWindow.xaml.cs) 
        // porque los PasswordBox de WPF no soportan Binding directo por motivos de seguridad en memoria.
        public string Autenticar(string password)
        {
            // 1. Validación rápida: Si dejaron algún campo vacío, cortamos el proceso al instante.
            if (string.IsNullOrWhiteSpace(Usuario) || string.IsNullOrWhiteSpace(password))
                return null;

            // 2. Limpieza de datos: El método Trim() elimina los espacios en blanco accidentales 
            // al principio o al final del texto (muy común al copiar y pegar un email).
            string credencialLimpia = Usuario.Trim();

            // 3. Consulta a la base de datos a través del servicio de la capa BLL.
            Usuario usuarioAutenticado = _usuarioService.AutenticarUsuario(credencialLimpia, password);

            // 4. Resolución.
            if (usuarioAutenticado != null)
            {
                // Guardamos el nombre y el ID a nivel global para usarlos en toda la sesión
                Proyecto_TillasDesktop.App.NombreUsuarioActual = $"{usuarioAutenticado.Nombre} {usuarioAutenticado.Apellido}";
                Proyecto_TillasDesktop.App.IdUsuarioActual = usuarioAutenticado.Id_Usuario; // Vital para registrar quién hizo la venta en SQL

                // Si el usuario existe y la clave es correcta, retornamos el rol real (Ej: "Admin", "Vendedor") 
                // para que la ventana principal sepa qué botones ocultar o mostrar.
                return usuarioAutenticado.Rol;
            }

            // Si retorna null, significa que las credenciales son incorrectas o el usuario fue desactivado.
            return null;
        }
    }
}