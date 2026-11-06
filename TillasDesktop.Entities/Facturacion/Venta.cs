// Define el espacio de nombres (namespace) perteneciente a las entidades de facturación del sistema
namespace TillasDesktop.Entities.Facturacion
{
    // Declara la clase pública Venta, que modela la cabecera de un comprobante o transacción comercial
    public class Venta
    {
        // Propiedad entera que almacena el identificador único (ID / Clave primaria) de la venta
        public int ID { get; set; }

        // Propiedad de tipo DateTime que registra la fecha y la hora exacta en que se efectuó la operación
        public DateTime Fecha_Hora { get; set; }

        // Propiedad entera que representa la clave foránea (Foreign Key) del usuario o vendedor que procesó la venta
        public int Usuario_ID { get; set; }

        // Propiedad entera que almacena la clave foránea del cliente comprador asociado a la transacción
        public int Cliente_ID { get; set; }

        // Propiedad de tipo decimal que guarda el monto económico total final abonado en la venta
        public decimal Total { get; set; }

        // Propiedad entera que guarda la clave foránea correspondiente al registro de pago vinculado a esta venta
        public int Pago_ID { get; set; }
    }
}