using System;
using System.Collections.Generic;
using System.Linq;
using TillasDesktop.DAL.Repositorios;
using TillasDesktop.Entities.Reportes;

namespace TillasDesktop.BLL.Services
{
    public class ReportesService
    {
        private readonly ReportesRepository _repoReportes;
        private readonly InventarioService _servicioInventario;

        public ReportesService()
        {
            _repoReportes = new ReportesRepository();
            _servicioInventario = new InventarioService();
        }

        public List<VentaResumenDTO> GenerarReporteVentas(DateTime inicio, DateTime fin)
        {
            try { return _repoReportes.ObtenerReporteVentas(inicio, fin); }
            catch (Exception ex) { throw new Exception("Error al cargar el reporte de ventas: " + ex.Message); }
        }

        public List<RendimientoVendedorDTO> GenerarReporteVendedores(DateTime inicio, DateTime fin)
        {
            try { return _repoReportes.ObtenerRendimientoVendedores(inicio, fin); }
            catch (Exception ex) { throw new Exception("Error al cargar el rendimiento de vendedores: " + ex.Message); }
        }

        public List<ProductoVendidoDTO> GenerarReporteProductos(DateTime inicio, DateTime fin, string marca, string categoria)
        {
            try { return _repoReportes.ObtenerProductosMasVendidos(inicio, fin, marca, categoria); }
            catch (Exception ex) { throw new Exception("Error al cargar el reporte de productos: " + ex.Message); }
        }

        public List<string> ObtenerMarcasParaFiltro()
        {
            try
            {
                var marcas = _servicioInventario.ObtenerMarcasActivas();
                return marcas.Select(m => m.Nombre).OrderBy(n => n).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al cargar marcas desde el inventario: " + ex.Message);
            }
        }

        public List<string> ObtenerCategoriasParaFiltro()
        {
            try
            {
                var categorias = _servicioInventario.ObtenerCategoriasActivas();
                return categorias.Select(c => c.Nombre).OrderBy(n => n).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al cargar categorías desde el inventario: " + ex.Message);
            }
        }
    }
}