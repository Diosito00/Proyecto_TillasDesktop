namespace TillasDesktop.Entities.Reportes
{
    public class RendimientoVendedorDTO
    {
        public string Vendedor { get; set; }
        public int CantidadVentas { get; set; }
        public decimal TotalRecaudado { get; set; }
        public decimal PromedioPorVenta => CantidadVentas > 0 ? TotalRecaudado / CantidadVentas : 0;
    }
}