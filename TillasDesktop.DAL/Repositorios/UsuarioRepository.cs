using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using TillasDesktop.DAL.Base;
using TillasDesktop.Entities.Usuarios;

namespace TillasDesktop.DAL.Repositorios
{
    
    /// Clase encargada de manejar todas las operaciones de base de datos (CRUD) para la entidad Usuario.
    /// Hereda de ConexionDb para obtener automáticamente la configuración de conexión al servidor SQL.
    
    public class UsuarioRepository : ConexionDb
    {
        
        // 1. OBTENER TODOS LOS USUARIOS (READ / LECTURA)
        public List<Usuario> ObtenerTodos()
        {
            // Crea una lista vacía donde iremos guardando cada usuario que traigamos de la base de datos.
            List<Usuario> listaUsuarios = new List<Usuario>();

            // Pide una conexión activa a la clase base utilizando la cadena configurada en App.config.
            // El bloque 'using' asegura que la conexión se cierre y libere recursos automáticamente al terminar.
            using (var conexion = ObtenerConexion())
            {
                // Abre físicamente el canal de comunicación con el servidor de base de datos.
                conexion.Open();


                // Agregamos el filtro para traer solo los activos
                string query = "SELECT * FROM Usuarios WHERE activo = 1";
              

                // Prepara el comando SQL pasándole la consulta y la conexión activa.
                using (var comando = new SqlCommand(query, conexion))
                {

                    // Ejecuta la consulta y devuelve un lector para recorrer los resultados fila por fila.
                    // El bloque 'using' garantiza que el lector también se cierre y libere al terminar.
                    using (var lector = comando.ExecuteReader())
                    {
                        // Mientras queden filas (registros) por leer en la base de datos, el ciclo avanza.
                        while (lector.Read())
                        {
                            // Creamos una nueva instancia de la entidad Usuario para mapear los datos de la fila actual.
                            var usuario = new Usuario
                            {
                                // Leemos la columna 0 (id_usuario) asegurando que sea un entero.
                                Id_Usuario = lector.GetInt32(0),

                                // Verificamos si la columna 1 (nombre) es nula en la BD; si lo es, asignamos un string vacío, de lo contrario leemos el texto.
                                Nombre = lector.IsDBNull(1) ? string.Empty : lector.GetString(1),

                                // Verificamos si la columna 2 (apellido) es nula; si es así, asignamos un string vacío.
                                Apellido = lector.IsDBNull(2) ? string.Empty : lector.GetString(2),

                                // Verificamos y leemos la columna 3 (dni).
                                Dni = lector.IsDBNull(3) ? string.Empty : lector.GetString(3),

                                // Verificamos y leemos la columna 4 (email).
                                Email = lector.IsDBNull(4) ? string.Empty : lector.GetString(4),

                                // Verificamos y leemos la columna 5 (nombre_usuario).
                                Nombre_Usuario = lector.IsDBNull(5) ? string.Empty : lector.GetString(5),

                                // Verificamos y leemos la columna 6 (password).
                                Password = lector.IsDBNull(6) ? string.Empty : lector.GetString(6),

                                // Verificamos si la fecha de nacimiento es nula; si lo es, asignamos un DateTime? nulo, de lo contrario obtenemos la fecha.
                                Fecha_Nacimiento = lector.IsDBNull(7) ? DateTime.MinValue : lector.GetDateTime(7),
                               
                                // Verificamos y leemos la columna 8 (rol).
                                Rol = lector.IsDBNull(8) ? string.Empty : lector.GetString(8),

                                // Leemos la columna 9 (activo) convirtiéndola a su valor booleano correspondiente.
                                Activo = lector.GetBoolean(9)
                            };

                            // Agrega el usuario ya armado a nuestra lista de resultados.
                            listaUsuarios.Add(usuario);
                        }
                    }
                }
            }

            // Devuelve la lista completa de usuarios hacia la capa o ViewModel que la solicitó.
            return listaUsuarios;
        }

