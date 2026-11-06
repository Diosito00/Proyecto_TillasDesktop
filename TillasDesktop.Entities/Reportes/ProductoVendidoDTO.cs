// Define el espacio de nombres (namespace) perteneciente a las entidades y DTOs de reportes del sistema
namespace TillasDesktop.Entities.Reportes
{
    // Declara la clase pública ProductoVendidoDTO (Data Transfer Object), utilizada para empaquetar 
    // y estructurar la información estadística y métricas de rendimiento comercial de los productos vendidos.
    public class ProductoVendidoDTO
    {
        // Propiedad de tipo string que almacena el código de modelo o referencia alfanumérica del producto
        public string Codigo { get; set; }

        // Propiedad de tipo string que almacena el nombre comercial o descripción principal del producto
        public string Nombre { get; set; }

        // Propiedad de tipo string que almacena el nombre de la marca asociada al producto
        public string Marca { get; set; }

        // Propiedad de tipo string que almacena la categoría a la que pertenece el producto
        public string Categoria { get; set; }

        // Propiedad entera que almacena la cantidad total acumulada de unidades vendidas de este artículo en un período o reporte específico
        public int CantidadVendida { get; set; }

        // Propiedad de tipo decimal que guarda el monto monetario total generado por la comercialización de este producto
        public decimal TotalGenerado { get; set; }
    }
}