// Define el espacio de nombres (namespace) correspondiente a las entidades de usuarios del sistema
namespace TillasDesktop.Entities.Usuarios
{
    // Declara la clase pública Usuario, que modela la información personal, de credenciales y de permisos del personal o operadores del sistema
    public class Usuario
    {
        // Propiedad entera que almacena el identificador único (ID / Clave primaria) del usuario en la base de datos
        public int Id_Usuario { get; set; }

        // Propiedad de tipo string que almacena el nombre de pila del usuario
        public string Nombre { get; set; }

        // Propiedad de tipo string que almacena el apellido del usuario
        public string Apellido { get; set; }

        // Propiedad de tipo string que almacena el DNI o documento nacional de identidad del usuario
        public string Dni { get; set; }

        // Propiedad de tipo string que almacena la dirección de correo electrónico institucional o personal del usuario
        public string Email { get; set; }

        // Propiedad de tipo string que almacena el nombre de usuario o alias (Username) utilizado para iniciar sesión en la aplicación
        public string Nombre_Usuario { get; set; }

        // Propiedad de tipo string que almacena la contraseña 
        public string Password { get; set; }

        // Propiedad de tipo DateTime que registra la fecha de nacimiento del usuario
        public DateTime Fecha_Nacimiento { get; set; }

        // Propiedad de tipo string que define el rol, perfil o nivel de privilegios del usuario dentro del sistema (ej. Administrador, Vendedor)
        public string Rol { get; set; }

        // Propiedad de tipo booleano que indica si la cuenta de usuario se encuentra activa para operar (true) o dada de baja/suspendida (false)
        public bool Activo { get; set; }
    }
}