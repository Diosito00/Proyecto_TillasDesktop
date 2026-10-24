using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using TillasDesktop.DAL.Base;
using TillasDesktop.Entities.Reportes;

namespace TillasDesktop.DAL.Repositorios
{
    public class ReportesRepository : ConexionDb
    {
        public List<VentaResumenDTO> ObtenerReporteVentas(DateTime inicio, DateTime fin)
        {
            var lista = new List<VentaResumenDTO>();
            using (var conexion = ObtenerConexion())
            {
                string query = @"
                    SELECT 
                        v.ID, 
                        v.Fecha_Hora, 
                        u.nombre_usuario AS Vendedor, 
                        ISNULL(c.Nombre + ' ' + c.Apellido, 'Consumidor Final') AS Cliente, 
                        tp.Nombre AS MetodoPago, 
                        v.Total 
                    FROM Ventas v
                    INNER JOIN Usuarios u ON v.Usuario_ID = u.id_usuario
                    LEFT JOIN Clientes c ON v.Cliente_ID = c.ID
                    INNER JOIN Pago p ON v.Pago_ID = p.ID
                    INNER JOIN Tipo_Pago tp ON p.Tipo_Pago_ID = tp.ID
                    WHERE v.Fecha_Hora >= @Inicio AND v.Fecha_Hora <= @Fin
                    ORDER BY v.Fecha_Hora DESC";

                using (var cmd = new SqlCommand(query, conexion))
                {
                    cmd.Parameters.AddWithValue("@Inicio", inicio.Date);
                    cmd.Parameters.AddWithValue("@Fin", fin.Date.AddDays(1).AddSeconds(-1)); // Hasta las 23:59:59 del día final

                    conexion.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(new VentaResumenDTO
                            {
                                ID = Convert.ToInt32(reader["ID"]),
                                Fecha_Hora = Convert.ToDateTime(reader["Fecha_Hora"]),
                                NombreVendedor = reader["Vendedor"].ToString(),
                                NombreCliente = reader["Cliente"].ToString(),
                                MetodoPago = reader["MetodoPago"].ToString(),
                                Total = Convert.ToDecimal(reader["Total"])
                            });
                        }
                    }
                }
            }
            return lista;
        }

        public List<RendimientoVendedorDTO> ObtenerRendimientoVendedores(DateTime inicio, DateTime fin)
        {
            var lista = new List<RendimientoVendedorDTO>();
            using (var conexion = ObtenerConexion())
            {
                string query = @"
                    SELECT 
                        u.nombre_usuario AS Vendedor, 
                        COUNT(v.ID) AS CantidadVentas, 
                        SUM(v.Total) AS TotalRecaudado 
                    FROM Ventas v
                    INNER JOIN Usuarios u ON v.Usuario_ID = u.id_usuario
                    WHERE v.Fecha_Hora >= @Inicio AND v.Fecha_Hora <= @Fin
                    GROUP BY u.nombre_usuario
                    ORDER BY TotalRecaudado DESC";

                using (var cmd = new SqlCommand(query, conexion))
                {
                    cmd.Parameters.AddWithValue("@Inicio", inicio.Date);
                    cmd.Parameters.AddWithValue("@Fin", fin.Date.AddDays(1).AddSeconds(-1));

                    conexion.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(new RendimientoVendedorDTO
                            {
                                Vendedor = reader["Vendedor"].ToString(),
                                CantidadVentas = Convert.ToInt32(reader["CantidadVentas"]),
                                TotalRecaudado = Convert.ToDecimal(reader["TotalRecaudado"])
                            });
                        }
                    }
                }
            }
            return lista;
        }

        public List<ProductoVendidoDTO> ObtenerProductosMasVendidos(DateTime inicio, DateTime fin, string marca, string categoria)
        {
            var lista = new List<ProductoVendidoDTO>();
            using (var conexion = ObtenerConexion())
            {
                string query = @"
                    SELECT 
                        p.Codigo_Modelo, 
                        p.Nombre, 
                        m.Nombre AS Marca, 
                        c.Nombre AS Categoria, 
                        SUM(dv.Cantidad) AS CantidadVendida, 
                        SUM(dv.Subtotal) AS TotalGenerado
                    FROM Detalle_Ventas dv
                    INNER JOIN Ventas v ON dv.Venta_ID = v.ID
                    INNER JOIN Producto_Talles pt ON dv.Producto_Talle_ID = pt.ID
                    INNER JOIN Productos p ON pt.Producto_ID = p.ID
                    INNER JOIN Marca m ON p.Marca_ID = m.ID
                    INNER JOIN Categoria c ON p.Categoria_ID = c.ID
                    WHERE v.Fecha_Hora >= @Inicio AND v.Fecha_Hora <= @Fin ";

                if (marca != "Todas") query += " AND m.Nombre = @Marca ";
                if (categoria != "Todas") query += " AND c.Nombre = @Categoria ";

                query += @" GROUP BY p.Codigo_Modelo, p.Nombre, m.Nombre, c.Nombre 
                            ORDER BY CantidadVendida DESC";

                using (var cmd = new SqlCommand(query, conexion))
                {
                    cmd.Parameters.AddWithValue("@Inicio", inicio.Date);
                    cmd.Parameters.AddWithValue("@Fin", fin.Date.AddDays(1).AddSeconds(-1));

                    if (marca != "Todas") cmd.Parameters.AddWithValue("@Marca", marca);
                    if (categoria != "Todas") cmd.Parameters.AddWithValue("@Categoria", categoria);

                    conexion.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(new ProductoVendidoDTO
                            {
                                Codigo = reader["Codigo_Modelo"].ToString(),
                                Nombre = reader["Nombre"].ToString(),
                                Marca = reader["Marca"].ToString(),
                                Categoria = reader["Categoria"].ToString(),
                                CantidadVendida = Convert.ToInt32(reader["CantidadVendida"]),
                                TotalGenerado = Convert.ToDecimal(reader["TotalGenerado"])
                            });
                        }
                    }
                }
            }
            return lista;
        }
    }
}