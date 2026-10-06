using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using TillasDesktop.DAL.Base;
using TillasDesktop.Entities.Clientes;

namespace TillasDesktop.DAL.Repositorios
{
    /// Clase encargada de manejar todas las operaciones de base de datos (CRUD) para la entidad Cliente.
    /// Hereda de ConexionDb para obtener automáticamente la configuración de conexión al servidor SQL.
    public class ClienteRepository : ConexionDb
    {
        // OBTENER TODOS LOS CLIENTES
        public List<Cliente> ObtenerTodos()
        {
            // Crea una lista vacía donde iremos guardando cada cliente que traigamos de la base de datos.
            List<Cliente> listaClientes = new List<Cliente>();

            // Pide una conexión activa a la clase base utilizando la cadena configurada.
            // El bloque 'using' asegura que la conexión se cierre y libere recursos automáticamente al terminar.
            using (var conexion = ObtenerConexion())
            {
                string query = "SELECT * FROM Clientes";

                // Abre físicamente el canal de comunicación con el servidor de base de datos.
                conexion.Open();

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
                            // Creamos una nueva instancia de la entidad Cliente para mapear los datos de la fila actual de forma segura.
                            var cliente = new Cliente
                            {
                                // Leemos la columna 0 (ID) asegurando que sea un entero.
                                ID = lector.GetInt32(0),

                                // Verificamos si la columna 1 (Apellido) es nula en la BD; si lo es, asignamos un string vacío, de lo contrario leemos el texto.
                                Apellido = lector.IsDBNull(1) ? string.Empty : lector.GetString(1),

                                // Verificamos si la columna 2 (Nombre) es nula; si es así, asignamos un string vacío.
                                Nombre = lector.IsDBNull(2) ? string.Empty : lector.GetString(2),

                                // Verificamos y leemos la columna 3 (CUIT).
                                CUIT = lector.IsDBNull(3) ? string.Empty : lector.GetString(3),

                                // Verificamos y leemos la columna 4 (Telefono).
                                Telefono = lector.IsDBNull(4) ? string.Empty : lector.GetString(4),

                                // Verificamos y leemos la columna 5 (Email).
                                Email = lector.IsDBNull(5) ? string.Empty : lector.GetString(5)
                            };

                            // Agrega el cliente ya armado a nuestra lista de resultados.
                            listaClientes.Add(cliente);
                        }
                    }
                }
            }

            // Devuelve la lista completa de clientes hacia la capa o ViewModel que la solicitó.
            return listaClientes;
        }

        // OBTENER POR ID
        public Cliente ObtenerPorId(int id)
        {
            Cliente cliente;
            string query = "SELECT * FROM Clientes WHERE ID = @ID";

            using (var con = ObtenerConexion())
            using (var cmd = new SqlCommand(query, con))
            {
                    cmd.Parameters.AddWithValue("@ID", id);
                    con.Open();

                    using (SqlDataReader lector = cmd.ExecuteReader())
                    {
                        if (lector.Read())
                        {
                            cliente = new Cliente
                            {
                                // Leemos la columna 0 (ID) asegurando que sea un entero.
                                ID = lector.GetInt32(0),

                                // Verificamos si la columna 1 (Apellido) es nula en la BD; si lo es, asignamos un string vacío, de lo contrario leemos el texto.
                                Apellido = lector.IsDBNull(1) ? string.Empty : lector.GetString(1),

                                // Verificamos si la columna 2 (Nombre) es nula; si es así, asignamos un string vacío.
                                Nombre = lector.IsDBNull(2) ? string.Empty : lector.GetString(2),

                                // Verificamos y leemos la columna 3 (CUIT).
                                CUIT = lector.IsDBNull(3) ? string.Empty : lector.GetString(3),

                                // Verificamos y leemos la columna 4 (Telefono).
                                Telefono = lector.IsDBNull(4) ? string.Empty : lector.GetString(4),

                                // Verificamos y leemos la columna 5 (Email).
                                Email = lector.IsDBNull(5) ? string.Empty : lector.GetString(5)
                            };
                            return cliente;
                        }
                    }
                
            }
           

            return null; // Retorna null si no se encuentra ningún registro con ese ID.
        }

        // INSERTAR UN NUEVO CLIENTE
        public bool Insertar(Cliente nuevoCliente)
        {
            // Bloque try-catch para atrapar cualquier error imprevisto durante la ejecución con la base de datos.
            try
            {
                // Abre una conexión nueva utilizando el método heredado.
                using (var conexion = ObtenerConexion())
                {
                    // Consulta SQL de inserción indicando las columnas y los parámetros seguros (@).
                    string query = @"INSERT INTO Clientes (Apellido, Nombre, CUIT, Telefono, Email) 
                                     OUTPUT INSERTED.ID
                                     VALUES (@Apellido, @Nombre, @CUIT, @Telefono, @Email)";

                    // Prepara el comando de SQL con la consulta y la conexión.
                    using (var comando = new SqlCommand(query, conexion))
                    {
                        // Asigna los valores del objeto recibido a cada parámetro SQL.
                        // Esto blinda la aplicación previniendo ataques de Inyección SQL.
                        comando.Parameters.AddWithValue("@Apellido", nuevoCliente.Apellido);
                        comando.Parameters.AddWithValue("@Nombre", nuevoCliente.Nombre);
                        comando.Parameters.AddWithValue("@CUIT", nuevoCliente.CUIT);
                        comando.Parameters.AddWithValue("@Telefono", nuevoCliente.Telefono);
                        comando.Parameters.AddWithValue("@Email", nuevoCliente.Email);

                        // Abre el canal de comunicación con el servidor.
                        conexion.Open();

                        // ExecuteScalar ejecuta la consulta y devuelve la primera columna de la primera fila (nuestro nuevo ID)
                        object resultado = comando.ExecuteScalar();

                        // Verificamos que el resultado sea válido antes de intentar convertirlo a entero
                        if (resultado != null && resultado != DBNull.Value)
                        {
                            // Asignamos el ID real de la base de datos a nuestra entidad
                            nuevoCliente.ID = Convert.ToInt32(resultado);
                            return true;
                        }

                        return false;
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                // El error 2627 o 2601 en SQL Server significa "Violación de restricción UNIQUE" (ej. CUIT o Email duplicado)
                if (sqlEx.Number == 2627 || sqlEx.Number == 2601)
                {
                    // Inspeccionamos el mensaje original para saber qué campo causó el choque
                    if (!string.IsNullOrEmpty(nuevoCliente.CUIT) && sqlEx.Message.Contains(nuevoCliente.CUIT))
                        throw new Exception("Ya existe un cliente registrado con este CUIT.");

                    if (!string.IsNullOrEmpty(nuevoCliente.Email) && sqlEx.Message.Contains(nuevoCliente.Email))
                        throw new Exception("Este correo electrónico ya está registrado por otro cliente.");

                    // Mensaje por defecto si choca otra restricción
                    throw new Exception("Un dato ingresado ya existe en el sistema y no puede duplicarse.");
                }

                // Si es otro error de SQL (ej. base de datos caída), mostramos el genérico
                throw new Exception("Error de base de datos al insertar el cliente: " + sqlEx.Message);
            }
            catch (Exception ex)
            {
                // Atrapa cualquier otro error que no provenga de SQL Server
                throw new Exception("Error inesperado al insertar el cliente: " + ex.Message);
            }
        }

        // ACTUALIZAR UN CLIENTE EXISTENTE
        public bool Actualizar(Cliente clienteModificado)
        {
            // Consulta SQL para modificar los campos de un cliente específico usando su ID como filtro (WHERE).
            string query = @"UPDATE Clientes 
                             SET Apellido = @Apellido, 
                                 Nombre = @Nombre, 
                                 CUIT = @CUIT, 
                                 Telefono = @Telefono, 
                                 Email = @Email 
                             WHERE ID = @ID";

            try
            {
                // Obtiene la conexión a través de la clase base.
                using (var conexion = ObtenerConexion())
                {
                    // Prepara el comando SQL.
                    using (var comando = new SqlCommand(query, conexion))
                    {
                        // Asigna los valores actualizados y el identificador único a los parámetros protegidos.
                        comando.Parameters.AddWithValue("@ID", clienteModificado.ID);
                        comando.Parameters.AddWithValue("@Apellido", clienteModificado.Apellido);
                        comando.Parameters.AddWithValue("@Nombre", clienteModificado.Nombre);
                        comando.Parameters.AddWithValue("@CUIT", clienteModificado.CUIT);
                        comando.Parameters.AddWithValue("@Telefono", clienteModificado.Telefono);
                        comando.Parameters.AddWithValue("@Email", clienteModificado.Email);

                        // Abre la conexión con la base de datos.
                        conexion.Open();

                        // Ejecuta la actualización y guarda cuántas filas fueron modificadas.
                        int filasAfectadas = comando.ExecuteNonQuery();

                        // Retorna true si la actualización afectó a una o más filas.
                        return filasAfectadas > 0;
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                if (sqlEx.Number == 2627 || sqlEx.Number == 2601)
                {
                    if (!string.IsNullOrEmpty(clienteModificado.CUIT) && sqlEx.Message.Contains(clienteModificado.CUIT))
                        throw new Exception("Ya existe un cliente registrado con este CUIT.");

                    if (!string.IsNullOrEmpty(clienteModificado.Email) && sqlEx.Message.Contains(clienteModificado.Email))
                        throw new Exception("Este correo electrónico ya está registrado por otro cliente.");

                    throw new Exception("Un dato ingresado ya existe en el sistema y no puede duplicarse.");
                }

                throw new Exception("Error de base de datos al actualizar el cliente: " + sqlEx.Message);
            }
            catch (Exception ex)
            {
                throw new Exception("Error inesperado al actualizar el cliente: " + ex.Message);
            }
        }

        // BAJA FISICA
        public bool Eliminar(int idCliente)
        {
            try
            {
                // Establece la conexión utilizando la clase base.
                using (var conexion = ObtenerConexion())
                {
                    // Consulta SQL para borrar un registro de la tabla según su ID.
                    string query = "DELETE FROM Clientes WHERE ID = @ID";

                    // Prepara el comando con la consulta y la conexión.
                    using (var comando = new SqlCommand(query, conexion))
                    {
                        // Vincula el ID recibido por parámetro de manera segura.
                        comando.Parameters.AddWithValue("@ID", idCliente);

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
                throw new Exception("Error al eliminar el cliente: " + ex.Message);
            }
        }
    }
}