// Importa clases fundamentales de .NET para el manejo general y tipos básicos
using System;
// Importa colecciones genéricas para manejar listas de objetos (List<T>)
using System.Collections.Generic;
// Importa las herramientas de la interfaz gráfica de WPF, específicamente para mostrar alertas visuales (MessageBox)
using System.Windows;
// Importa los repositorios de la capa de acceso a datos (DAL) para interactuar con la base de datos de usuarios
using TillasDesktop.DAL.Repositorios;
// Importa las entidades o modelos de datos correspondientes a los usuarios del sistema
using TillasDesktop.Entities.Usuarios;


// Define el espacio de nombres de la capa de lógica de negocio (BLL) para los servicios de usuarios
namespace TillasDesktop.BLL.Services
{

    // Declaración de la clase pública 'UsuariosService', encargada de aplicar las reglas de seguridad y validación antes de gestionar usuarios
    public class UsuariosService
    {
        // Instancia del repositorio encargada de las consultas y operaciones en la base de datos de usuarios.
        private readonly UsuarioRepository _usuarioRepo;

        // Constructor de la clase que se ejecuta al instanciar el servicio
        public UsuariosService()
        {
            // Inicializa la variable creando un nuevo objeto de la clase 'UsuarioRepository'
            _usuarioRepo = new UsuarioRepository();
        }

        // Obtiene la lista completa de usuarios registrados. 
        // Funciona como una pasarela simple que comunica el ViewModel con la capa de datos (DAL).
        public List<Usuario> ObtenerTodos(bool activos = true)
        {
            // Llama al repositorio para traer los usuarios desde SQL Server y los retorna
            return _usuarioRepo.ObtenerTodos(activos);
        }

        // Valida los datos obligatorios y solicita al repositorio la creación de un nuevo usuario.
        public bool CrearUsuario(Usuario nuevoUsuario)
        {
            // Validación: Verifica que el nombre de usuario y la contraseña no estén vacíos, nulos o formados solo por espacios
            if (string.IsNullOrWhiteSpace(nuevoUsuario.Nombre_Usuario) || string.IsNullOrWhiteSpace(nuevoUsuario.Password))
            {
                // Muestra una ventana de advertencia visual al operador si faltan datos obligatorios
                MessageBox.Show("El nombre de usuario y la contraseña son obligatorios.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false; // Detiene el proceso retornando false
            }

            // Si pasa las validaciones, delega la inserción física de los datos a la capa de datos (DAL) y retorna el resultado booleano.
            return _usuarioRepo.Insertar(nuevoUsuario);
        }

        // Valida que el ID sea correcto y procede a actualizar los datos del usuario en la base de datos.
        public bool ActualizarUsuario(Usuario usuarioActualizado)
        {
            // Validación: Asegura que el ID del usuario a modificar sea un número válido mayor a cero
            if (usuarioActualizado.Id_Usuario <= 0)
            {
                MessageBox.Show("ID de usuario inválido.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // Si es válido, solicita al repositorio ejecutar la actualización en la base de datos
            return _usuarioRepo.Actualizar(usuarioActualizado);
        }

        // Gestiona la baja de un usuario aplicando reglas críticas de seguridad.
        public bool EliminarUsuario(int idUsuario)
        {
            // Valida que el ID recibido sea un número positivo válido
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

            // Si supera la regla de seguridad, procede con la baja lógica a través del repositorio
            return _usuarioRepo.Eliminar(idUsuario);
        }


        // Gestiona la eliminación permanente o física (borrado definitivo de la base de datos) aplicando controles de seguridad.
        public bool EliminarUsuarioFisico(int idUsuario)
        {
            // Si el ID no es válido, retorna falso inmediatamente
            if (idUsuario <= 0) return false;

            // Bloquea de forma absoluta la eliminación permanente del usuario administrador principal (ID 1)
            if (idUsuario == 1)
            {
                MessageBox.Show("El usuario administrador principal jamás puede ser eliminado del sistema.", "Seguridad", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            // Ejecuta el borrado físico en la base de datos mediante el repositorio
            return _usuarioRepo.EliminarFisico(idUsuario);
        }

        // Valida las credenciales de acceso contra la base de datos para iniciar sesión.
        public Usuario AutenticarUsuario(string credencial, string password)
        {
            // Si llega algo vacío, ni siquiera molestamos a la base de datos con una consulta inútil :D.
            if (string.IsNullOrWhiteSpace(credencial) || string.IsNullOrWhiteSpace(password))
            {
                return null; // Retorna null indicando credenciales vacías
            }

            // Consulta al repositorio si existe un usuario que coincida con las credenciales ingresadas
            return _usuarioRepo.Autenticar(credencial, password);
        }

        // Valida si un correo electrónico ya está registrado y pertenece a un usuario activo.
        public bool VerificarEmailExistente(string email)
        {
            // Si el email está vacío, retorna falso
            if (string.IsNullOrWhiteSpace(email)) return false;

            // Obtenemos todos los usuarios activos de la base de datos
            var usuarios = ObtenerTodos();
            bool resultado = usuarios.Exists(u => u.Email != null && u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
            // Comparamos ignorando mayúsculas y minúsculas para evitar falsos negativos
            return usuarios.Exists(u => u.Email != null && u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        }
    }
}