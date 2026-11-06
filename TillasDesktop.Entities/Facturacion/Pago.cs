// Define el espacio de nombres (namespace) correspondiente a las entidades de facturación del sistema
namespace TillasDesktop.Entities.Facturacion
{
    // Declara la clase pública Pago, que modela la información financiera y el registro de los pagos efectuados en las transacciones
    public class Pago
    {
        // Propiedad entera que almacena el identificador único (ID / Clave primaria) del registro de pago en la base de datos
        public int ID { get; set; }

        // Propiedad entera que actúa como clave foránea (Foreign Key) para vincular el pago con su método o tipo correspondiente (ej. efectivo, tarjeta, transferencia)
        public int Tipo_Pago_ID { get; set; }

        // Propiedad de tipo decimal que almacena el monto monetario total abonado en esta transacción de pago
        public decimal Monto { get; set; }

        // Propiedad de tipo DateTime que registra la fecha y la hora exacta en la que se procesó y registró el pago
        public DateTime Fecha_Pago { get; set; }

        // Propiedad de tipo booleano que indica si el registro de pago se encuentra activo (true) o dado de baja/anulado (false) en el sistema
        public bool Activo { get; set; }
    }
}