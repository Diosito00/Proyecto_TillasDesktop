using TillasDesktop.Entities.Inventario;
using TillasDesktop.Entities.Facturacion;

namespace TillasDesktop.BLL.Services
{
    public class VentasService
    {
        // Catálogo temporal simulado para las pruebas iniciales del Punto de Venta.
        private readonly List<Producto> _catalogoPrueba;

        public VentasService()
        {
            _catalogoPrueba = new List<Producto>
            {
                new Producto { Codigo_Modelo = "779001", Nombre = "Air Force 1 - Talle 42", Precio_Venta = 125000 },
                new Producto { Codigo_Modelo = "779002", Nombre = "Samba OG - Talle 39", Precio_Venta = 110000 }
            };
        }

        // Busca y devuelve un producto del catálogo interno según su código de modelo.
        public Producto ObtenerProductoPorCodigo(string codigo)
        {
            if (string.IsNullOrWhiteSpace(codigo)) return null;

            // LINQ: FirstOrDefault busca el primer elemento que coincide con la condición.
            // Si no lo encuentra, devuelve null de forma segura en lugar de lanzar una excepción.
            return _catalogoPrueba.FirstOrDefault(p => p.Codigo_Modelo == codigo);
        }

        // Recibe IEnumerable en lugar de List u ObservableCollection. Esto hace que el método sea mucho más 
        // versátil, ya que puede aceptar cualquier tipo de colección que se pueda iterar.
        public decimal CalcularTotalVenta(IEnumerable<DetalleVenta> detalles)
        {
            // LINQ: .Sum() recorre cada 'item' del carrito, multiplica el precio por la cantidad, 
            // y suma todos los resultados en una sola línea de código.
            return detalles.Sum(item => item.Precio_Unitario * item.Cantidad);
        }

        // Valida que el carrito tenga elementos antes de procesar el cobro y registrar la venta.
        public bool RegistrarVenta(IEnumerable<DetalleVenta> carrito, out string mensajeRespuesta)
        {
            // LINQ: .Any() verifica si la colección tiene al menos 1 elemento. 
            // Al negarlo (!), verificamos rápidamente si el carrito está vacío.
            if (!carrito.Any())
            {
                mensajeRespuesta = "No hay productos para cobrar.";
                return false;
            }

            mensajeRespuesta = "Venta registrada exitosamente en el sistema.";
            return true;
        }
    }
}