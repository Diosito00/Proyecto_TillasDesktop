// Define el espacio de nombres (namespace) correspondiente a las entidades de inventario del sistema
namespace TillasDesktop.Entities.Inventario
{
    // Declara la clase pública ProductoTalle, que modela la relación intermedia entre un producto y sus talles disponibles, gestionando su inventario físico
    public class ProductoTalle
    {
        // Propiedad entera que almacena el identificador único (ID / Clave primaria) del registro de variante Producto-Talle en la base de datos
        public int ID { get; set; }

        // Propiedad entera que actúa como clave foránea (Foreign Key) para vincular este talle con su producto principal correspondiente
        public int Producto_ID { get; set; }

        // Propiedad entera que indica el número o valor del talle específico (por ejemplo, 38, 39, 40, etc.)
        public int Talle { get; set; }

        // Propiedad entera que almacena el stock o cantidad física actual disponible de este producto en este talle específico dentro del depósito
        public int Stock_Actual { get; set; }
    }
}