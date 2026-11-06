// Importa clases fundamentales del sistema (como clases base de excepciones y tipos de datos)
using System;
// Importa colecciones genéricas para manejar listas de objetos (List<T>)
using System.Collections.Generic;
// Importa el proveedor oficial de SQL Server para .NET (Microsoft.Data.SqlClient) para ejecutar comandos contra la base de datos
using Microsoft.Data.SqlClient;
// Importa la clase base de conexión de la capa de acceso a datos (DAL) para heredar el método de obtención de conexión
using TillasDesktop.DAL.Base;
// Importa las entidades o modelos de datos del inventario (Producto, ProductoTalle, Marca, Categoria)
using TillasDesktop.Entities.Inventario;


// Define el espacio de nombres de los repositorios dentro de la capa de acceso a datos (DAL)
namespace TillasDesktop.DAL.Repositorios
{
    // Clase de acceso a datos unificada para el manejo de todo el inventario.
    // Hereda de ConexionDb para administrar de forma segura los canales de comunicación con SQL Server.
    public class InventarioRepository : ConexionDb
    {
        // Método unificado para consultar productos filtrando por su estado (activo o inactivo)
        public List<Producto> ObtenerProductos(bool activos = true)
        {
            // Instanciamos una lista genérica donde iremos recolectando los registros mapeados.
            List<Producto> listaProductos = new List<Producto>();

            // Definimos la sentencia de selección SQL filtrando únicamente aquellos productos cuyo estado sea activo 
            string query = "SELECT ID, Codigo_Modelo, Nombre, Marca_ID, Categoria_ID, Precio_Venta, Activo FROM Productos WHERE Activo = @Activo";

            // Solicitamos una conexión abierta a la base de datos utilizando el patrón using para garantizar la liberación de recursos.
            using (var conexion = ObtenerConexion())
            {
                // Preparamos el comando SQL vinculando la instrucción y la conexión activa.
                using (var comando = new SqlCommand(query, conexion))
                {
                    // Añade el parámetro booleano de forma segura
                    comando.Parameters.AddWithValue("@Activo", activos);

                    // Abrimos físicamente el canal con el motor de base de datos.
                    conexion.Open();

                    // Ejecutamos la consulta a través de un lector de datos de flujo continuo.
                    using (var lector = comando.ExecuteReader())
                    {
                        // Iteramos renglón por renglón mientras existan registros devueltos por la base de datos.
                        while (lector.Read())
                        {
                            // Mapea y agrega cada registro de la base de datos a un nuevo objeto Producto dentro de la lista
                            listaProductos.Add(new Producto
                            {
                                ID = lector.GetInt32(0),
                                Codigo_Modelo = lector.IsDBNull(1) ? string.Empty : lector.GetString(1),
                                Nombre = lector.IsDBNull(2) ? string.Empty : lector.GetString(2),
                                Marca_ID = lector.GetInt32(3),
                                Categoria_ID = lector.GetInt32(4),
                                Precio_Venta = lector.GetDecimal(5),
                                Activo = lector.GetBoolean(6)
                            });
                           
                        }
                    }
                }
            }

            // Retornamos la colección completa de productos listada.
            return listaProductos;
        }

        

