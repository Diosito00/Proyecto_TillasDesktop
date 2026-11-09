
// Importa las clases y tipos de datos esenciales del entorno de .NET Framework/Core (Exceptions, Convert, DateTime, etc.)
using System;
// Importa las colecciones genéricas para manipular listas fuertemente tipadas como List<T>
using System.Collections.Generic;
// Importa la biblioteca de Microsoft Data SqlClient para interactuar directamente con la base de datos SQL Server
using Microsoft.Data.SqlClient;
// Importa la clase base de conexión dentro del proyecto (ConexionDb) que administra el String de Conexión
using TillasDesktop.DAL.Base;
// Importa las clases de dominio relativas a la facturación (Venta, DetalleVenta, Pago, TipoPago)
using TillasDesktop.Entities.Facturacion;
// Importa los Objetos de Transferencia de Datos (DTOs) utilizados para el reporte y la vista del carrito/catálogo
using TillasDesktop.Entities.Reportes;

// Define la estructura de espacio de nombres perteneciente a los repositorios de la capa de acceso a datos (DAL)
namespace TillasDesktop.DAL.Repositorios
{
    // Declara la clase pública VentaRepository que hereda de ConexionDb para acceder a la base de datos SQL Server
    public class VentaRepository : ConexionDb
    {
        // Declara el método público para registrar una venta completa procesando pago, cabecera de venta, ítems y descuento de stock
        public bool RegistrarVenta(Venta nuevaVenta, Pago nuevoPago, List<DetalleVenta> detalles, out string mensajeError)
        {
            // Inicializa la variable de salida de mensaje de error como una cadena vacía por defecto
            mensajeError = string.Empty;

            // Instancia la conexión a la base de datos garantizando la liberación automática de memoria al finalizar el bloque
            using (var conexion = ObtenerConexion())
            {
                // Abre físicamente el canal de comunicación y la sesión con el servidor SQL Server
                conexion.Open();

                // Inicia una transacción SQL explícita sobre la conexión abierta para asegurar la atomicidad del proceso
                using (SqlTransaction transaccion = conexion.BeginTransaction())
                {
                    // Inicia un bloque de control de excepciones para capturar cualquier fallo durante la transacción
                    try
                    {
                        // INSERTAR EL PAGO Y OBTENER SU ID
                        // Define la instrucción de inserción en la tabla Pago retornando inmediatamente el ID generado con OUTPUT INSERTED.ID
                        string queryPago = @"INSERT INTO Pago (Tipo_Pago_ID, Monto, Fecha_Pago, Activo) 
                                             OUTPUT INSERTED.ID 
                                             VALUES (@Tipo_Pago_ID, @Monto, @Fecha_Pago, @Activo);";

                        // Declara la variable numérica donde se almacenará el identificador único de pago generado por la BD
                        int idPagoGenerado;

                        // Instancia el comando SQL vinculando la consulta, la conexión abierta y la transacción en curso
                        using (var cmdPago = new SqlCommand(queryPago, conexion, transaccion))
                        {
                            // Asigna el valor del parámetro @Tipo_Pago_ID extraído del objeto recibido
                            cmdPago.Parameters.AddWithValue("@Tipo_Pago_ID", nuevoPago.Tipo_Pago_ID);
                            // Asigna el valor del parámetro @Monto correspondiente al pago efectuado
                            cmdPago.Parameters.AddWithValue("@Monto", nuevoPago.Monto);
                            // Asigna el valor del parámetro @Fecha_Pago indicando el momento en que se procesó el pago
                            cmdPago.Parameters.AddWithValue("@Fecha_Pago", nuevoPago.Fecha_Pago);
                            // Asigna el valor del parámetro booleano @Activo para establecer el estado del pago
                            cmdPago.Parameters.AddWithValue("@Activo", nuevoPago.Activo);

                            // Ejecuta la consulta escalar y convierte la primera columna/fila devuelta a un número entero
                            idPagoGenerado = Convert.ToInt32(cmdPago.ExecuteScalar());
                        }

                        // INSERTAR LA VENTA ASIGNANDO EL PAGO_ID
                        // Define la consulta SQL para guardar la cabecera de la venta en la tabla Ventas retornando el ID generado
                        string queryVenta = @"INSERT INTO Ventas (Fecha_Hora, Usuario_ID, Cliente_ID, Total, Pago_ID) 
                                              OUTPUT INSERTED.ID 
                                              VALUES (@Fecha_Hora, @Usuario_ID, @Cliente_ID, @Total, @Pago_ID);";

                        // Declara la variable para almacenar el ID resultante de la venta creada
                        int idVentaGenerada;

                        // Prepara el comando SqlCommand vinculando el texto SQL de la venta, la conexión y la transacción
                        using (var cmdVenta = new SqlCommand(queryVenta, conexion, transaccion))
                        {
                            // Asigna de forma protegida el parámetro de fecha y hora del sistema
                            cmdVenta.Parameters.AddWithValue("@Fecha_Hora", nuevaVenta.Fecha_Hora);
                            // Asigna el parámetro del ID del usuario/vendedor que realiza la operación
                            cmdVenta.Parameters.AddWithValue("@Usuario_ID", nuevaVenta.Usuario_ID);
                            // Asigna el parámetro del ID del cliente comprador (o nulo/consumidor final)
                            cmdVenta.Parameters.AddWithValue("@Cliente_ID", nuevaVenta.Cliente_ID);
                            // Asigna el parámetro del monto total final de la venta realizada
                            cmdVenta.Parameters.AddWithValue("@Total", nuevaVenta.Total);
                            // Asigna el ID del registro de pago previamente generado e insertado
                            cmdVenta.Parameters.AddWithValue("@Pago_ID", idPagoGenerado);

                            // Ejecuta la sentencia recuperando y convirtiendo el ID autoincremental de la venta en la base de datos
                            idVentaGenerada = Convert.ToInt32(cmdVenta.ExecuteScalar());
                        }

                        //  INSERTAR LOS DETALLES Y DESCONTAR STOCK FÍSICO
                        // Define la sentencia SQL para insertar los renglones individuales de productos comprados en Detalle_Ventas
                        string queryDetalle = @"INSERT INTO Detalle_Ventas (Venta_ID, Producto_Talle_ID, Cantidad, Precio_Unitario, Subtotal) 
                                                VALUES (@Venta_ID, @Producto_Talle_ID, @Cantidad, @Precio_Unitario, @Subtotal);";

                        // Al tener el ID directo del talle, la consulta de stock es mucho más rápida y segura.
                        // El cliente se lo llevó: Descontamos el físico y liberamos el bloqueo virtual
                        // Define la consulta doble SQL para actualizar los contadores de inventario y limpiar el bloqueo temporal del carrito
                        string queryDescontarStock = @"UPDATE Producto_Talles
                                                       SET Stock_Actual = Stock_Actual - @Cantidad, 
                                                           Stock_Reserva = Stock_Reserva - @Cantidad 
                                                       WHERE ID = @Producto_Talle_ID;
  
                                                       DELETE FROM Reservas_Temporales 
                                                       WHERE Usuario_ID = @Usuario_ID AND Producto_Talle_ID = @Producto_Talle_ID;";

                        // Itera a través de cada renglón de productos contenido en la lista recibida por parámetro
                        foreach (var item in detalles)
                        {
                            // Guardar Renglón
                            // Prepara el comando para la inserción del detalle individual en curso
                            using (var cmdDetalle = new SqlCommand(queryDetalle, conexion, transaccion))
                            {
                                // Inyecta el ID principal de la venta a la cual pertenece este detalle
                                cmdDetalle.Parameters.AddWithValue("@Venta_ID", idVentaGenerada);
                                // Inyecta el identificador único del talle específico del producto vendido
                                cmdDetalle.Parameters.AddWithValue("@Producto_Talle_ID", item.Producto_Talle_ID);
                                // Inyecta la cantidad de unidades vendidas del producto
                                cmdDetalle.Parameters.AddWithValue("@Cantidad", item.Cantidad);
                                // Inyecta el precio unitario del producto registrado en la transacción
                                cmdDetalle.Parameters.AddWithValue("@Precio_Unitario", item.Precio_Unitario);
                                // Inyecta el cálculo de subtotal correspondiente a esta línea de venta
                                cmdDetalle.Parameters.AddWithValue("@Subtotal", item.Subtotal);

                                // Ejecuta la inserción en la tabla de detalles sin retorno de valores
                                cmdDetalle.ExecuteNonQuery();
                            }

                            // Descontar Stock
                            // Prepara el comando SQL para actualizar el stock real y remover las reservas de la base de datos
                            using (var cmdStock = new SqlCommand(queryDescontarStock, conexion, transaccion))
                            {
                                // Asigna el valor de la cantidad de unidades vendidas a descontar
                                cmdStock.Parameters.AddWithValue("@Cantidad", item.Cantidad);
                                // Asigna el parámetro del ID de la relación Producto-Talle a actualizar
                                cmdStock.Parameters.AddWithValue("@Producto_Talle_ID", item.Producto_Talle_ID);
                                // Asigna el ID del usuario/cajero activo para borrar sus reservas temporales
                                cmdStock.Parameters.AddWithValue("@Usuario_ID", nuevaVenta.Usuario_ID);

                                // Ejecuta las instrucciones SQL devolviendo la cantidad total de filas afectadas
                                int filasAfectadas = cmdStock.ExecuteNonQuery();

                                // Valida si el proceso modificó registros en la tabla para garantizar consistencia
                                if (filasAfectadas == 0)
                                {
                                    // Dispara una excepción si no existía la fila del talle en la base de datos
                                    throw new Exception($"Error de consistencia: No se encontró el registro de stock para el ID {item.Producto_Talle_ID}.");
                                }
                            }
                        }

                        // CONFIRMAR TRANSACCIÓN
                        // Confirma y consolida permanentemente todos los cambios en la base de datos si no ocurrieron errores
                        transaccion.Commit();
                        // Devuelve true para notificar que la transacción finalizó correctamente
                        return true;
                    }
                    // Bloque de captura de errores imprevistos o violaciones de restricciones SQL durante la venta
                    catch (Exception ex)
                    {
                        // Ante cualquier mínimo error (como intentar cobrar algo sin stock), se anula toda la operación
                        // Anula y revierte absolutamente todos los INSERT/UPDATE ejecutados en el bloque try
                        transaccion.Rollback();
                        // Formatea la propiedad de salida de error agregando la causa descriptiva
                        mensajeError = "No se pudo completar la transacción. Detalle: " + ex.Message;
                        // Retorna false informando el fallo de la operación de registro
                        return false;
                    }
                }
            }
        }

