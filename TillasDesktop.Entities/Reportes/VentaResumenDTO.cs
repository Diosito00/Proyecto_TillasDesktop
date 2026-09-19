namespace TillasDesktop.Entities.Reportes
{
    public class VentaResumenDTO
    {
        public int ID { get; set; }
        public DateTime Fecha_Hora { get; set; }
        public string NombreVendedor { get; set; }
        public string NombreCliente { get; set; }
        public string MetodoPago { get; set; }
        public decimal Total { get; set; }
    }
}
