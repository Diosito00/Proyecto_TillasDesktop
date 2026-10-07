using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using TillasDesktop.DAL.Base;
using TillasDesktop.Entities.Facturacion;

namespace TillasDesktop.DAL.Repositorios
{
    public class VentaRepository : ConexionDb
    {
        // Recibe las tres partes involucradas desde la interfaz gráfica
        public bool RegistrarVenta(Venta nuevaVenta, Pago nuevoPago, List<DetalleVenta> detalles, out string mensajeError)
        {
            mensajeError = string.Empty;

            using (var conexion = ObtenerConexion())
            {
                conexion.Open();

                using (SqlTransaction transaccion = conexion.BeginTransaction())
                {
                    try
                    {
                        // INSERTAR EL PAGO Y OBTENER SU ID
                        string queryPago = @"INSERT INTO Pago (Tipo_Pago_ID, Monto, Fecha_Pago, Activo) 
                                             OUTPUT INSERTED.ID 
                                             VALUES (@Tipo_Pago_ID, @Monto, @Fecha_Pago, @Activo);";

                        int idPagoGenerado;

                        using (var cmdPago = new SqlCommand(queryPago, conexion, transaccion))
                        {
                            cmdPago.Parameters.AddWithValue("@Tipo_Pago_ID", nuevoPago.Tipo_Pago_ID);
                            cmdPago.Parameters.AddWithValue("@Monto", nuevoPago.Monto);
                            cmdPago.Parameters.AddWithValue("@Fecha_Pago", nuevoPago.Fecha_Pago);
                            cmdPago.Parameters.AddWithValue("@Activo", nuevoPago.Activo);

                            idPagoGenerado = Convert.ToInt32(cmdPago.ExecuteScalar());
                        }

                        // INSERTAR LA VENTA ASIGNANDO EL PAGO_ID
                        string queryVenta = @"INSERT INTO Ventas (Fecha_Hora, Usuario_ID, Cliente_ID, Total, Pago_ID) 
                                              OUTPUT INSERTED.ID 
                                              VALUES (@Fecha_Hora, @Usuario_ID, @Cliente_ID, @Total, @Pago_ID);";

                        int idVentaGenerada;

                        using (var cmdVenta = new SqlCommand(queryVenta, conexion, transaccion))
                        {
                            cmdVenta.Parameters.AddWithValue("@Fecha_Hora", nuevaVenta.Fecha_Hora);
                            cmdVenta.Parameters.AddWithValue("@Usuario_ID", nuevaVenta.Usuario_ID);
                            cmdVenta.Parameters.AddWithValue("@Cliente_ID", nuevaVenta.Cliente_ID);
                            cmdVenta.Parameters.AddWithValue("@Total", nuevaVenta.Total);
                            cmdVenta.Parameters.AddWithValue("@Pago_ID", idPagoGenerado);

                            idVentaGenerada = Convert.ToInt32(cmdVenta.ExecuteScalar());
                        }

                        //  INSERTAR LOS DETALLES Y DESCONTAR STOCK FÍSICO
                        string queryDetalle = @"INSERT INTO Detalle_Ventas (Venta_ID, Producto_Talle_ID, Cantidad, Precio_Unitario, Subtotal) 
                                                VALUES (@Venta_ID, @Producto_Talle_ID, @Cantidad, @Precio_Unitario, @Subtotal);";

                        // Al tener el ID directo del talle, la consulta de stock es mucho más rápida y segura.
                        // El cliente se lo llevó: Descontamos el físico y liberamos el bloqueo virtual
                        string queryDescontarStock = @"UPDATE Producto_Talles
                                                       SET Stock_Actual = Stock_Actual - @Cantidad, 
                                                           Stock_Reserva = Stock_Reserva - @Cantidad 
                                                       WHERE ID = @Producto_Talle_ID;
  
                                                       DELETE FROM Reservas_Temporales 
                                                       WHERE Usuario_ID = @Usuario_ID AND Producto_Talle_ID = @Producto_Talle_ID;"; //El cliente se lo llevó: Descontamos el físico y liberamos el bloqueo virtual
                        foreach (var item in detalles)
                        {
                            // Guardar Renglón
                            using (var cmdDetalle = new SqlCommand(queryDetalle, conexion, transaccion))
                            {
                                cmdDetalle.Parameters.AddWithValue("@Venta_ID", idVentaGenerada);
                                cmdDetalle.Parameters.AddWithValue("@Producto_Talle_ID", item.Producto_Talle_ID);
                                cmdDetalle.Parameters.AddWithValue("@Cantidad", item.Cantidad);
                                cmdDetalle.Parameters.AddWithValue("@Precio_Unitario", item.Precio_Unitario);
                                cmdDetalle.Parameters.AddWithValue("@Subtotal", item.Subtotal);

                                cmdDetalle.ExecuteNonQuery();
                            }

                            // Descontar Stock
                            using (var cmdStock = new SqlCommand(queryDescontarStock, conexion, transaccion))
                            {
                                cmdStock.Parameters.AddWithValue("@Cantidad", item.Cantidad);
                                cmdStock.Parameters.AddWithValue("@Producto_Talle_ID", item.Producto_Talle_ID);
                                cmdStock.Parameters.AddWithValue("@Usuario_ID", nuevaVenta.Usuario_ID);

                                int filasAfectadas = cmdStock.ExecuteNonQuery();

                                if (filasAfectadas == 0)
                                {
                                    throw new Exception($"Error de consistencia: No se encontró el registro de stock para el ID {item.Producto_Talle_ID}.");
                                }
                            }
                        }

                        // CONFIRMAR TRANSACCIÓN
                        transaccion.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        // Ante cualquier mínimo error (como intentar cobrar algo sin stock), se anula toda la operación
                        transaccion.Rollback();
                        mensajeError = "No se pudo completar la transacción. Detalle: " + ex.Message;
                        return false;
                    }
                }
            }
        }