        // Declara el método público para reservar temporalmente unidades de un talle evitando ventas simultáneas del mismo producto
        public bool ReservarProducto(int idUsuario, int idProductoTalle, int cantidad, int minutosValidez = 30)
        {
            // Obtiene la conexión a la base de datos resguardada con un bloque using
            using (var conexion = ObtenerConexion())
            {
                // Abre la conexión físicamente con el servidor SQL
                conexion.Open();
                // Inicia una transacción para proteger el chequeo y bloqueo del stock
                using (var transaccion = conexion.BeginTransaction())
                {
                    // Inicia el bloque try para controlar la ejecución segura
                    try
                    {
                        // Verificar que hay stock REAL disponible (Stock_Actual - Stock_Reserva >= Cantidad)
                        // Define la consulta de selección calculando el stock físico libre real de reservas previo al bloqueo
                        string queryCheck = @"SELECT (Stock_Actual - Stock_Reserva) 
                                              FROM Producto_Talles 
                                              WHERE ID = @IdTalle";

                        // Prepara el comando SqlCommand asociado a la transacción
                        using (var cmdCheck = new SqlCommand(queryCheck, conexion, transaccion))
                        {
                            // Inyecta el ID del talle que se desea consultar
                            cmdCheck.Parameters.AddWithValue("@IdTalle", idProductoTalle);
                            // Ejecuta y convierte el resultado a un entero para conocer la disponibilidad real
                            int disponible = Convert.ToInt32(cmdCheck.ExecuteScalar());

                            // Comprueba si el stock libre calculado es menor a la cantidad exigida por la reserva
                            if (disponible < cantidad)
                                // Lanza una excepción cancelando la reserva por falta de stock
                                throw new Exception("No hay stock suficiente disponible en este momento.");
                        }

                        // Bloquear el stock en la tabla principal
                        // Consulta SQL para incrementar la columna Stock_Reserva restándolo de la disponibilidad para otros usuarios
                        string queryBloqueo = @"UPDATE Producto_Talles 
                                                SET Stock_Reserva = Stock_Reserva + @Cantidad 
                                                WHERE ID = @IdTalle";
                        // Instancia el comando SQL para ejecutar el incremento de reservas
                        using (var cmdBloqueo = new SqlCommand(queryBloqueo, conexion, transaccion))
                        {
                            // Parámetro de la cantidad a retener temporalmente
                            cmdBloqueo.Parameters.AddWithValue("@Cantidad", cantidad);
                            // Parámetro de identificación del talle afectado
                            cmdBloqueo.Parameters.AddWithValue("@IdTalle", idProductoTalle);
                            // Ejecuta el UPDATE en la tabla Producto_Talles
                            cmdBloqueo.ExecuteNonQuery();
                        }

                        // Registrar a quién le pertenece la reserva y cuándo vence
                        // Consulta SQL para crear una entrada en la tabla Reservas_Temporales asignando expiración
                        string queryRegistro = @"INSERT INTO Reservas_Temporales (Usuario_ID, Producto_Talle_ID, Cantidad, Fecha_Expiracion) 
                                                 VALUES (@Usuario_ID, @IdTalle, @Cantidad, @Expiracion)";
                        // Prepara el comando para la inserción de la reserva temporal
                        using (var cmdRegistro = new SqlCommand(queryRegistro, conexion, transaccion))
                        {
                            // Asigna el usuario al que se le reserva el carrito
                            cmdRegistro.Parameters.AddWithValue("@Usuario_ID", idUsuario);
                            // Asigna el ID del talle involucrado
                            cmdRegistro.Parameters.AddWithValue("@IdTalle", idProductoTalle);
                            // Asigna la cantidad reservada
                            cmdRegistro.Parameters.AddWithValue("@Cantidad", cantidad);
                            // Asigna la fecha y hora de expiración sumando los minutos pasados por parámetro a la fecha actual
                            cmdRegistro.Parameters.AddWithValue("@Expiracion", DateTime.Now.AddMinutes(minutosValidez));
                            // Ejecuta la inserción del registro en la BD
                            cmdRegistro.ExecuteNonQuery();
                        }

                        // Consolida y aplica definitivamente los cambios en la BD
                        transaccion.Commit();
                        // Retorna true notificando la reserva exitosa
                        return true;
                    }
                    // Captura errores en caso de fallo de stock o bloqueos concurrentes
                    catch (Exception)
                    {
                        // Revierte y cancela cualquier modificación efectuada
                        transaccion.Rollback();
                        // Retorna false indicando que la reserva fue rechazada
                        return false;
                    }
                }
            }
        }

