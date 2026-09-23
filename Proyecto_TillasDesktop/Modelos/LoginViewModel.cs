using System;
using TillasDesktop.BLL.Services;
using TillasDesktop.Entities.Usuarios;

namespace TillasDesktop.UI.Modelos
{
    // Hereda de ViewModelBase para mantener la pantalla sincronizada con los datos mediante notificaciones.
    public class LoginViewModel : ViewModelBase
    {
        // Instancia del servicio que maneja la lógica de negocio de usuarios para verificar las credenciales.
        private readonly UsuariosService _usuarioService;

        // Campo privado que almacena el nombre de usuario o correo que escribe la persona.
        private string _usuario;

        // Propiedad pública vinculada al cuadro de texto del usuario en la pantalla de inicio de sesión.
        public string Usuario
        {
            get => _usuario;
            set { _usuario = value; OnPropertyChanged(); }
        }

        // Constructor principal: inicializa el servicio de usuarios ni bien se abre la ventana de login.
        public LoginViewModel()
        {
            // Inicializa el servicio al momento de abrir la ventana de Login.
            _usuarioService = new UsuariosService();
        }

        // Método principal de autenticación. Recibe la contraseña directamente desde el Code-Behind (LoginWindow.xaml.cs) 
        // porque los PasswordBox de WPF no soportan Binding directo por motivos de seguridad en memoria.
        public string Autenticar(string password)
        {
            /// Validamos que no hayan dejado el usuario o la contraseña en blanco; si falta alguno, cortamos acá.
            if (string.IsNullOrWhiteSpace(Usuario) || string.IsNullOrWhiteSpace(password))
                return null;

            //  Limpieza de datos: El método Trim() elimina los espacios en blanco accidentales 
            // al principio o al final del texto (muy común al copiar y pegar un email).
            string credencialLimpia = Usuario.Trim();

            // Consultamos a la base de datos a través de la capa de servicios para chequear si el usuario y la clave existen.
            Usuario usuarioAutenticado = _usuarioService.AutenticarUsuario(credencialLimpia, password);
            
            // Si la base de datos nos devuelve un usuario válido...
            if (usuarioAutenticado != null)
            {
                // Guardamos el nombre y el ID a nivel global para usarlos en toda la sesión
                Proyecto_TillasDesktop.App.NombreUsuarioActual = $"{usuarioAutenticado.Nombre} {usuarioAutenticado.Apellido}";
                Proyecto_TillasDesktop.App.IdUsuarioActual = usuarioAutenticado.Id_Usuario;
                Proyecto_TillasDesktop.App.RolUsuarioActual = $"{usuarioAutenticado.Rol}";

                // Si el usuario existe y la clave es correcta, retornamos el rol real (Ej: "Admin", "Vendedor") 
                // para que la ventana principal sepa qué botones ocultar o mostrar.
                return usuarioAutenticado.Rol;
            }

            // Si el usuario no existe o la contraseña es incorrecta, devolvemos null para que la vista muestre el error.
            return null;
        }
    }
}