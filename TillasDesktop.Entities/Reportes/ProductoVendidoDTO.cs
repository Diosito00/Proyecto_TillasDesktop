namespace TillasDesktop.Entities.Reportes
{
    public class ProductoVendidoDTO
    {
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public string Marca { get; set; }
        public string Categoria { get; set; }
        public int CantidadVendida { get; set; }
        public decimal TotalGenerado { get; set; }
    }
}