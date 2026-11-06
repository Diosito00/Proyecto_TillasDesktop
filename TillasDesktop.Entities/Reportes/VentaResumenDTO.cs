// Define el espacio de nombres (namespace) perteneciente a las entidades y DTOs de reportes del sistema
namespace TillasDesktop.Entities.Reportes
{
    // Declara la clase pública VentaResumenDTO (Data Transfer Object), utilizada para empaquetar 
    // y estructurar la información resumida de cada comprobante de venta destinada a listados generales, grillas de consulta y reportes.
    public class VentaResumenDTO
    {
        // Propiedad entera que almacena el identificador único (ID / Clave primaria) de la venta
        public int ID { get; set; }

        // Propiedad de tipo DateTime que registra la fecha y la hora exacta en la que se efectuó la transacción
        public DateTime Fecha_Hora { get; set; }

        // Propiedad de tipo string que almacena el nombre, apellido o identificación del vendedor que procesó la operación (obtenido mediante JOIN)
        public string NombreVendedor { get; set; }

        // Propiedad de tipo string que almacena el nombre completo del cliente que realizó la compra (obtenido mediante JOIN)
        public string NombreCliente { get; set; }

        // Propiedad de tipo string que almacena el nombre o etiqueta del método de pago utilizado (ej. Efectivo, Tarjeta, etc., obtenido mediante JOIN)
        public string MetodoPago { get; set; }

        // Propiedad de tipo decimal que guarda el monto económico total final de la venta
        public decimal Total { get; set; }
    }
}