// Define el espacio de nombres (namespace) correspondiente a las entidades de inventario del sistema
namespace TillasDesktop.Entities.Inventario
{
    // Declara la clase pública Marca, que modela los fabricantes o marcas comerciales asociadas a los productos
    public class Marca
    {
        // Propiedad entera que almacena el identificador único (ID / Clave primaria) de la marca en la base de datos
        public int ID { get; set; }

        // Propiedad de tipo string que almacena el nombre comercial o denominación de la marca
        public string Nombre { get; set; }

        // Propiedad de tipo booleano que indica si la marca se encuentra activa para su uso (true) o dada de baja lógica (false)
        public bool Activo { get; set; }
    }
}