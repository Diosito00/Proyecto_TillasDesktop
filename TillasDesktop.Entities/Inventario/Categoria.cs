// Define el espacio de nombres (namespace) correspondiente a las entidades de inventario del sistema
namespace TillasDesktop.Entities.Inventario
{
    // Declara la clase pública Categoria, que modela las clasificaciones o grupos a los que pertenecen los productos
    public class Categoria
    {
        // Propiedad entera que almacena el identificador único (ID / Clave primaria) de la categoría en la base de datos
        public int ID { get; set; }

        // Propiedad de tipo string que almacena el nombre o denominación descriptiva de la categoría
        public string Nombre { get; set; }

        // Propiedad de tipo booleano que indica si la categoría está activa para ser seleccionada (true) o dada de baja lógica (false)
        public bool Activo { get; set; }
    }
}