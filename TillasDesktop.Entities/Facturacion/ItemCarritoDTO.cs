// Define el espacio de nombres (namespace) correspondiente a las entidades de facturación del sistema
namespace TillasDesktop.Entities.Facturacion
{
    // Declara la clase pública ItemCarritoDTO (Data Transfer Object), utilizada para empaquetar y transferir 
    // la información de los productos seleccionados temporalmente en el carrito de compras de la caja.
    public class ItemCarritoDTO
    {
        // Propiedad entera que almacena la clave foránea o identificador único del talle específico del producto en el inventario
        public int Producto_Talle_ID { get; set; }

        // Propiedad de tipo string que almacena el nombre comercial o descripción principal del producto
        public string Nombre { get; set; }

        // Propiedad entera que indica el número de talle correspondiente al calzado o artículo seleccionado
        public int Talle { get; set; }

        // Propiedad de tipo decimal que guarda el precio unitario vigente del producto dentro del carrito
        public decimal Precio_Unitario { get; set; }

        // Propiedad entera que representa la cantidad de unidades que el usuario cargó o reservó para este ítem
        public int Cantidad { get; set; }
    }
}