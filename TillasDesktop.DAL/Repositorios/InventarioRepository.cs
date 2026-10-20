using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using TillasDesktop.DAL.Base;
using TillasDesktop.Entities.Inventario;

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
                    comando.Parameters.AddWithValue("@Activo", activos);

                    // Abrimos físicamente el canal con el motor de base de datos.
                    conexion.Open();

                    // Ejecutamos la consulta a través de un lector de datos de flujo continuo.
                    using (var lector = comando.ExecuteReader())
                    {
                        // Iteramos renglón por renglón mientras existan registros devueltos por la base de datos.
                        while (lector.Read())
                        {
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

        // Método para buscar y recuperar un producto puntual de la base de datos haciendo uso de su identificador único.
        public Producto ObtenerPorId(int id)
        {
            string query = "SELECT ID, Codigo_Modelo, Nombre, Marca_ID, Categoria_ID, Precio_Venta, Activo FROM Productos WHERE ID = @ID";

            using (var conexion = ObtenerConexion())
            {
                using (var comando = new SqlCommand(query, conexion))
                {
                    // Añadimos el parámetro de forma segura para evitar inyecciones SQL.
                    comando.Parameters.AddWithValue("@ID", id);
                    conexion.Open();

                    using (SqlDataReader lector = comando.ExecuteReader())
                    {
                        if (lector.Read())
                        {
                            return new Producto
                            {
                                // Leemos la columna 0 (ID) asegurando que sea un entero.
                                ID = lector.GetInt32(0),

                                // Verificamos si la columna 1 (Codigo_Modelo) es nula en la BD; si lo es, asignamos un string vacío, de lo contrario leemos el texto.
                                Codigo_Modelo = lector.IsDBNull(1) ? string.Empty : lector.GetString(1),

                                // Verificamos si la columna 2 (Nombre) es nula; si es así, asignamos un string vacío.
                                Nombre = lector.IsDBNull(2) ? string.Empty : lector.GetString(2),

                                // Verificamos y leemos la columna 3 (Marca_ID).
                                Marca_ID = lector.GetInt32(3),

                                // Verificamos y leemos la columna 4 (Categoria_ID).
                                Categoria_ID = lector.GetInt32(4),

                                // Verificamos y leemos la columna 5 (Precio_Venta).
                                Precio_Venta = lector.GetDecimal(5),

                                // Verificamos y leemos la columna 6 (Activo).
                                Activo = lector.GetBoolean(6)
                            };
                        }
                    }
                }
            }
            return null;
        }

        // Método para dar de alta un producto nuevo en la base de datos devolviendo el ID generado automáticamente.
        public bool Insertar(Producto nuevoProducto)
        {
            try
            {
                using (var conexion = ObtenerConexion())
                {
                    // La instrucción OUTPUT INSERTED.ID nos permite capturar de inmediato el ID autoincremental que asigna SQL Server.
                    string query = @"INSERT INTO Productos (Codigo_Modelo, Nombre, Marca_ID, Categoria_ID, Precio_Venta, Activo) 
                                     OUTPUT INSERTED.ID
                                     VALUES (@Codigo_Modelo, @Nombre, @Marca_ID, @Categoria_ID, @Precio_Venta, @Activo)";

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

                        conexion.Open();
                        object resultado = comando.ExecuteScalar();

                        if (resultado != null && resultado != DBNull.Value)
                        {
                            // Inyectamos el ID real obtenido directamente en la entidad en memoria.
                            nuevoProducto.ID = Convert.ToInt32(resultado);
                            return true;
                        }
                        return false;
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                // Controlamos códigos de error específicos de SQL Server, como la violación de restricciones de índice único (duplicación de SKU).
                if (sqlEx.Number == 2627 || sqlEx.Number == 2601)
                {
                    throw new Exception("Ya existe un producto registrado con este Código de Modelo (SKU).");
                }
                throw new Exception("Error de base de datos al insertar el producto: " + sqlEx.Message);
            }
            catch (Exception ex)
            {
                throw new Exception("Error inesperado al insertar el producto: " + ex.Message);
            }
        }

        // Método para actualizar los datos generales de un producto preexistente.
        public bool Actualizar(Producto productoModificado)
        {
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
                using (var conexion = ObtenerConexion())
                {
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


                        conexion.Open();
                        // ExecuteNonQuery retorna la cantidad de filas afectadas; si es mayor a cero, la modificación fue exitosa.
                        return comando.ExecuteNonQuery() > 0;
                    }
                }
            }
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
                using (var conexion = ObtenerConexion())
                {
                    string query = "UPDATE Productos SET Activo = 0 WHERE ID = @ID";
                    using (var comando = new SqlCommand(query, conexion))
                    {
                        comando.Parameters.AddWithValue("@ID", idProducto);
                        conexion.Open();
                        return comando.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al eliminar el producto: " + ex.Message);
            }
        }

        // Método para consultar todos los talles y stock físico vinculados a un ID de producto específico.
        public List<ProductoTalle> ObtenerTallesPorProducto(int productoId)
        {
            List<ProductoTalle> listaTalles = new List<ProductoTalle>();
            string query = "SELECT ID, Producto_ID, Talle, Stock_Actual FROM Producto_Talles WHERE Producto_ID = @Producto_ID";

            using (var conexion = ObtenerConexion())
            {
                using (var comando = new SqlCommand(query, conexion))
                {
                    comando.Parameters.AddWithValue("@Producto_ID", productoId);
                    conexion.Open();

                    using (var lector = comando.ExecuteReader())
                    {
                        while (lector.Read())
                        {
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
            return listaTalles;
        }

        // Método para registrar el stock de un talle nuevo asociado a un producto.
        public bool InsertarTalleStock(ProductoTalle nuevoTalle)
        {
            try
            {
                using (var conexion = ObtenerConexion())
                {
                    string query = @"INSERT INTO Producto_Talles (Producto_ID, Talle, Stock_Actual) 
                                     OUTPUT INSERTED.ID
                                     VALUES (@Producto_ID, @Talle, @Stock_Actual)";

                    using (var comando = new SqlCommand(query, conexion))
                    {
                        comando.Parameters.AddWithValue("@Producto_ID", nuevoTalle.Producto_ID);
                        comando.Parameters.AddWithValue("@Talle", nuevoTalle.Talle);
                        comando.Parameters.AddWithValue("@Stock_Actual", nuevoTalle.Stock_Actual);

                        conexion.Open();
                        object resultado = comando.ExecuteScalar();

                        if (resultado != null && resultado != DBNull.Value)
                        {
                            nuevoTalle.ID = Convert.ToInt32(resultado);
                            return true;
                        }
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al registrar el stock del talle: " + ex.Message);
            }
        }

        // Método para modificar la cantidad o el número de un talle ya existente.
        public bool ActualizarTalleStock(ProductoTalle talleModificado)
        {
            try
            {
                using (var conexion = ObtenerConexion())
                {
                    string query = @"UPDATE Producto_Talles 
                                     SET Talle = @Talle, Stock_Actual = @Stock_Actual 
                                     WHERE ID = @ID";

                    using (var comando = new SqlCommand(query, conexion))
                    {
                        comando.Parameters.AddWithValue("@ID", talleModificado.ID);
                        comando.Parameters.AddWithValue("@Talle", talleModificado.Talle);
                        comando.Parameters.AddWithValue("@Stock_Actual", talleModificado.Stock_Actual);

                        conexion.Open();
                        return comando.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al actualizar el talle: " + ex.Message);
            }
        }

        // Método para realizar la baja lógica de un talle (o eliminarlo si no tiene ventas asociadas)
        public bool EliminarTalleStock(int idTalle)
        {
            try
            {
                using (var conexion = ObtenerConexion())
                {
                    // Intentamos hacer un DELETE por si nunca se vendió. 
                    // Si tiene ventas asociadas, SQL Server lanzará la excepción de FK que capturaremos abajo.
                    string query = "DELETE FROM Producto_Talles WHERE ID = @ID";
                    using (var comando = new SqlCommand(query, conexion))
                    {
                        comando.Parameters.AddWithValue("@ID", idTalle);
                        conexion.Open();
                        return comando.ExecuteNonQuery() > 0;
                    }
                }
            }
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
            var lista = new List<Marca>();
            string query = "SELECT ID, Nombre, Activo FROM Marca";

            using (var conexion = ObtenerConexion())
            {
                using (var comando = new SqlCommand(query, conexion))
                {
                   
                    conexion.Open();
                    using (var lector = comando.ExecuteReader())
                    {
                        while (lector.Read())
                        {
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
            return lista;
        }

        // Método para registrar una nueva marca en la base de datos de forma segura.
        public bool InsertarMarca(Marca marca)
        {
            string query = "INSERT INTO Marca (Nombre, Activo) OUTPUT INSERTED.ID VALUES (@Nombre, @Activo)";

            using (var conexion = ObtenerConexion())
            using (var comando = new SqlCommand(query, conexion))
            {
                comando.Parameters.AddWithValue("@Nombre", marca.Nombre);
                comando.Parameters.AddWithValue("@Activo", marca.Activo);

                conexion.Open();
                object resultado = comando.ExecuteScalar();

                if (resultado != null && resultado != DBNull.Value)
                {
                    marca.ID = Convert.ToInt32(resultado);
                    return true;
                }
                return false;
            }
        }

        // Método para actualizar una marca existente.
        public bool ActualizarMarca(Marca marca)
        {
            string query = "UPDATE Marca SET Nombre = @Nombre, Activo = @Activo WHERE ID = @ID";
            using (var conexion = ObtenerConexion())
            {
                using (var comando = new SqlCommand(query, conexion))
                {
                    comando.Parameters.AddWithValue("@ID", marca.ID);
                    comando.Parameters.AddWithValue("@Nombre", marca.Nombre);
                    comando.Parameters.AddWithValue("@Activo", marca.Activo);
                    conexion.Open();
                    return comando.ExecuteNonQuery() > 0;
                }
            }
        }

        // Método para realizar la baja lógica de una marca.
        public bool EliminarMarca(int id)
        {
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
            var lista = new List<Categoria>();
            string query = "SELECT ID, Nombre, Activo FROM Categoria";

            using (var conexion = ObtenerConexion())
            {
                using (var comando = new SqlCommand(query, conexion))
                {
                    
                    conexion.Open();
                    using (var lector = comando.ExecuteReader())
                    {
                        while (lector.Read())
                        {
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
            string query = "INSERT INTO Categoria (Nombre, Activo) OUTPUT INSERTED.ID VALUES (@Nombre, @Activo)";

            using (var conexion = ObtenerConexion())
            using (var comando = new SqlCommand(query, conexion))
            {
                comando.Parameters.AddWithValue("@Nombre", cat.Nombre);
                comando.Parameters.AddWithValue("@Activo", cat.Activo);

                conexion.Open();
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