        // Método para dar de alta un producto nuevo en la base de datos devolviendo el ID generado automáticamente.
        public bool Insertar(Producto nuevoProducto)
        {
            try
            {
                // Abre un canal de conexión utilizando el bloque using para garantizar su cierre automático
                using (var conexion = ObtenerConexion())
                {
                    // La instrucción OUTPUT INSERTED.ID nos permite capturar de inmediato el ID autoincremental que asigna SQL Server.
                    string query = @"INSERT INTO Productos (Codigo_Modelo, Nombre, Marca_ID, Categoria_ID, Precio_Venta, Activo) 
                                     OUTPUT INSERTED.ID
                                     VALUES (@Codigo_Modelo, @Nombre, @Marca_ID, @Categoria_ID, @Precio_Venta, @Activo)";

                    // Prepara el comando SQL con la consulta y la conexión abierta
                    using (var comando = new SqlCommand(query, conexion))
                    {
                        // Asigna los valores del objeto recibido a cada parámetro SQL.
                        // Esto blinda la aplicación previniendo ataques de Inyección SQL.
                        comando.Parameters.AddWithValue("@Codigo_Modelo", nuevoProducto.Codigo_Modelo);
                        comando.Parameters.AddWithValue("@Nombre", nuevoProducto.Nombre);
                        comando.Parameters.AddWithValue("@Marca_ID", nuevoProducto.Marca_ID);
                        comando.Parameters.AddWithValue("@Categoria_ID", nuevoProducto.Categoria_ID);
                        comando.Parameters.AddWithValue("@Precio_Venta", nuevoProducto.Precio_Venta);
                        comando.Parameters.AddWithValue("@Activo", nuevoProducto.Activo);

                        // Abre físicamente la conexión con la base de datos
                        conexion.Open();
                        // Ejecuta la consulta devolviendo la primera columna de la primera fila (el ID recién generado)
                        object resultado = comando.ExecuteScalar();

                        // Verifica que el resultado obtenido no sea nulo
                        if (resultado != null && resultado != DBNull.Value)
                        {
                            // Inyectamos el ID real obtenido directamente en la entidad en memoria.
                            nuevoProducto.ID = Convert.ToInt32(resultado);
                            return true; // Retorna true indicando inserción exitosa
                        }
                        return false; // Retorna false si no se pudo obtener el ID
                    }
                }
            }
            // Captura errores específicos provenientes del motor de base de datos SQL Server
            catch (SqlException sqlEx)
            {
                // Controlamos códigos de error específicos de SQL Server, como la violación de restricciones de índice único (duplicación de SKU).
                if (sqlEx.Number == 2627 || sqlEx.Number == 2601)
                {
                    throw new Exception("Ya existe un producto registrado con este Código de Modelo (SKU).");
                }
                // Si es otro error de SQL, lanza una excepción descriptiva
                throw new Exception("Error de base de datos al insertar el producto: " + sqlEx.Message);
            }
            // Captura cualquier otro error general imprevisto
            catch (Exception ex)
            {
                throw new Exception("Error inesperado al insertar el producto: " + ex.Message);
            }
        }

        // Método para actualizar los datos generales de un producto preexistente.
        public bool Actualizar(Producto productoModificado)
        {
            // Consulta SQL de actualización (UPDATE) filtrando por el identificador único del producto
            string query = @"UPDATE Productos 
                             SET Codigo_Modelo = @Codigo_Modelo, 
                                 Nombre = @Nombre, 
                                 Marca_ID = @Marca_ID, 
                                 Categoria_ID = @Categoria_ID, 
                                 Precio_Venta = @Precio_Venta, 
                                 Activo = @Activo 
                             WHERE ID = @ID";

            try
            {
                // Abre la conexión mediante el bloque using
                using (var conexion = ObtenerConexion())
                {
                    // Prepara el comando SQL asociado a la conexión
                    using (var comando = new SqlCommand(query, conexion))
                    {
                        // Asigna los valores del objeto recibido a cada parámetro SQL.
                        // Esto blinda la aplicación previniendo ataques de Inyección SQL.
                        comando.Parameters.AddWithValue("@ID", productoModificado.ID);
                        comando.Parameters.AddWithValue("@Codigo_Modelo", productoModificado.Codigo_Modelo);
                        comando.Parameters.AddWithValue("@Nombre", productoModificado.Nombre);
                        comando.Parameters.AddWithValue("@Marca_ID", productoModificado.Marca_ID);
                        comando.Parameters.AddWithValue("@Categoria_ID", productoModificado.Categoria_ID);
                        comando.Parameters.AddWithValue("@Precio_Venta", productoModificado.Precio_Venta);
                        comando.Parameters.AddWithValue("@Activo", productoModificado.Activo);

                        // Abre la conexión con el servidor
                        conexion.Open();
                        // ExecuteNonQuery retorna la cantidad de filas afectadas; si es mayor a cero, la modificación fue exitosa.
                        return comando.ExecuteNonQuery() > 0;
                    }
                }
            }
            // Captura violaciones de claves únicas (SKU duplicado) en la actualización
            catch (SqlException sqlEx)
            {
                if (sqlEx.Number == 2627 || sqlEx.Number == 2601)
                {
                    throw new Exception("Ya existe un producto registrado con este Código de Modelo (SKU).");
                }
                throw new Exception("Error de base de datos al actualizar el producto: " + sqlEx.Message);
            }
            catch (Exception ex)
            {
                throw new Exception("Error inesperado al actualizar el producto: " + ex.Message);
            }
        }

