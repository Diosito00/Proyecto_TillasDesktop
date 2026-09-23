using System;
using TillasDesktop.Entities.Usuarios;

namespace TillasDesktop.UI.Modelos
{
    // Hereda de ViewModelBase para que la interfaz sepa cuándo repintar un TextBox si el valor cambia en código.
    public class UsuarioViewModel : ViewModelBase
    {
        // La entidad de datos original proveniente de la base de datos (modelo puro sin lógica de interfaz).
        private readonly Usuario _usuarioPuro;

        // Constructor principal: exige una entidad de usuario. Si llega como nula (por ejemplo, para dar de alta uno nuevo), inicializa una instancia vacía.
        public UsuarioViewModel(Usuario usuario)
        {
            _usuarioPuro = usuario ?? new Usuario();
        }

        // Identificador único del usuario en el sistema.
        public int Id_Usuario
        {
            get => _usuarioPuro.Id_Usuario;
            set { _usuarioPuro.Id_Usuario = value; OnPropertyChanged(); }
        }

        // Nombre  del usuario.
        public string Nombre
        {
            get => _usuarioPuro.Nombre;
            set { _usuarioPuro.Nombre = value; OnPropertyChanged(); }
        }

        // Apellido del usuario.
        public string Apellido
        {
            get => _usuarioPuro.Apellido;
            set { _usuarioPuro.Apellido = value; OnPropertyChanged(); }
        }

        // Documento Nacional de Identidad (DNI).
        public string Dni
        {
            get => _usuarioPuro.Dni;
            set { _usuarioPuro.Dni = value; OnPropertyChanged(); }
        }

        // Correo electrónico de contacto o inicio de sesión.
        public string Email
        {
            get => _usuarioPuro.Email;
            set { _usuarioPuro.Email = value; OnPropertyChanged(); }
        }

        // Nombre de usuario (username) utilizado para loguearse en la aplicación.
        public string Nombre_Usuario
        {
            get => _usuarioPuro.Nombre_Usuario;
            set { _usuarioPuro.Nombre_Usuario = value; OnPropertyChanged(); }
        }

        // Fecha de nacimiento del usuario.
        public DateTime Fecha_Nacimiento
        {
            get => _usuarioPuro.Fecha_Nacimiento;
            set { _usuarioPuro.Fecha_Nacimiento = value; OnPropertyChanged(); }
        }

        // Rol asignado dentro del sistema (ej: Administrador, Cajero, etc.).
        public string Rol
        {
            get => _usuarioPuro.Rol;
            set { _usuarioPuro.Rol = value; OnPropertyChanged(); }
        }

        // Bandera lógica que indica si el usuario se encuentra activo o dado de baja en el sistema.
        public bool Activo
        {
            get => _usuarioPuro.Activo;
            set { _usuarioPuro.Activo = value; OnPropertyChanged(); }
        }

        // Devuelve la entidad "limpia" para que pueda ser enviada con seguridad a la capa de negocios (BLL) y luego a la base de datos.
        public Usuario ObtenerEntidadPura()
        {
            return _usuarioPuro;
        }
    }
}