        // 2. Obtener por ID
        public Usuario ObtenerPorId(int id)
        {
            Usuario usuario;
            string query = "SELECT * FROM Usuarios WHERE Id_Usuario = @Id";

            using (var con = ObtenerConexion())
            using (var cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                con.Open();

                using (SqlDataReader lector = cmd.ExecuteReader())
                {
                    if (lector.Read())
                    {
                        usuario = new Usuario
                        {
                            // Leemos la columna 0 (id_usuario) asegurando que sea un entero.
                            Id_Usuario = lector.GetInt32(0),

                            // Verificamos si la columna 1 (nombre) es nula en la BD; si lo es, asignamos un string vacío, de lo contrario leemos el texto.
                            Nombre = lector.IsDBNull(1) ? string.Empty : lector.GetString(1),

                            // Verificamos si la columna 2 (apellido) es nula; si es así, asignamos un string vacío.
                            Apellido = lector.IsDBNull(2) ? string.Empty : lector.GetString(2),

                            // Verificamos y leemos la columna 3 (dni).
                            Dni = lector.IsDBNull(3) ? string.Empty : lector.GetString(3),

                            // Verificamos y leemos la columna 4 (email).
                            Email = lector.IsDBNull(4) ? string.Empty : lector.GetString(4),

                            // Verificamos y leemos la columna 5 (nombre_usuario).
                            Nombre_Usuario = lector.IsDBNull(5) ? string.Empty : lector.GetString(5),

                            // Verificamos y leemos la columna 6 (password).
                            Password = lector.IsDBNull(6) ? string.Empty : lector.GetString(6),

                            // Verificamos si la fecha de nacimiento es nula; si lo es, asignamos un DateTime? nulo, de lo contrario obtenemos la fecha.
                            Fecha_Nacimiento = lector.IsDBNull(7) ? DateTime.MinValue : lector.GetDateTime(7),

                            // Verificamos y leemos la columna 8 (rol).
                            Rol = lector.IsDBNull(8) ? string.Empty : lector.GetString(8),

                            // Leemos la columna 9 (activo) convirtiéndola a su valor booleano correspondiente.
                            Activo = lector.GetBoolean(9)
                        };
                        return usuario;
                    }
                }
            }
            return null;
        }


