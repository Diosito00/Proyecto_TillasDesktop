// Define el espacio de nombres (namespace) correspondiente a las entidades de clientes del sistema
namespace TillasDesktop.Entities.Clientes
{
    // Declara la clase pública Cliente, que modela la información personal y comercial de los compradores
    public class Cliente
    {
        // Propiedad entera que almacena el identificador único (ID / Clave primaria) del cliente en la base de datos
        public int ID { get; set; }

        // Propiedad de tipo string que almacena el apellido del cliente
        public string Apellido { get; set; }

        // Propiedad de tipo string que almacena el nombre de pila del cliente
        public string Nombre { get; set; }

        // Propiedad de solo lectura calculada que unifica el nombre y el apellido en una sola cadena de texto formateada
        public string NombreCompleto
        {
            // Define el accesor get para concatenar el nombre y el apellido eliminando espacios sobrantes mediante Trim()
            get { return $"{Nombre} {Apellido}".Trim(); }
        }

        // Propiedad de tipo string que almacena el CUIT, CUIL o documento de identificación fiscal del cliente
        public string CUIT { get; set; }

        // Propiedad de tipo string que guarda el número de teléfono o contacto telefónico del cliente
        public string Telefono { get; set; }

        // Propiedad de tipo string que almacena la dirección de correo electrónico del cliente
        public string Email { get; set; }
    }
}