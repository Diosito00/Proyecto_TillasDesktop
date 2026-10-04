namespace TillasDesktop.Entities.Facturacion
{
    public class ProductoCatalogoDTO
    {
        public int ProductoTalle_ID { get; set; }
        public string Codigo_Modelo { get; set; }
        public string Nombre { get; set; }
        public string NombreMarca { get; set; }
        public string NombreCategoria { get; set; }
        public int Talle { get; set; }
        public int Stock_Disponible { get; set; } // El resultado de Stock_Actual - Stock_Reserva
        public decimal Precio_Venta { get; set; }
    }
}