// Define el espacio de nombres (namespace) perteneciente a las entidades y DTOs de reportes del sistema
namespace TillasDesktop.Entities.Reportes
{
    // Declara la clase pública RendimientoVendedorDTO (Data Transfer Object), utilizada para empaquetar 
    // y estructurar las métricas de desempeño y estadísticas de ventas asociadas a cada operador o vendedor.
    public class RendimientoVendedorDTO
    {
        // Propiedad de tipo string que almacena el nombre, apellido o identificación del vendedor evaluado
        public string Vendedor { get; set; }

        // Propiedad entera que almacena la cantidad total de operaciones o comprobantes de venta realizados por el vendedor
        public int CantidadVentas { get; set; }

        // Propiedad de tipo decimal que guarda la recaudación económica total acumulada por las ventas del vendedor
        public decimal TotalRecaudado { get; set; }

        // Propiedad calculada de solo lectura mediante una expresión condicional ternaria
        // Valida que la cantidad de ventas sea mayor a cero para evitar divisiones por cero; 
        // si se cumple, divide el total recaudado entre la cantidad de ventas para obtener el ticket promedio, de lo contrario retorna 0
        public decimal PromedioPorVenta => CantidadVentas > 0 ? TotalRecaudado / CantidadVentas : 0;
    }
}