        public bool ReservarProducto(int idUsuario, int idProductoTalle, int cantidad, int minutosValidez = 30)
        {
            using (var conexion = ObtenerConexion())
            {
                conexion.Open();
                using (var transaccion = conexion.BeginTransaction())
                {
                    try
                    {
                        // Verificar que hay stock REAL disponible (Stock_Actual - Stock_Reserva >= Cantidad)
                        string queryCheck = @"SELECT (Stock_Actual - Stock_Reserva) 
                                              FROM Producto_Talles 
                                              WHERE ID = @IdTalle";

                        using (var cmdCheck = new SqlCommand(queryCheck, conexion, transaccion))
                        {
                            cmdCheck.Parameters.AddWithValue("@IdTalle", idProductoTalle);
                            int disponible = Convert.ToInt32(cmdCheck.ExecuteScalar());

                            if (disponible < cantidad)
                                throw new Exception("No hay stock suficiente disponible en este momento.");
                        }

                        // Bloquear el stock en la tabla principal
                        string queryBloqueo = @"UPDATE Producto_Talles 
                                                SET Stock_Reserva = Stock_Reserva + @Cantidad 
                                                WHERE ID = @IdTalle";
                        using (var cmdBloqueo = new SqlCommand(queryBloqueo, conexion, transaccion))
                        {
                            cmdBloqueo.Parameters.AddWithValue("@Cantidad", cantidad);
                            cmdBloqueo.Parameters.AddWithValue("@IdTalle", idProductoTalle);
                            cmdBloqueo.ExecuteNonQuery();
                        }

                        // Registrar a quién le pertenece la reserva y cuándo vence
                        string queryRegistro = @"INSERT INTO Reservas_Temporales (Usuario_ID, Producto_Talle_ID, Cantidad, Fecha_Expiracion) 
                                                 VALUES (@Usuario_ID, @IdTalle, @Cantidad, @Expiracion)";
                        using (var cmdRegistro = new SqlCommand(queryRegistro, conexion, transaccion))
                        {
                            cmdRegistro.Parameters.AddWithValue("@Usuario_ID", idUsuario);
                            cmdRegistro.Parameters.AddWithValue("@IdTalle", idProductoTalle);
                            cmdRegistro.Parameters.AddWithValue("@Cantidad", cantidad);
                            cmdRegistro.Parameters.AddWithValue("@Expiracion", DateTime.Now.AddMinutes(minutosValidez));
                            cmdRegistro.ExecuteNonQuery();
                        }

                        transaccion.Commit();
                        return true;
                    }
                    catch (Exception)
                    {
                        transaccion.Rollback();
                        return false;
                    }
                }
            }
        }