        // Método para realizar la baja lógica de un producto modificando su estado a inactivo (Activo = 0).
        public bool Eliminar(int idProducto)
        {
            try
            {
                // Inicia la conexión utilizando un bloque using
                using (var conexion = ObtenerConexion())
                {
                    // Consulta SQL para realizar la baja lógica cambiando el booleano 'Activo' a falso (0)
                    string query = "UPDATE Productos SET Activo = 0 WHERE ID = @ID";
                    // Prepara el comando SQL
                    using (var comando = new SqlCommand(query, conexion))
                    {
                        // Asigna de forma segura el ID del producto por parámetro
                        comando.Parameters.AddWithValue("@ID", idProducto);
                        // Abre el canal de comunicación
                        conexion.Open();
                        // Ejecuta la actualización y retorna true si se modificó al menos una fila
                        return comando.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                // Captura y encapsula cualquier error en la baja lógica
                throw new Exception("Error al eliminar el producto: " + ex.Message);
            }
        }

        // Método para realizar la baja física (eliminación permanente) de la base de datos
        public bool EliminarPermanente(int idProducto)
        {
            try
            {
                // Abre la conexión utilizando el patrón using
                using (var conexion = ObtenerConexion())
                {
                    // Sentencia SQL para borrar físicamente el registro de la tabla
                    string query = "DELETE FROM Productos WHERE ID = @ID";
                    // Prepara el comando SQL
                    using (var comando = new SqlCommand(query, conexion))
                    {
                        // Asigna el ID del producto de forma segura
                        comando.Parameters.AddWithValue("@ID", idProducto);
                        // Abre la conexión con el servidor
                        conexion.Open();
                        // Ejecuta el borrado y retorna true si afectó registros
                        return comando.ExecuteNonQuery() > 0;
                    }
                }

            }
            // Captura errores específicos de SQL Server al intentar borrar registros con dependencias
            catch (SqlException sqlEx)
            {
                // Si el producto ya se vendió en el pasado o tiene registros asociados, SQL bloqueará el DELETE por Clave Foránea
                if (sqlEx.Number == 547)
                {
                    throw new Exception("No se puede eliminar permanentemente este producto porque posee historial registrado en ventas o reservas.");
                }
                throw new Exception("Error al eliminar el producto: " + sqlEx.Message);
            }
        }

        // Método para consultar todos los talles y stock físico vinculados a un ID de producto específico.
        public List<ProductoTalle> ObtenerTallesPorProducto(int productoId)
        {
            // Crea una lista genérica vacía para almacenar los registros de talles recuperados
            List<ProductoTalle> listaTalles = new List<ProductoTalle>();

            // Consulta SQL para seleccionar los campos de la tabla de talles filtrando por el ID del producto
            string query = "SELECT ID, Producto_ID, Talle, Stock_Actual FROM Producto_Talles WHERE Producto_ID = @Producto_ID";

            // Abre una conexión utilizando el bloque using para garantizar su liberación automática
            using (var conexion = ObtenerConexion())
            {
                // Prepara el comando SQL asociado a la conexión
                using (var comando = new SqlCommand(query, conexion))
                {
                    // Asigna de forma segura el parámetro del ID del producto
                    comando.Parameters.AddWithValue("@Producto_ID", productoId);
                    // Abre físicamente el canal de comunicación con SQL Server
                    conexion.Open();

                    // Ejecuta la consulta mediante un lector de flujo continuo
                    using (var lector = comando.ExecuteReader())
                    {
                        // Lee registro por registro mientras existan filas en el resultado
                        while (lector.Read())
                        {
                            // Mapea cada fila de la base de datos a un objeto ProductoTalle y lo agrega a la lista
                            listaTalles.Add(new ProductoTalle
                            {
                                ID = lector.GetInt32(0),
                                Producto_ID = lector.GetInt32(1),
                                Talle = lector.GetInt32(2),
                                Stock_Actual = lector.GetInt32(3)
                            });
                        }
                    }
                }
            }
            // Retorna la lista completa de talles asociados al producto
            return listaTalles;
        }

        // Método para registrar el stock de un talle nuevo asociado a un producto.
        public bool InsertarTalleStock(ProductoTalle nuevoTalle)
        {
            try
            {
                // Inicia la conexión mediante el patrón using
                using (var conexion = ObtenerConexion())
                {
                    // Sentencia SQL para insertar un nuevo talle y capturar inmediatamente su ID autoincremental con OUTPUT INSERTED.ID
                    string query = @"INSERT INTO Producto_Talles (Producto_ID, Talle, Stock_Actual) 
                                     OUTPUT INSERTED.ID
                                     VALUES (@Producto_ID, @Talle, @Stock_Actual)";

                    // Prepara el comando SQL
                    using (var comando = new SqlCommand(query, conexion))
                    {
                        // Asigna los parámetros correspondientes de forma segura
                        comando.Parameters.AddWithValue("@Producto_ID", nuevoTalle.Producto_ID);
                        comando.Parameters.AddWithValue("@Talle", nuevoTalle.Talle);
                        comando.Parameters.AddWithValue("@Stock_Actual", nuevoTalle.Stock_Actual);

                        // Abre la conexión y ejecuta la consulta escalar para obtener el nuevo ID
                        conexion.Open();
                        object resultado = comando.ExecuteScalar();

                        // Verifica que se haya devuelto un ID válido
                        if (resultado != null && resultado != DBNull.Value)
                        {
                            // Asigna el ID real generado al objeto en memoria y retorna true
                            nuevoTalle.ID = Convert.ToInt32(resultado);
                            return true;
                        }
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                // Captura y encapsula cualquier error ocurrido durante el registro del stock
                throw new Exception("Error al registrar el stock del talle: " + ex.Message);
            }
        }

        // Método para modificar la cantidad o el número de un talle ya existente.
        public bool ActualizarTalleStock(ProductoTalle talleModificado)
        {
            try
            {
                // Abre la conexión con la base de datos
                using (var conexion = ObtenerConexion())
                {
                    // Sentencia SQL de actualización (UPDATE) para modificar el talle y su stock actual según el ID
                    string query = @"UPDATE Producto_Talles 
                                     SET Talle = @Talle, Stock_Actual = @Stock_Actual 
                                     WHERE ID = @ID";

                    // Prepara el comando SQL
                    using (var comando = new SqlCommand(query, conexion))
                    {
                        // Asigna de forma segura los parámetros de actualización
                        comando.Parameters.AddWithValue("@ID", talleModificado.ID);
                        comando.Parameters.AddWithValue("@Talle", talleModificado.Talle);
                        comando.Parameters.AddWithValue("@Stock_Actual", talleModificado.Stock_Actual);

                        // Abre la conexión física y ejecuta la sentencia
                        conexion.Open();
                        // Retorna true si se modificó al menos una fila en la base de datos
                        return comando.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                // Captura y encapsula errores en la actualización del talle
                throw new Exception("Error al actualizar el talle: " + ex.Message);
            }
        }

        // Método para realizar la baja lógica de un talle (o eliminarlo si no tiene ventas asociadas)
        public bool EliminarTalleStock(int idTalle)
        {
            try
            {
                // Inicia la conexión utilizando el bloque using
                using (var conexion = ObtenerConexion())
                {
                    // Intentamos hacer un DELETE por si nunca se vendió. 
                    // Si tiene ventas asociadas, SQL Server lanzará la excepción de FK que capturaremos abajo.
                    string query = "DELETE FROM Producto_Talles WHERE ID = @ID";

                    // Prepara el comando SQL de borrado
                    using (var comando = new SqlCommand(query, conexion))
                    {
                        // Asigna el ID del talle por parámetro
                        comando.Parameters.AddWithValue("@ID", idTalle);
                        // Abre la conexión y ejecuta el comando
                        conexion.Open();
                        return comando.ExecuteNonQuery() > 0;
                    }
                }
            }
            // Captura violaciones de claves foráneas (registros históricos vinculados)
            catch (SqlException sqlEx)
            {
                // Si hay conflicto de clave foránea (número 547), significa que el talle tiene ventas y debemos aplicar baja lógica o avisar
                if (sqlEx.Number == 547)
                {
                    throw new Exception("No se puede eliminar el talle porque posee registros históricos de ventas asociados. Puede dejar su stock en 0.");
                }
                throw new Exception("Error al eliminar el talle: " + sqlEx.Message);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al eliminar el talle: " + ex.Message);
            }
        }

        // Método para obtener TODAS las marcas (activas e inactivas) para la validación y reactivación
        public List<Marca> ObtenerMarcas()
        {
            // Crea una lista genérica para almacenar las marcas encontradas
            var lista = new List<Marca>();
            // Consulta SQL para seleccionar todos los campos de la tabla Marca
            string query = "SELECT ID, Nombre, Activo FROM Marca";

            // Abre la conexión con la base de datos
            using (var conexion = ObtenerConexion())
            {
                using (var comando = new SqlCommand(query, conexion))
                {
                   
                    conexion.Open();
                    // Ejecuta el lector de datos para recorrer cada marca registrada
                    using (var lector = comando.ExecuteReader())
                    {
                        while (lector.Read())
                        {
                            // Mapea los valores de la fila actual a un nuevo objeto Marca y lo añade a la lista
                            lista.Add(new Marca
                            {
                                ID = lector.GetInt32(0),
                                Nombre = lector.IsDBNull(1) ? string.Empty : lector.GetString(1),
                                Activo = lector.GetBoolean(2)
                            });
                        }
                    }
                }
            }
            // Retorna la colección completa de marcas
            return lista;
        }

        // Método para registrar una nueva marca en la base de datos de forma segura.
        public bool InsertarMarca(Marca marca)
        {
            // Sentencia SQL de inserción con captura inmediata del ID autoincremental generado
            string query = "INSERT INTO Marca (Nombre, Activo) OUTPUT INSERTED.ID VALUES (@Nombre, @Activo)";

            // Abre conexión y prepara el comando SQL en simultáneo
            using (var conexion = ObtenerConexion())
            using (var comando = new SqlCommand(query, conexion))
            {
                // Asigna los parámetros de nombre y estado de manera segura
                comando.Parameters.AddWithValue("@Nombre", marca.Nombre);
                comando.Parameters.AddWithValue("@Activo", marca.Activo);

                conexion.Open();
                // Ejecuta ExecuteScalar para obtener el ID recién creado
                object resultado = comando.ExecuteScalar();

                if (resultado != null && resultado != DBNull.Value)
                {
                    // Asigna el ID obtenido a la marca en memoria y retorna true
                    marca.ID = Convert.ToInt32(resultado);
                    return true;
                }
                return false;
            }
        }

        // Método para actualizar una marca existente.
        public bool ActualizarMarca(Marca marca)
        {
            // Consulta SQL de actualización (UPDATE) para modificar el nombre y estado de la marca según su ID
            string query = "UPDATE Marca SET Nombre = @Nombre, Activo = @Activo WHERE ID = @ID";
            using (var conexion = ObtenerConexion())
            {
                using (var comando = new SqlCommand(query, conexion))
                {
                    // Asigna los parámetros protegidos
                    comando.Parameters.AddWithValue("@ID", marca.ID);
                    comando.Parameters.AddWithValue("@Nombre", marca.Nombre);
                    comando.Parameters.AddWithValue("@Activo", marca.Activo);
                    conexion.Open();
                    // Retorna true si la actualización afectó registros
                    return comando.ExecuteNonQuery() > 0;
                }
            }
        }

        // Método para realizar la baja lógica de una marca.
        public bool EliminarMarca(int id)
        {
            // Consulta SQL para cambiar el booleano 'Activo' a falso (0) mediante el ID
            string query = "UPDATE Marca SET Activo = 0 WHERE ID = @ID";
            using (var conexion = ObtenerConexion())
            {
                using (var comando = new SqlCommand(query, conexion))
                {
                    comando.Parameters.AddWithValue("@ID", id);
                    conexion.Open();
                    return comando.ExecuteNonQuery() > 0;
                }
            }
        }

        // Método para obtener TODAS las categorías (activas e inactivas) para la validación y reactivación
        public List<Categoria> ObtenerCategorias()
        {
            // Crea una lista genérica para almacenar las categorías
            var lista = new List<Categoria>();

            // Sentencia SQL para seleccionar todas las columnas de la tabla Categoria
            string query = "SELECT ID, Nombre, Activo FROM Categoria";

            // Abre la conexión utilizando el bloque using
            using (var conexion = ObtenerConexion())
            {
                using (var comando = new SqlCommand(query, conexion))
                {
                    
                    conexion.Open();
                    // Recorre los registros devueltos mediante el lector de datos
                    using (var lector = comando.ExecuteReader())
                    {
                        while (lector.Read())
                        {
                            // Mapea cada fila a un objeto Categoria y lo añade a la lista
                            lista.Add(new Categoria
                            {
                                ID = lector.GetInt32(0),
                                Nombre = lector.IsDBNull(1) ? string.Empty : lector.GetString(1),
                                Activo = lector.GetBoolean(2)
                            });
                        }
                    }
                }
            }
            return lista;
        }

        // Método para registrar una nueva categoría en la base de datos de forma segura.
        public bool InsertarCategoria(Categoria cat)
        {
            // Consulta SQL de inserción con retorno de ID autoincremental
            string query = "INSERT INTO Categoria (Nombre, Activo) OUTPUT INSERTED.ID VALUES (@Nombre, @Activo)";

            using (var conexion = ObtenerConexion())
            using (var comando = new SqlCommand(query, conexion))
            {
                // Asigna los parámetros de manera segura para evitar inyecciones SQL
                comando.Parameters.AddWithValue("@Nombre", cat.Nombre);
                comando.Parameters.AddWithValue("@Activo", cat.Activo);

                conexion.Open();
                // Ejecuta y captura el ID generado por la base de datos
                object resultado = comando.ExecuteScalar();

                if (resultado != null && resultado != DBNull.Value)
                {
                    cat.ID = Convert.ToInt32(resultado);
                    return true;
                }
                return false;
            }
        }

        // Método para modificar los datos de una categoría existente.
        public bool ActualizarCategoria(Categoria cat)
        {
            // Sentencia SQL para actualizar el nombre y estado de la categoría filtrando por ID
            string query = "UPDATE Categoria SET Nombre = @Nombre, Activo = @Activo WHERE ID = @ID";
            using (var conexion = ObtenerConexion())
            {
                using (var comando = new SqlCommand(query, conexion))
                {
                    comando.Parameters.AddWithValue("@ID", cat.ID);
                    comando.Parameters.AddWithValue("@Nombre", cat.Nombre);
                    comando.Parameters.AddWithValue("@Activo", cat.Activo);
                    conexion.Open();
                    return comando.ExecuteNonQuery() > 0;
                }
            }
        }

        // Método para dar de baja lógica una categoría cambiando su estado.
        public bool EliminarCategoria(int id)
        {
            // Consulta SQL para efectuar la baja lógica desactivando la categoría
            string query = "UPDATE Categoria SET Activo = 0 WHERE ID = @ID";
            using (var conexion = ObtenerConexion())
            {
                using (var comando = new SqlCommand(query, conexion))
                {
                    comando.Parameters.AddWithValue("@ID", id);
                    conexion.Open();
                    return comando.ExecuteNonQuery() > 0;
                }
            }
        }
    }
}