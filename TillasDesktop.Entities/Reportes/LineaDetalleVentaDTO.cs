// Define el espacio de nombres (namespace) perteneciente a las entidades y DTOs de reportes del sistema
namespace TillasDesktop.Entities.Reportes
{
    // Declara la clase pública LineaDetalleVentaDTO (Data Transfer Object), utilizada para empaquetar 
    // y estructurar la información detallada de cada ítem al consultar o imprimir el comprobante de una venta.
    public class LineaDetalleVentaDTO
    {
        // Propiedad de tipo string que almacena el código de modelo o referencia alfanumérica del producto vendido
        public string CodigoModelo { get; set; }

        // Propiedad de tipo string que almacena el nombre comercial o descripción del producto
        public string NombreProducto { get; set; }

        // Propiedad de tipo string que almacena el nombre de la marca asociada al producto
        public string Marca { get; set; }

        // Propiedad de tipo decimal que indica el talle del calzado correspondiente a este renglón de la venta
        public decimal Talle { get; set; }

        // Propiedad entera que indica la cantidad de unidades de este producto y talle que fueron adquiridas
        public int Cantidad { get; set; }

        // Propiedad de tipo decimal que guarda el precio unitario que tenía el producto al momento de la transacción
        public decimal PrecioUnitario { get; set; }

    }
}