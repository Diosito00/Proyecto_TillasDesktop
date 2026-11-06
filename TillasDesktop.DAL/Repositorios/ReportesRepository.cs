// Importa clases fundamentales del sistema (como conversiones y tipos básicos)
using System;
// Importa colecciones genéricas para trabajar con listas tipadas (List<T>)
using System.Collections.Generic;
// Importa el proveedor oficial de SQL Server para .NET (Microsoft.Data.SqlClient) para ejecutar comandos y consultas
using Microsoft.Data.SqlClient;
// Importa la clase base de conexión para heredar el método de obtención de canales con la base de datos
using TillasDesktop.DAL.Base;
// Importa las entidades o DTOs (Data Transfer Objects) destinados a almacenar los resultados de los reportes
using TillasDesktop.Entities.Reportes;

// Define el espacio de nombres correspondiente a los repositorios de la capa de acceso a datos (DAL)
namespace TillasDesktop.DAL.Repositorios
{
    // Clase de acceso a datos especializada en la generación y extracción de reportes y estadísticas del sistema.
    // Hereda de ConexionDb para administrar de forma centralizada y segura las conexiones a SQL Server.
    public class ReportesRepository : ConexionDb
    {
        // Método para obtener un resumen detallado de todas las ventas realizadas dentro de un rango de fechas específico.
        public List<VentaResumenDTO> ObtenerReporteVentas(DateTime inicio, DateTime fin)
        {
            // Instancia una lista genérica vacía para recolectar los DTOs de resumen de ventas
            var lista = new List<VentaResumenDTO>();

            // Solicita una conexión abierta a la base de datos utilizando el patrón using para liberar recursos automáticamente
            using (var conexion = ObtenerConexion())
            {
                // Define la sentencia SQL con múltiples uniones (JOINs) para relacionar ventas, usuarios, clientes y medios de pago
                string query = @"
                    SELECT 
                        v.ID, 
                        v.Fecha_Hora, 
                        CONCAT(u.nombre, ' ', u.apellido) AS Vendedor, 
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

                // Prepara el comando SQL vinculando la consulta y la conexión activa
                using (var cmd = new SqlCommand(query, conexion))
                {
                    // Asigna el parámetro de inicio tomando solo la fecha (00:00:00)
                    cmd.Parameters.AddWithValue("@Inicio", inicio.Date);

                    // Asigna el parámetro de fin configurándolo hasta el último segundo del día (23:59:59)
                    cmd.Parameters.AddWithValue("@Fin", fin.Date.AddDays(1).AddSeconds(-1));

                    // Abre físicamente el canal con el motor de base de datos
                    conexion.Open();

                    // Ejecuta la consulta mediante un lector de datos de flujo continuo
                    using (var reader = cmd.ExecuteReader())
                    {
                        // Itera renglón por renglón mientras existan registros devueltos
                        while (reader.Read())
                        {
                            // Mapea cada fila de la base de datos a un objeto VentaResumenDTO y lo añade a la lista
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
            // Retorna la lista completa de resúmenes de ventas filtrados
            return lista;
        }

        // Método para calcular el rendimiento y desempeño comercial de cada vendedor en un período determinado.
        public List<RendimientoVendedorDTO> ObtenerRendimientoVendedores(DateTime inicio, DateTime fin)
        {
            // Instancia la lista para almacenar el ranking de vendedores
            var lista = new List<RendimientoVendedorDTO>();

            // Abre la conexión mediante un bloque using
            using (var conexion = ObtenerConexion())
            {
                // Sentencia SQL que agrupa por vendedor calculando la cantidad de ventas realizadas y el total recaudado
                string query = @"
                    SELECT 
                        CONCAT(u.nombre, ' ', u.apellido) AS Vendedor, 
                        COUNT(v.ID) AS CantidadVentas, 
                        SUM(v.Total) AS TotalRecaudado 
                    FROM Ventas v
                    INNER JOIN Usuarios u ON v.Usuario_ID = u.id_usuario
                    WHERE v.Fecha_Hora >= @Inicio AND v.Fecha_Hora <= @Fin
                    GROUP BY u.nombre, u.apellido
                    ORDER BY TotalRecaudado DESC";

                // Prepara el comando SQL asociado
                using (var cmd = new SqlCommand(query, conexion))
                {
                    // Asigna los parámetros de fecha de inicio y fin de manera segura
                    cmd.Parameters.AddWithValue("@Inicio", inicio.Date);
                    cmd.Parameters.AddWithValue("@Fin", fin.Date.AddDays(1).AddSeconds(-1));

                    // Abre la conexión con el servidor
                    conexion.Open();
                    // Ejecuta el lector de datos
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // Mapea los resultados agrupados a un objeto RendimientoVendedorDTO
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
            // Retorna la lista ordenada de rendimiento por vendedor
            return lista;
        }

        // Método para consultar los productos más vendidos permitiendo filtrar opcionalmente por marca y categoría.
        public List<ProductoVendidoDTO> ObtenerProductosMasVendidos(DateTime inicio, DateTime fin, string marca, string categoria)
        {
            // Instancia la lista genérica para los productos más vendidos
            var lista = new List<ProductoVendidoDTO>();

            // Inicia la conexión utilizando el patrón using
            using (var conexion = ObtenerConexion())
            {

                // Construye la base de la consulta SQL uniendo el detalle de ventas, talles, productos, marcas y categorías
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

                // Evalúa de forma dinámica si se seleccionó una marca específica distinta a "Todas" para agregar el filtro SQL
                if (marca != "Todas") query += " AND m.Nombre = @Marca ";

                // Evalúa si se seleccionó una categoría específica distinta a "Todas" para concatenar el filtro
                if (categoria != "Todas") query += " AND c.Nombre = @Categoria ";

                // Agrega el agrupamiento por atributos del producto y ordena de mayor a menor según la cantidad vendida
                query += @" GROUP BY p.Codigo_Modelo, p.Nombre, m.Nombre, c.Nombre 
                            ORDER BY CantidadVendida DESC";


                // Prepara el comando SQL con la consulta dinámica armada
                using (var cmd = new SqlCommand(query, conexion))
                {
                    // Asigna los parámetros obligatorios de fechas
                    cmd.Parameters.AddWithValue("@Inicio", inicio.Date);
                    cmd.Parameters.AddWithValue("@Fin", fin.Date.AddDays(1).AddSeconds(-1));

                    // Añade condicionalmente el parámetro de marca si fue requerido en el filtro
                    if (marca != "Todas") cmd.Parameters.AddWithValue("@Marca", marca);

                    // Añade condicionalmente el parámetro de categoría si fue requerido en el filtro
                    if (categoria != "Todas") cmd.Parameters.AddWithValue("@Categoria", categoria);

                    // Abre la conexión física
                    conexion.Open();

                    // Ejecuta el lector de datos
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // Mapea cada registro obtenido a un objeto ProductoVendidoDTO y lo agrega a la colección
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
            // Retorna la lista final con el ranking de productos más vendidos
            return lista;
        }
    }
}