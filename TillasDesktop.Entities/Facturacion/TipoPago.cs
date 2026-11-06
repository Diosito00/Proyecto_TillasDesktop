// Define el espacio de nombres (namespace) correspondiente a las entidades de facturación del sistema
namespace TillasDesktop.Entities.Facturacion
{
    // Declara la clase pública TipoPago, que modela el catálogo de formas o métodos de pago disponibles (ej. Efectivo, Tarjeta, Transferencia)
    public class TipoPago
    {
        // Propiedad entera que almacena el identificador único (ID / Clave primaria) del tipo de pago en la base de datos
        public int ID { get; set; }

        // Propiedad de tipo string que almacena el nombre comercial o etiqueta principal de la forma de pago
        public string Nombre { get; set; }

        // Propiedad de tipo string que almacena notas, aclaraciones o una descripción detallada sobre el método de pago
        public string Descripcion { get; set; }

        // Propiedad de tipo booleano que indica si el método de pago se encuentra habilitado para su uso en caja (true) o dado de baja (false)
        public bool Activo { get; set; }
    }
}