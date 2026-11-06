// Importa el espacio de nombres fundamental de .NET para tipos base y utilidades generales
using System;
// Importa las entidades del dominio de usuarios (específicamente la clase base Usuario)
using TillasDesktop.Entities.Usuarios;

// Define el espacio de nombres (namespace) correspondiente a los modelos de vista de la interfaz gráfica del sistema
namespace TillasDesktop.UI.Modelos
{
    // Declara la clase pública UsuarioViewModel que hereda de ViewModelBase, encargada de envolver a la entidad pura Usuario para mantener la pantalla sincronizada mediante notificaciones de cambios (Patrón MVVM).
    public class UsuarioViewModel : ViewModelBase
    {
        // La entidad de datos "pura" (modelo original sin lógica de interfaz ni dependencias de WPF) que luego se enviará de forma limpia a las capas BLL y DAL.
        private readonly Usuario _usuarioPuro;

        // Constructor principal: exige una entidad de usuario. Si llega como nula (por ejemplo, al dar de alta un operador nuevo), instancia de manera preventiva un objeto Usuario vacío.
        public UsuarioViewModel(Usuario usuario)
        {
            _usuarioPuro = usuario ?? new Usuario();
        }

        // Identificador numérico único del usuario en el sistema, enlazado con la entidad interna y notificación de cambios.
        public int Id_Usuario
        {
            get => _usuarioPuro.Id_Usuario;
            set { _usuarioPuro.Id_Usuario = value; OnPropertyChanged(); }
        }

        // Nombre de pila del operador, con notificación automática a la interfaz gráfica.
        public string Nombre
        {
            get => _usuarioPuro.Nombre;
            set { _usuarioPuro.Nombre = value; OnPropertyChanged(); }
        }

        // Apellido del operador, con notificación automática a la interfaz gráfica.
        public string Apellido
        {
            get => _usuarioPuro.Apellido;
            set { _usuarioPuro.Apellido = value; OnPropertyChanged(); }
        }

        // Documento Nacional de Identidad (DNI) del operador, con notificación de cambios.
        public string Dni
        {
            get => _usuarioPuro.Dni;
            set { _usuarioPuro.Dni = value; OnPropertyChanged(); }
        }

        // Correo electrónico de contacto o de acceso al sistema, con notificación de cambios.
        public string Email
        {
            get => _usuarioPuro.Email;
            set { _usuarioPuro.Email = value; OnPropertyChanged(); }
        }

        // Nombre de usuario (username) utilizado para iniciar sesión en la aplicación, con notificación de cambios.
        public string Nombre_Usuario
        {
            get => _usuarioPuro.Nombre_Usuario;
            set { _usuarioPuro.Nombre_Usuario = value; OnPropertyChanged(); }
        }

        // Fecha de nacimiento del operador, utilizada para validaciones de mayoría de edad, con notificación de cambios.
        public DateTime Fecha_Nacimiento
        {
            get => _usuarioPuro.Fecha_Nacimiento;
            set { _usuarioPuro.Fecha_Nacimiento = value; OnPropertyChanged(); }
        }

        // Rol o perfil de seguridad asignado dentro del sistema (ej: Administrador, Gerente, Vendedor), con notificación de cambios.
        public string Rol
        {
            get => _usuarioPuro.Rol;
            set { _usuarioPuro.Rol = value; OnPropertyChanged(); }
        }

        // Bandera lógica booleana que indica si el usuario se encuentra activo o dado de baja lógica en el sistema.
        public bool Activo
        {
            get => _usuarioPuro.Activo;
            set { _usuarioPuro.Activo = value; OnPropertyChanged(); }
        }

        // Método clave de diseño que "desempaqueta" y retorna la entidad pura subyacente para poder enviarla con total seguridad 
        // hacia la Capa de Negocios (BLL) y luego a la base de datos sin contaminarla con propiedades visuales exclusivas de la UI.
        public Usuario ObtenerEntidadPura()
        {
            return _usuarioPuro;
        }
    }
}