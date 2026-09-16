using TillasDesktop.Entities.Inventario;
using TillasDesktop.Entities.Facturacion;

namespace TillasDesktop.BLL.Services
{
    public class VentasService
    {
        private readonly List<Producto> _catalogoPrueba;

        public VentasService()
        {
            _catalogoPrueba = new List<Producto>
            {
                new Producto { Codigo_Modelo = "779001", Nombre = "Air Force 1 - Talle 42", Precio_Venta = 125000 },
                new Producto { Codigo_Modelo = "779002", Nombre = "Samba OG - Talle 39", Precio_Venta = 110000 }
            };
        }

        public Producto ObtenerProductoPorCodigo(string codigo)
        {
            if (string.IsNullOrWhiteSpace(codigo)) return null;
            return _catalogoPrueba.FirstOrDefault(p => p.Codigo_Modelo == codigo);
        }

        public decimal CalcularTotalVenta(IEnumerable<DetalleVenta> detalles)
        {
            return detalles.Sum(item => item.Precio_Unitario * item.Cantidad);
        }

        public bool RegistrarVenta(IEnumerable<DetalleVenta> carrito, out string mensajeRespuesta)
        {
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