        // Se ejecuta si el cajero quita el producto del carrito o cancela la venta
        // Declara el método público para des-bloquear o liberar las reservas devolviendo disponibilidad al sistema
        public bool LiberarReserva(int idUsuario, int idProductoTalle, int cantidad)
        {
            // Instancia la conexión a la base de datos dentro de un bloque using
            using (var conexion = ObtenerConexion())
            {
                // Abre la conexión con el servidor
                conexion.Open();
                // Inicia la transacción para asegurar que la liberación y limpieza sean atómicas
                using (var transaccion = conexion.BeginTransaction())
                {
                    // Bloque try para control de excepciones
                    try
                    {
                        // Restar la cantidad retenida de la columna Stock_Reserva
                        // Consulta SQL para descontar las unidades de la columna de reservas de la tabla Producto_Talles
                        string queryRestar = @"UPDATE Producto_Talles 
                                               SET Stock_Reserva = Stock_Reserva - @Cantidad 
                                               WHERE ID = @IdTalle";

                        // Prepara el comando SQL correspondiente
                        using (var cmdRestar = new SqlCommand(queryRestar, conexion, transaccion))
                        {
                            // Inyecta la cantidad a descontar
                            cmdRestar.Parameters.AddWithValue("@Cantidad", cantidad);
                            // Inyecta el ID del talle a liberar
                            cmdRestar.Parameters.AddWithValue("@IdTalle", idProductoTalle);
                            // Ejecuta la actualización de restitución de stock
                            cmdRestar.ExecuteNonQuery();
                        }

                        // Eliminar el registro temporal del usuario 
                        // Consulta SQL para purgar la reserva correspondiente en la tabla Reservas_Temporales
                        string queryEliminar = @"DELETE FROM Reservas_Temporales 
                                                 WHERE Usuario_ID = @Usuario_ID 
                                                 AND Producto_Talle_ID = @IdTalle";

                        // Prepara el comando SQL de eliminación
                        using (var cmdEliminar = new SqlCommand(queryEliminar, conexion, transaccion))
                        {
                            // Asigna el usuario dueño del carrito cancelado
                            cmdEliminar.Parameters.AddWithValue("@Usuario_ID", idUsuario);
                            // Asigna el ID del talle que se remueve del carrito
                            cmdEliminar.Parameters.AddWithValue("@IdTalle", idProductoTalle);
                            // Ejecuta la eliminación del registro temporal
                            cmdEliminar.ExecuteNonQuery();
                        }

                        // Confirma la transacción en la base de datos
                        transaccion.Commit();
                        // Retorna true indicando liberación exitosa
                        return true;
                    }
                    // Manejo de errores
                    catch (Exception)
                    {
                        // Cancela los cambios hechos si ocurrió un error
                        transaccion.Rollback();
                        // Retorna false indicando fallo al liberar la reserva
                        return false;
                    }
                }
            }
        }

