// Importa clases fundamentales del sistema (como la clase base Exception)
using System;
// Importa colecciones genéricas para manejar listas de objetos (List<T>)
using System.Collections.Generic;
// Importa el espacio de nombres necesario para leer las cadenas de conexión desde el archivo de configuración (App.config / Web.config)
using System.Configuration;
// Importa el proveedor oficial de SQL Server para .NET (Microsoft.Data.SqlClient) para ejecutar comandos contra la base de datos
using Microsoft.Data.SqlClient;
// Importa LINQ para realizar transformaciones y ordenamientos sobre listas en memoria (.Select, .OrderBy, .ToList)
using System.Linq;
// Importa el repositorio de reportes de la capa de acceso a datos (DAL)
using TillasDesktop.DAL.Repositorios;
// Importa las entidades o DTOs (Data Transfer Objects) utilizados en los reportes
using TillasDesktop.Entities.Reportes;

// Define el espacio de nombres de los servicios dentro de la capa de lógica de negocio (BLL)
namespace TillasDesktop.BLL.Services
{
    // Servicio encargado de procesar y proveer toda la lógica de negocio relacionada con reportes y estadísticas del sistema
    public class ReportesService
    {
        // Instancia privada y de solo lectura del repositorio que ejecuta las consultas SQL complejas de reportes
        private readonly ReportesRepository _repoReportes;
        // Instancia privada del servicio de inventario, utilizada para obtener marcas y categorías para los filtros
        private readonly InventarioService _servicioInventario;

        // Constructor del servicio: inicializa los repositorios y servicios dependientes
        public ReportesService()
        {
            _repoReportes = new ReportesRepository();
            _servicioInventario = new InventarioService();
        }

        // Método público para generar y obtener el reporte resumido de ventas dentro de un rango de fechas
        public List<VentaResumenDTO> GenerarReporteVentas(DateTime inicio, DateTime fin)
        {
            try {
                // Llama al repositorio para traer los datos y los retorna al ViewModel o capa visual
                return _repoReportes.ObtenerReporteVentas(inicio, fin); 
            }

            catch (Exception ex) {
                // Captura cualquier error de base de datos y lanza una excepción descriptiva
                throw new Exception("Error al cargar el reporte de ventas: " + ex.Message); 
            }
        }

        // Método para obtener el rendimiento y las estadísticas de venta por cada vendedor en un período determinado
        public List<RendimientoVendedorDTO> GenerarReporteVendedores(DateTime inicio, DateTime fin)
        {
            try { 
                return _repoReportes.ObtenerRendimientoVendedores(inicio, fin);
            }
            catch (Exception ex) { 
                throw new Exception("Error al cargar el rendimiento de vendedores: " + ex.Message);
            }
        }

        // Método para obtener el reporte de productos más vendidos, permitiendo además filtrar opcionalmente por Marca y Categoría
        public List<ProductoVendidoDTO> GenerarReporteProductos(DateTime inicio, DateTime fin, string marca, string categoria)
        {
            try { 
                return _repoReportes.ObtenerProductosMasVendidos(inicio, fin, marca, categoria); 
            }
            catch (Exception ex) {
                throw new Exception("Error al cargar el reporte de productos: " + ex.Message); 
            }
        }

        // Método que obtiene la lista de nombres de marcas disponibles exclusivamente para poblar los filtros de la interfaz
        public List<string> ObtenerMarcasParaFiltro()
        {
            try
            {
                // Obtiene las marcas activas desde el servicio de inventario
                var marcas = _servicioInventario.ObtenerMarcasActivas();

                // Proyecta la lista extrayendo solo los nombres, los ordena alfabéticamente de la A a la Z y los convierte en una lista de strings
                return marcas.Select(m => m.Nombre).OrderBy(n => n).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al cargar marcas desde el inventario: " + ex.Message);
            }
        }

        // Método que obtiene la lista de nombres de categorías para poblar los filtros desplegables de la interfaz
        public List<string> ObtenerCategoriasParaFiltro()
        {
            try
            {
                // Obtiene las categorías activas y transforma la colección extrayendo y ordenando alfabéticamente sus nombres
                var categorias = _servicioInventario.ObtenerCategoriasActivas();
                return categorias.Select(c => c.Nombre).OrderBy(n => n).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al cargar categorías desde el inventario: " + ex.Message);
            }
        }

        // Método para calcular cuántos registros de talles se encuentran con stock crítico o por debajo del límite establecido (por defecto 5)
        public int ObtenerCantidadProductosBajoStock(int limiteStockMinimo = 5)
        {
            try
            {
                // Obtiene la cadena de conexión a la base de datos de Tillas desde el archivo de configuración de la app
                string conexionStr = ConfigurationManager.ConnectionStrings["ConexionTillas"].ConnectionString;

                // Consulta SQL optimizada que cuenta cuántos registros en la tabla de talles tienen un stock actual menor o igual al límite permitido
                string query = "SELECT COUNT(*) FROM Producto_Talles WHERE Stock_Actual <= @Limite";

                // Crea la conexión a SQL Server y el comando utilizando bloques 'using' para asegurar el cierre y liberación correcta de recursos
                using (var con = new SqlConnection(conexionStr))
                using (var cmd = new SqlCommand(query, con))
                {
                    // Agrega el parámetro seguro @Limite para evitar inyecciones SQL
                    cmd.Parameters.AddWithValue("@Limite", limiteStockMinimo);
                    con.Open(); // Abre la conexión física

                    // Ejecuta la consulta escalar (devuelve un único valor numérico) y lo convierte a entero para retornarlo
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
            catch
            {
                // Si ocurre algún fallo de conexión o error imprevisto, retorna 0 para evitar que la interfaz del dashboard sufra un cierre forzoso
                return 0;
            }
        }
    }
}