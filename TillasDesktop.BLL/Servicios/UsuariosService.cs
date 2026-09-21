using System;
using System.Collections.Generic;
using System.Windows;
using TillasDesktop.DAL.Repositorios;
using TillasDesktop.Entities.Usuarios;

namespace TillasDesktop.BLL.Services
{
    public class UsuariosService
    {
        // Variable de solo lectura que mantiene la conexión con el repositorio de SQL a lo largo de este servicio.
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
            // Actúa como simple pasarela: Pide los datos directamente a la DAL y los devuelve al ViewModel.
            return _usuarioRepo.ObtenerTodos();
        }

        // ==========================================
        // CREAR
        // ==========================================
        public bool CrearUsuario(Usuario nuevoUsuario)
        {
            // Tener 'MessageBox.Show' aquí en la BLL rompe el patrón de capas 
            // (la BLL no debería saber que existe una interfaz gráfica). Lo ideal sería usar el patrón 
            // 'out string mensaje' que aplicamos en InventarioService.
            if (string.IsNullOrWhiteSpace(nuevoUsuario.Nombre_Usuario) || string.IsNullOrWhiteSpace(nuevoUsuario.Password))
            {
                MessageBox.Show("El nombre de usuario y la contraseña son obligatorios.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // Delegar la inserción a la capa de datos (DAL).
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

            // Protección contra el bloqueo del sistema.
            // Impide que otro administrador borre accidentalmente (o por malicia) a la cuenta de SuperAdministrador (vo) (ID 1).
            if (idUsuario == 1)
            {
                MessageBox.Show("Por seguridad, el usuario administrador principal no puede ser eliminado del sistema.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return _usuarioRepo.Eliminar(idUsuario);
        }

        // ==========================================
        // AUTENTICAR LOGIN
        // ==========================================
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