// Define el espacio de nombres (namespace) perteneciente a las entidades de facturación del sistema
namespace TillasDesktop.Entities.Facturacion
{
    // Declara la clase pública DetalleVenta, que modela cada renglón o ítem individual dentro de un comprobante de venta
    public class DetalleVenta
    {
        // Propiedad entera que almacena el identificador único (ID / Clave primaria) del registro de detalle en la base de datos
        public int ID { get; set; }

        // Propiedad entera que actúa como clave foránea (Foreign Key) para vincular este renglón con su respectiva cabecera de venta
        public int Venta_ID { get; set; }

        // Propiedad entera que almacena la clave foránea vinculada al talle específico del producto comercializado en este renglón
        public int Producto_Talle_ID { get; set; }

        // Propiedad entera que indica cuántas unidades de ese producto y talle específico fueron adquiridas en la transacción
        public int Cantidad { get; set; }

        // Propiedad de tipo decimal que guarda el precio unitario de venta que tenía el producto al momento de la operación
        public decimal Precio_Unitario { get; set; }

        // Propiedad de tipo decimal que almacena el monto total parcial (Cantidad * Precio_Unitario) correspondiente a esta línea
        public decimal Subtotal { get; set; }
    }
}