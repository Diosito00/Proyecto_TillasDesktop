// Define el espacio de nombres (namespace) correspondiente a las entidades de inventario del sistema
namespace TillasDesktop.Entities.Inventario
{
    // Declara la clase pública Producto, que modela los artículos o zapatillas comercializadas en el inventario
    public class Producto
    {
        // Propiedad entera que almacena el identificador único (ID / Clave primaria) del producto en la base de datos
        public int ID { get; set; }

        // Propiedad de tipo string que almacena el código de modelo, SKU o referencia alfanumérica única del producto
        public string Codigo_Modelo { get; set; }

        // Propiedad de tipo string que almacena el nombre comercial o descripción del producto
        public string Nombre { get; set; }

        // Propiedad entera que actúa como clave foránea (Foreign Key) para vincular el producto con su marca correspondiente
        public int Marca_ID { get; set; }

        // Propiedad entera que actúa como clave foránea (Foreign Key) para vincular el producto con su categoría correspondiente
        public int Categoria_ID { get; set; }

        // Propiedad de tipo decimal que guarda el precio de venta al público fijado para el producto
        public decimal Precio_Venta { get; set; }

        // Propiedad de tipo booleano que indica si el producto se encuentra activo y disponible para la venta (true) o dado de baja lógica (false)
        public bool Activo { get; set; }
    }
}