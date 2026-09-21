using System;
using TillasDesktop.Entities.Usuarios;

namespace TillasDesktop.UI.Modelos
{
    // Hereda de ViewModelBase para que la interfaz sepa cuándo repintar un TextBox si el valor cambia en código.
    public class UsuarioViewModel : ViewModelBase
    {
        // La entidad original proveniente de la base de datos.
        private readonly Usuario _usuarioPuro;

        // El constructor exige una entidad. Si llega nula (ej: al crear un usuario nuevo), inicializa una vacía.
        public UsuarioViewModel(Usuario usuario)
        {
            _usuarioPuro = usuario ?? new Usuario();
        }

        // ==========================================
        // PROPIEDADES ENLAZADAS (BINDING)
        // Cada 'set' actualiza la entidad original y dispara OnPropertyChanged() para notificar a la vista.
        // ==========================================
        public int Id_Usuario
        {
            get => _usuarioPuro.Id_Usuario;
            set { _usuarioPuro.Id_Usuario = value; OnPropertyChanged(); }
        }

        public string Nombre
        {
            get => _usuarioPuro.Nombre;
            set { _usuarioPuro.Nombre = value; OnPropertyChanged(); }
        }

        public string Apellido
        {
            get => _usuarioPuro.Apellido;
            set { _usuarioPuro.Apellido = value; OnPropertyChanged(); }
        }

        public string Dni
        {
            get => _usuarioPuro.Dni;
            set { _usuarioPuro.Dni = value; OnPropertyChanged(); }
        }

        public string Email
        {
            get => _usuarioPuro.Email;
            set { _usuarioPuro.Email = value; OnPropertyChanged(); }
        }

        public string Nombre_Usuario
        {
            get => _usuarioPuro.Nombre_Usuario;
            set { _usuarioPuro.Nombre_Usuario = value; OnPropertyChanged(); }
        }

        public DateTime Fecha_Nacimiento
        {
            get => _usuarioPuro.Fecha_Nacimiento;
            set { _usuarioPuro.Fecha_Nacimiento = value; OnPropertyChanged(); }
        }

        public string Rol
        {
            get => _usuarioPuro.Rol;
            set { _usuarioPuro.Rol = value; OnPropertyChanged(); }
        }

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