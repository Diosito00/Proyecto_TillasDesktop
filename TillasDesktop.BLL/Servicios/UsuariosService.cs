using System;
using System.Collections.Generic;
using System.Windows;
using TillasDesktop.DAL.Repositorios;
using TillasDesktop.Entities.Usuarios;

namespace TillasDesktop.BLL.Services
{
    public class UsuariosService
    {
        // Instancia del repositorio encargada de las consultas y operaciones en la base de datos de usuarios.
        private readonly UsuarioRepository _usuarioRepo;

        public UsuariosService()
        {
            _usuarioRepo = new UsuarioRepository();
        }

        // Obtiene la lista completa de usuarios registrados. 
        // Funciona como una pasarela simple que comunica el ViewModel con la capa de datos (DAL).
        public List<Usuario> ObtenerTodos()
        {
            return _usuarioRepo.ObtenerTodos();
        }

        // Valida los datos obligatorios y solicita al repositorio la creación de un nuevo usuario.
        public bool CrearUsuario(Usuario nuevoUsuario)
        {
            // Tener MessageBox aquí acopla la lógica de negocio a la interfaz gráfica.
            // Lo ideal en el futuro es migrar esto al patrón 'out string mensaje' (como hicimos en InventarioService).
            if (string.IsNullOrWhiteSpace(nuevoUsuario.Nombre_Usuario) || string.IsNullOrWhiteSpace(nuevoUsuario.Password))
            {
                MessageBox.Show("El nombre de usuario y la contraseña son obligatorios.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // Delegar la inserción a la capa de datos (DAL).
            return _usuarioRepo.Insertar(nuevoUsuario);
        }

        // Valida que el ID sea correcto y procede a actualizar los datos del usuario en la base de datos.
        public bool ActualizarUsuario(Usuario usuarioActualizado)
        {
            if (usuarioActualizado.Id_Usuario <= 0)
            {
                MessageBox.Show("ID de usuario inválido.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return _usuarioRepo.Actualizar(usuarioActualizado);
        }

        // Gestiona la baja de un usuario aplicando reglas críticas de seguridad.
        public bool EliminarUsuario(int idUsuario)
        {
            if (idUsuario <= 0)
            {
                MessageBox.Show("ID de usuario inválido para eliminación.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // Regla de seguridad crítica: 
            // Evita que se pueda eliminar por error o malicia al administrador principal del sistema (ID 1).
            if (idUsuario == 1)
            {
                MessageBox.Show("Por seguridad, el usuario administrador principal no puede ser eliminado del sistema.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return _usuarioRepo.Eliminar(idUsuario);
        }


        // Valida las credenciales de acceso contra la base de datos para iniciar sesión.
        public Usuario AutenticarUsuario(string credencial, string password)
        {
            // Si llega algo vacío, ni siquiera molestamos a la base de datos con una consulta inútil :D.
            if (string.IsNullOrWhiteSpace(credencial) || string.IsNullOrWhiteSpace(password))
            {
                return null;
            }

            return _usuarioRepo.Autenticar(credencial, password);
        }
    }
}