        // Se ejecuta si el cajero quita el producto del carrito o cancela la venta
        public bool LiberarReserva(int idUsuario, int idProductoTalle, int cantidad)
        {
            using (var conexion = ObtenerConexion())
            {
                conexion.Open();
                using (var transaccion = conexion.BeginTransaction())
                {
                    try
                    {
                        // Restar la cantidad retenida de la columna Stock_Reserva
                        string queryRestar = @"UPDATE Producto_Talles 
                                               SET Stock_Reserva = Stock_Reserva - @Cantidad 
                                               WHERE ID = @IdTalle";

                        using (var cmdRestar = new SqlCommand(queryRestar, conexion, transaccion))
                        {
                            cmdRestar.Parameters.AddWithValue("@Cantidad", cantidad);
                            cmdRestar.Parameters.AddWithValue("@IdTalle", idProductoTalle);
                            cmdRestar.ExecuteNonQuery();
                        }

                        // Eliminar el registro temporal del usuario 
                        string queryEliminar = @"DELETE FROM Reservas_Temporales 
                                                 WHERE Usuario_ID = @Usuario_ID 
                                                 AND Producto_Talle_ID = @IdTalle";

                        using (var cmdEliminar = new SqlCommand(queryEliminar, conexion, transaccion))
                        {
                            cmdEliminar.Parameters.AddWithValue("@Usuario_ID", idUsuario);
                            cmdEliminar.Parameters.AddWithValue("@IdTalle", idProductoTalle);
                            cmdEliminar.ExecuteNonQuery();
                        }

                        transaccion.Commit();
                        return true;
                    }
                    catch (Exception)
                    {
                        transaccion.Rollback();
                        return false;
                    }
                }
            }
        }   

        // Llama al SP (procedimiento almacenado)
        public void EjecutarLimpieza()
        {
            using (var conexion = ObtenerConexion())
            {
                string query = "sp_LimpiarReservasVencidas";
                using (var cmd = new SqlCommand(query, conexion))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    conexion.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // OBTENER CATÁLOGO EN TIEMPO REAL PARA EL PUNTO DE VENTA
        public List<ProductoCatalogoDTO> ObtenerCatalogoCaja()
        {
            var catalogo = new List<ProductoCatalogoDTO>();

            using (var conexion = ObtenerConexion())
            {
                // Cruzamos las 4 tablas y calculamos el stock disponible al vuelo
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

                using (var comando = new SqlCommand(query, conexion))
                {
                    conexion.Open();
                    using (var lector = comando.ExecuteReader())
                    {
                        while (lector.Read())
                        {
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
            return catalogo;
        }

        public List<ItemCarritoDTO> ObtenerReservasUsuario(int idUsuario)
        {
            var carritoRecuperado = new List<ItemCarritoDTO>();
            using (var conexion = ObtenerConexion())
            {
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

                using (var cmd = new SqlCommand(query, conexion))
                {
                    cmd.Parameters.AddWithValue("@Usuario_ID", idUsuario);
                    conexion.Open();
                    using (var lector = cmd.ExecuteReader())
                    {
                        while (lector.Read())
                        {
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
            return carritoRecuperado;
        }

        public List<TipoPago> ObtenerTiposPago()
        {
            var lista = new List<TipoPago>();
            using (var conexion = ObtenerConexion())
            {
                string query = "SELECT ID, Nombre, Descripcion, Activo FROM Tipo_Pago WHERE Activo = 1";
                using (var cmd = new SqlCommand(query, conexion))
                {
                    conexion.Open();
                    using (var lector = cmd.ExecuteReader())
                    {
                        while (lector.Read())
                        {
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
            return lista;
        }
    }
}