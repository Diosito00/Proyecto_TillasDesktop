namespace TillasDesktop.Entities.Facturacion
{
    public class ItemCarritoDTO
    {
        public int Producto_Talle_ID { get; set; }
        public string Nombre { get; set; }
        public int Talle { get; set; }
        public decimal Precio_Unitario { get; set; }
        public int Cantidad { get; set; }
    }
}