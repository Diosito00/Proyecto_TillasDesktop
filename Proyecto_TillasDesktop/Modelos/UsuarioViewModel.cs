using System;
using TillasDesktop.Entities.Usuarios;

namespace TillasDesktop.UI.Modelos
{
    public class UsuarioViewModel : ViewModelBase
    {
        private readonly Usuario _usuarioPuro;

        public UsuarioViewModel(Usuario usuario)
        {
            _usuarioPuro = usuario ?? new Usuario();
        }

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

        // Método vital para extraer la entidad limpia y mandarla a la base de datos
        public Usuario ObtenerEntidadPura()
        {
            return _usuarioPuro;
        }
    }
}