        // 3. INSERTAR UN NUEVO USUARIO (CREATE / CREACIÓN)
        public bool Insertar(Usuario nuevoUsuario)
        {
            // Bloque try-catch para atrapar cualquier error imprevisto durante la ejecución con la base de datos.
            try
            {
                // Abre una conexión nueva utilizando el método heredado.
                using (var conexion = ObtenerConexion())
                {
                    // Consulta SQL de inserción indicando las columnas y los parámetros seguros (@).
                    string query = @"INSERT INTO Usuarios (Nombre, Apellido, Dni, Email, Nombre_Usuario, Password, Fecha_Nacimiento, Rol, Activo) 
                                     VALUES (@Nombre, @Apellido, @Dni, @Email, @Nombre_Usuario, @Password, @Fecha_Nacimiento, @Rol, @Activo)";

                    // Prepara el comando de SQL con la consulta y la conexión.
                    using (var comando = new SqlCommand(query, conexion))
                    {
                        // Asigna los valores del objeto recibido a cada parámetro SQL.
                        // Esto blinda la aplicación previniendo ataques de Inyección SQL.
                        comando.Parameters.AddWithValue("@Nombre", nuevoUsuario.Nombre);
                        comando.Parameters.AddWithValue("@Apellido", nuevoUsuario.Apellido);
                        comando.Parameters.AddWithValue("@Dni", nuevoUsuario.Dni);
                        comando.Parameters.AddWithValue("@Email", nuevoUsuario.Email);
                        comando.Parameters.AddWithValue("@Nombre_Usuario", nuevoUsuario.Nombre_Usuario);
                        comando.Parameters.AddWithValue("@Password", nuevoUsuario.Password);
                        comando.Parameters.AddWithValue("@Fecha_Nacimiento", nuevoUsuario.Fecha_Nacimiento);
                        comando.Parameters.AddWithValue("@Rol", nuevoUsuario.Rol);
                        comando.Parameters.AddWithValue("@Activo", nuevoUsuario.Activo);

                        // Abre el canal de comunicación con el servidor.
                        conexion.Open();

                        // ExecuteScalar ejecuta la consulta y devuelve la primera columna de la primera fila (nuestro nuevo ID)
                        object resultado = comando.ExecuteScalar();

                        if (resultado != null)
                        {
                            // Asignamos el ID real de la base de datos a nuestra entidad
                            nuevoUsuario.Id_Usuario = Convert.ToInt32(resultado);
                            return true;
                        }

                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                // Si ocurre un error, detiene la ejecución y lanza una excepción clara con el detalle.
                throw new Exception("Error al insertar el usuario: " + ex.Message);
            }
        }

       
        // 4. ACTUALIZAR UN USUARIO EXISTENTE (UPDATE / MODIFICACIÓN)
        
        public bool Actualizar(Usuario usuarioModificado)
        {
            // Verificamos si el campo de contraseña del formulario NO esta vacio
            bool actualizaPassword = !string.IsNullOrWhiteSpace(usuarioModificado.Password);

            // Consulta SQL para modificar los campos de un usuario específico usando su ID como filtro (WHERE).
            string query = @"UPDATE Usuarios 
                                     SET Nombre = @Nombre, 
                                         Apellido = @Apellido, 
                                         Dni = @Dni, 
                                         Email = @Email, 
                                         Nombre_Usuario = @Nombre_Usuario, 
                                         Fecha_Nacimiento = @Fecha_Nacimiento, 
                                         Rol = @Rol, 
                                         Activo = @Activo";

            // Si el campo contiene información nueva se extiende el query para actualizar la contraseña
            if (actualizaPassword) query += ", Password = @Password";

            query += " WHERE Id_Usuario = @Id_Usuario";

            try
            {
                // Obtiene la conexión a través de la clase base.
                using (var conexion = ObtenerConexion())
                {
                    // Prepara el comando SQL.
                    using (var comando = new SqlCommand(query, conexion))
                    {
                        // Asigna los valores actualizados y el identificador único a los parámetros protegidos.
                        comando.Parameters.AddWithValue("@Id_Usuario", usuarioModificado.Id_Usuario);
                        comando.Parameters.AddWithValue("@Nombre", usuarioModificado.Nombre);
                        comando.Parameters.AddWithValue("@Apellido", usuarioModificado.Apellido);
                        comando.Parameters.AddWithValue("@Dni", usuarioModificado.Dni);
                        comando.Parameters.AddWithValue("@Email", usuarioModificado.Email);
                        comando.Parameters.AddWithValue("@Nombre_Usuario", usuarioModificado.Nombre_Usuario);
                        comando.Parameters.AddWithValue("@Password", usuarioModificado.Password);
                        comando.Parameters.AddWithValue("@Fecha_Nacimiento", usuarioModificado.Fecha_Nacimiento);
                        comando.Parameters.AddWithValue("@Rol", usuarioModificado.Rol);
                        comando.Parameters.AddWithValue("@Activo", usuarioModificado.Activo);

                        // Abre la conexión con la base de datos.
                        conexion.Open();

                        // Ejecuta la actualización y guarda cuántas filas fueron modificadas.
                        int filasAfectadas = comando.ExecuteNonQuery();

                        // Retorna true si la actualización afectó a una o más filas.
                        return filasAfectadas > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                // Maneja cualquier fallo durante la actualización.
                throw new Exception("Error al actualizar el usuario: " + ex.Message);
            }
        }

        
        // 5. ELIMINAR UN USUARIO (DELETE / BORRADO)
        public bool Eliminar(int idUsuario)
        {
            try
            {
                // Establece la conexión utilizando la clase base.
                using (var conexion = ObtenerConexion())
                {
                    // Consulta SQL para borrar permanentemente un registro de la tabla según su ID.
                    string query = "UPDATE Usuarios SET activo = 0 WHERE Id_Usuario = @Id_Usuario";

                    // Prepara el comando con la consulta y la conexión.
                    using (var comando = new SqlCommand(query, conexion))
                    {
                        // Vincula el ID recibido por parámetro de manera segura.
                        comando.Parameters.AddWithValue("@Id_Usuario", idUsuario);

                        // Abre la conexión con el servidor.
                        conexion.Open();

                        // Ejecuta el comando de borrado y almacena el número de filas afectadas.
                        int filasAfectadas = comando.ExecuteNonQuery();

                        // Retorna true si el registro fue borrado correctamente.
                        return filasAfectadas > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                // Captura y reporta cualquier error en el proceso de eliminación.
                throw new Exception("Error al eliminar el usuario: " + ex.Message);
            }
        }

        // 6. OBTENER USUARIO POR CREDENCIALES (LOGIN)
        public Usuario Autenticar(string credencial, string password)
        {
            // Buscamos por Nombre_Usuario o Email, exigiendo que coincida la clave y esté activo
            string query = @"SELECT Id_Usuario, Nombre, Apellido, Rol 
                     FROM Usuarios 
                     WHERE (Nombre_Usuario = @Credencial OR Email = @Credencial) 
                     AND Password = @Password 
                     AND Activo = 1";

            try
            {
                using (var conexion = ObtenerConexion())
                using (var comando = new SqlCommand(query, conexion))
                {
                    comando.Parameters.AddWithValue("@Credencial", credencial);
                    comando.Parameters.AddWithValue("@Password", password);

                    conexion.Open();
                    using (var lector = comando.ExecuteReader())
                    {
                        if (lector.Read())
                        {
                            return new Usuario
                            {
                                Id_Usuario = lector.GetInt32(0),
                                Nombre = lector.IsDBNull(1) ? string.Empty : lector.GetString(1),
                                Apellido = lector.IsDBNull(2) ? string.Empty : lector.GetString(2),
                                Rol = lector.IsDBNull(3) ? string.Empty : lector.GetString(3)
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al intentar autenticar: " + ex.Message);
            }

            return null; // Retorna null si no encontró coincidencias
        }
    }
}