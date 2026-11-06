// Define el espacio de nombres (namespace) perteneciente a las entidades de facturación del sistema
namespace TillasDesktop.Entities.Facturacion
{
    // Declara la clase pública ProductoCatalogoDTO (Data Transfer Object), utilizada para empaquetar 
    // y transportar los datos consolidando productos, talles y stock disponible para mostrarlos en el catálogo de caja.
    public class ProductoCatalogoDTO
    {
        // Propiedad entera que almacena el identificador único de la relación Producto-Talle específico
        public int ProductoTalle_ID { get; set; }

        // Propiedad de tipo string que almacena el código de modelo o SKU del producto
        public string Codigo_Modelo { get; set; }

        // Propiedad de tipo string que almacena el nombre comercial o descripción del producto
        public string Nombre { get; set; }

        // Propiedad de tipo string que almacena el nombre de la marca asociada al producto (obtenido mediante JOIN)
        public string NombreMarca { get; set; }

        // Propiedad de tipo string que almacena el nombre de la categoría del producto (obtenido mediante JOIN)
        public string NombreCategoria { get; set; }

        // Propiedad entera que indica el número de talle correspondiente a esta variante del producto
        public int Talle { get; set; }

        // Propiedad entera que almacena la cantidad física real disponible para la venta 
        // (calculada al vuelo en la consulta SQL mediante la diferencia: Stock_Actual - Stock_Reserva)
        public int Stock_Disponible { get; set; } // El resultado de Stock_Actual - Stock_Reserva

        // Propiedad de tipo decimal que guarda el precio de venta vigente asignado al producto
        public decimal Precio_Venta { get; set; }
    }
}