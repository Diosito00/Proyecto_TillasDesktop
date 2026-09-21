using System;
using TillasDesktop.BLL.Services;
using TillasDesktop.Entities.Usuarios;

namespace TillasDesktop.UI.Modelos
{
    public class LoginViewModel : ViewModelBase
    {
        private readonly UsuariosService _usuarioService;

        private string _usuario;
        public string Usuario
        {
            get => _usuario;
            set { _usuario = value; OnPropertyChanged(); }
        }

        public LoginViewModel()
        {
            _usuarioService = new UsuariosService();
        }

        // Retorna el rol del usuario si el login es exitoso, o null si falla
        public string Autenticar(string password)
        {
            if (string.IsNullOrWhiteSpace(Usuario) || string.IsNullOrWhiteSpace(password))
                return null;

            string credencialLimpia = Usuario.Trim();

            // Llama a la base de datos a través del servicio
            Usuario usuarioAutenticado = _usuarioService.AutenticarUsuario(credencialLimpia, password);

            if (usuarioAutenticado != null)
            {
                // Retorna el rol real asignado en la base de datos (Ej: "Admin", "Vendedor")
                return usuarioAutenticado.Rol;
            }

            return null; // Credenciales incorrectas o usuario inactivo
        }
    }
}