namespace TillasDesktop.Entities.Reportes
{
    public class LineaDetalleVentaDTO
    {
        public string CodigoModelo { get; set; }
        public string NombreProducto { get; set; }
        public string Marca { get; set; }
        public decimal Talle { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }

        // Propiedad calculada solo lectura
        public decimal Subtotal => Cantidad * PrecioUnitario;
    }
}