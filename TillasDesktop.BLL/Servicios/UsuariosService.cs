using System;
using System.Collections.Generic;
using System.Windows;
using TillasDesktop.DAL.Repositorios;
using TillasDesktop.Entities.Usuarios;

namespace TillasDesktop.BLL.Services
{
    public class UsuariosService
    {
        private readonly UsuarioRepository _usuarioRepo;

        public UsuariosService()
        {
            _usuarioRepo = new UsuarioRepository();
        }

        // ==========================================
        // OBTENER TODOS
        // ==========================================
        public List<Usuario> ObtenerTodos()
        {
            // Pide los datos directamente a la DAL
            return _usuarioRepo.ObtenerTodos();
        }

        // ==========================================
        // CREAR
        // ==========================================
        public bool CrearUsuario(Usuario nuevoUsuario)
        {
            // Validaciones de negocio defensivas (por si la UI falla)
            if (string.IsNullOrWhiteSpace(nuevoUsuario.Nombre_Usuario) || string.IsNullOrWhiteSpace(nuevoUsuario.Password))
            {
                MessageBox.Show("El nombre de usuario y la contraseña son obligatorios.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // Delegar la inserción a la capa de datos
            return _usuarioRepo.Insertar(nuevoUsuario);
        }

        // ==========================================
        // ACTUALIZAR
        // ==========================================
        public bool ActualizarUsuario(Usuario usuarioActualizado)
        {
            if (usuarioActualizado.Id_Usuario <= 0)
            {
                MessageBox.Show("ID de usuario inválido.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            // El repositorio (DAL) ya se encarga de evaluar si la propiedad Password 
            // tiene texto o está en blanco para proteger la clave actual.
            return _usuarioRepo.Actualizar(usuarioActualizado);
        }

        // ==========================================
        // ELIMINAR
        // ==========================================
        public bool EliminarUsuario(int idUsuario)
        {
            if (idUsuario <= 0)
            {
                MessageBox.Show("ID de usuario inválido para eliminación.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // Podes agregar reglas de negocio, ej: "No se puede eliminar al usuario Admin principal"
            if (idUsuario == 1)
            {
                MessageBox.Show("Por seguridad, el usuario administrador principal no puede ser eliminado del sistema.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return _usuarioRepo.Eliminar(idUsuario);
        }
    }
}