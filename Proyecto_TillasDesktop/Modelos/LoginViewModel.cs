// Importa el espacio de nombres fundamental de .NET para tipos base y utilidades generales
using System;
// Importa los servicios de la Capa de Negocios (BLL) encargados de la gestión de usuarios y seguridad
using TillasDesktop.BLL.Services;
// Importa las entidades del dominio de usuarios (modelo Usuario)
using TillasDesktop.Entities.Usuarios;

// Define el espacio de nombres (namespace) correspondiente a los modelos de vista de la interfaz gráfica del sistema
namespace TillasDesktop.UI.Modelos
{
    // Hereda de ViewModelBase para mantener la pantalla sincronizada con los datos mediante notificaciones (Patrón MVVM).
    public class LoginViewModel : ViewModelBase
    {
        // Instancia de solo lectura del servicio que maneja la lógica de negocio de usuarios para verificar las credenciales.
        private readonly UsuariosService _usuarioService;

        // Campo privado que almacena el nombre de usuario o correo electrónico que ingresa la persona.
        private string _usuario;

        // Propiedad pública vinculada al cuadro de texto del usuario en la pantalla de inicio de sesión con notificación de cambios.
        public string Usuario
        {
            get => _usuario;
            set { _usuario = value; OnPropertyChanged(); } // Notifica a la interfaz cuando el texto cambia.
        }

        // Constructor principal: inicializa el servicio de usuarios ni bien se abre la ventana de inicio de sesión (Login).
        public LoginViewModel()
        {
            // Inicializa el servicio de negocio al momento de instanciar el ViewModel de Login.
            _usuarioService = new UsuariosService();
        }

        // Método principal de autenticación. Recibe la contraseña en texto claro enviada desde el Code-Behind (LoginWindow.xaml.cs) 
        // debido a que los controles PasswordBox de WPF no admiten enlace de datos (Binding) directo por razones de seguridad en memoria.
        public string Autenticar(string password)
        {
            // Validamos que el operador no haya dejado los campos de usuario o contraseña vacíos o en blanco; de ser así, cancelamos el proceso.
            if (string.IsNullOrWhiteSpace(Usuario) || string.IsNullOrWhiteSpace(password))
                return null;

            // Limpieza de datos: El método Trim() remueve espacios en blanco accidentales al inicio o final de la cadena (típicos al copiar/pegar).
            string credencialLimpia = Usuario.Trim();

            // Consultamos la base de datos mediante la capa de servicios para verificar si existe un operador activo con dicha credencial y clave.
            Usuario usuarioAutenticado = _usuarioService.AutenticarUsuario(credencialLimpia, password);

            // Si la capa de negocio y la base de datos confirman la validez y retornan un objeto de usuario válido...
            if (usuarioAutenticado != null)
            {
                // Registramos las credenciales y datos de perfil en las variables globales de la aplicación para mantener el contexto durante toda la sesión.
                Proyecto_TillasDesktop.App.NombreUsuarioActual = $"{usuarioAutenticado.Nombre} {usuarioAutenticado.Apellido}";
                Proyecto_TillasDesktop.App.IdUsuarioActual = usuarioAutenticado.Id_Usuario;
                Proyecto_TillasDesktop.App.RolUsuarioActual = $"{usuarioAutenticado.Rol}";

                // Si la autenticación es exitosa, retornamos el rol asignado al operador (ej: "Admin", "Gerente", "Vendedor") 
                // para permitir que la ventana principal configure la visibilidad de sus módulos.
                return usuarioAutenticado.Rol;
            }

            // Si las credenciales no coinciden o el usuario no existe/está inactivo, retornamos null para que la vista notifique el error de acceso.
            return null;
        }
    }
}