        // Llama al SP (procedimiento almacenado)
        // Método público para invocar el procedimiento almacenado que purga reservas temporales caducadas
        public void EjecutarLimpieza()
        {
            // Abre y cierra de forma segura la conexión a la base de datos
            using (var conexion = ObtenerConexion())
            {
                // Nombre del Stored Procedure almacenado en el motor SQL Server
                string query = "sp_LimpiarReservasVencidas";
                // Prepara el comando SqlCommand vinculando el procedimiento y la conexión
                using (var cmd = new SqlCommand(query, conexion))
                {
                    // Especifica explícitamente que el comando a ejecutar es un Procedimiento Almacenado
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    // Abre físicamente el canal de comunicación
                    conexion.Open();
                    // Ejecuta el procedimiento almacenado sin retornar conjunto de datos
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // OBTENER CATÁLOGO EN TIEMPO REAL PARA EL PUNTO DE VENTA
        // Método público que retorna la lista completa de artículos activos con stock real disponible para comercialización
        public List<ProductoCatalogoDTO> ObtenerCatalogoCaja()
        {
            // Instancia la lista genérica vacía para devolver el catálogo de caja
            var catalogo = new List<ProductoCatalogoDTO>();

            // Instancia la conexión con la base de datos
            using (var conexion = ObtenerConexion())
            {
                // Cruzamos las 4 tablas y calculamos el stock disponible al vuelo
                // Consulta SQL compleja relacionando Producto_Talles, Productos, Marca y Categoria restando reservas al stock actual
                string query = @"SELECT 
                                    PT.ID AS ProductoTalle_ID,
                                    P.Codigo_Modelo,
                                    P.Nombre,
                                    M.Nombre AS NombreMarca,
                                    C.Nombre AS NombreCategoria,
                                    PT.Talle,
                                    (PT.Stock_Actual - PT.Stock_Reserva) AS Stock_Disponible,
                                    P.Precio_Venta
                                 FROM Producto_Talles PT
                                 INNER JOIN Productos P ON PT.Producto_ID = P.ID
                                 INNER JOIN Marca M ON P.Marca_ID = M.ID
                                 INNER JOIN Categoria C ON P.Categoria_ID = C.ID
                                 WHERE P.Activo = 1 AND (PT.Stock_Actual - PT.Stock_Reserva) > 0";

                // Instancia el comando SQL vinculando la consulta y la conexión
                using (var comando = new SqlCommand(query, conexion))
                {
                    // Abre el canal de conexión con la base de datos
                    conexion.Open();
                    // Ejecuta el comando obteniendo un SqlDataReader de flujo directo
                    using (var lector = comando.ExecuteReader())
                    {
                        // Itera a través de todos los registros del catálogo obtenidos
                        while (lector.Read())
                        {
                            // Instancia un nuevo DTO del catálogo asignando cada campo según el índice de la columna en el SELECT y lo añade a la lista
                            catalogo.Add(new ProductoCatalogoDTO
                            {
                                ProductoTalle_ID = lector.GetInt32(0),
                                Codigo_Modelo = lector.GetString(1),
                                Nombre = lector.GetString(2),
                                NombreMarca = lector.GetString(3),
                                NombreCategoria = lector.GetString(4),
                                Talle = lector.GetInt32(5),
                                Stock_Disponible = lector.GetInt32(6),
                                Precio_Venta = lector.GetDecimal(7)
                            });
                        }
                    }
                }
            }
            // Devuelve la lista poblada de productos disponibles para la caja
            return catalogo;
        }

        // Método público para obtener el carrito actual con reservas activas y no expiradas de un usuario en particular
        public List<ItemCarritoDTO> ObtenerReservasUsuario(int idUsuario)
        {
            // Instancia la lista para devolver los ítems retenidos por el usuario
            var carritoRecuperado = new List<ItemCarritoDTO>();
            // Prepara el bloque de conexión con la base de datos
            using (var conexion = ObtenerConexion())
            {
                // Consulta SQL uniendo las reservas temporales no caducadas con la información de productos y talles
                string query = @"SELECT 
                                    PT.ID, 
                                    P.Nombre, 
                                    PT.Talle, 
                                    P.Precio_Venta, 
                                    RT.Cantidad
                                FROM Reservas_Temporales RT
                                INNER JOIN Producto_Talles PT ON RT.Producto_Talle_ID = PT.ID
                                INNER JOIN Productos P ON PT.Producto_ID = P.ID
                                WHERE RT.Usuario_ID = @Usuario_ID AND RT.Fecha_Expiracion >= GETDATE()";

                // Prepara el comando SQL
                using (var cmd = new SqlCommand(query, conexion))
                {
                    // Asigna el parámetro del ID de usuario a consultar
                    cmd.Parameters.AddWithValue("@Usuario_ID", idUsuario);
                    // Abre el canal de conexión
                    conexion.Open();
                    // Ejecuta el lector de datos
                    using (var lector = cmd.ExecuteReader())
                    {
                        // Lee fila a fila las reservas encontradas
                        while (lector.Read())
                        {
                            // Mapea la fila al objeto ItemCarritoDTO y lo acumula en la lista
                            carritoRecuperado.Add(new ItemCarritoDTO
                            {
                                Producto_Talle_ID = lector.GetInt32(0),
                                Nombre = lector.GetString(1),
                                Talle = lector.GetInt32(2),
                                Precio_Unitario = lector.GetDecimal(3),
                                Cantidad = lector.GetInt32(4)
                            });
                        }
                    }
                }
            }
            // Retorna la colección de ítems activos dentro del carrito recuperado
            return carritoRecuperado;
        }

        // Método público para listar todos los métodos/tipos de pago habilitados en la base de datos
        public List<TipoPago> ObtenerTiposPago()
        {
            // Instancia la lista para guardar las entidades TipoPago
            var lista = new List<TipoPago>();
            // Abre la conexión resguardada con using
            using (var conexion = ObtenerConexion())
            {
                // Sentencia SQL de selección filtrando aquellos registros marcados como Activos
                string query = "SELECT ID, Nombre, Descripcion, Activo FROM Tipo_Pago WHERE Activo = 1";
                // Prepara el comando SQL con la sentencia y la conexión
                using (var cmd = new SqlCommand(query, conexion))
                {
                    // Abre la conexión con el servidor SQL
                    conexion.Open();
                    // Ejecuta la lectura de datos
                    using (var lector = cmd.ExecuteReader())
                    {
                        // Itera mientras existan tipos de pago en el resultado
                        while (lector.Read())
                        {
                            // Construye el objeto TipoPago validando si la descripción es nula antes de convertirla
                            lista.Add(new TipoPago
                            {
                                ID = lector.GetInt32(0),
                                Nombre = lector.GetString(1),
                                Descripcion = lector.IsDBNull(2) ? "" : lector.GetString(2),
                                Activo = lector.GetBoolean(3)
                            });
                        }
                    }
                }
            }
            // Retorna la lista con los medios de pago disponibles
            return lista;
        }

        // Método público para recuperar las líneas de detalle asociadas a un ID de venta específico previamente cerrada
        public List<LineaDetalleVentaDTO> ObtenerDetallesPorVenta(int ventaId)
        {
            // Instancia la lista contenedora de las líneas del comprobante de venta
            var listaDetalles = new List<LineaDetalleVentaDTO>();

            // Abre el canal de conexión con la base de datos
            using (var conexion = ObtenerConexion())
            {
                // Cruzamos Detalle_Ventas con Producto_Talles, Productos y Marca
                // Consulta SQL de cruce de tablas para obtener la información de comprobante legible por el usuario
                string query = @"SELECT 
                            P.Codigo_Modelo,
                            P.Nombre AS NombreProducto,
                            M.Nombre AS Marca,
                            PT.Talle,
                            DV.Cantidad,
                            DV.Precio_Unitario,
                            DV.Subtotal
                         FROM Detalle_Ventas DV
                         INNER JOIN Producto_Talles PT ON DV.Producto_Talle_ID = PT.ID
                         INNER JOIN Productos P ON PT.Producto_ID = P.ID
                         INNER JOIN Marca M ON P.Marca_ID = M.ID
                         WHERE DV.Venta_ID = @Venta_ID";

                // Instancia el comando SQL de la consulta de comprobante
                using (var comando = new SqlCommand(query, conexion))
                {
                    // Asigna el ID de la venta recibida por parámetro
                    comando.Parameters.AddWithValue("@Venta_ID", ventaId);
                    // Abre la conexión físicamente
                    conexion.Open();

                    // Ejecuta el lector de registros
                    using (var lector = comando.ExecuteReader())
                    {
                        // Itera por los renglones del comprobante de la venta
                        while (lector.Read())
                        {
                            // Formatea y mapea cada renglón devuelto a un LineaDetalleVentaDTO
                            listaDetalles.Add(new LineaDetalleVentaDTO
                            {
                                CodigoModelo = lector.GetString(0),
                                NombreProducto = lector.GetString(1),
                                Marca = lector.GetString(2),
                                Talle = Convert.ToDecimal(lector.GetValue(3)),
                                Cantidad = lector.GetInt32(4),
                                PrecioUnitario = lector.GetDecimal(5),
                                Subtotal = lector.GetDecimal(6)
                            });
                        }
                    }
                }
            }

            // Retorna la lista de líneas de detalle mapeadas
            return listaDetalles;
        }
    }
}