using System;
using System.Collections.Generic;
using System.Text;

namespace TillasDesktop.Entities.Facturacion
{
    public class DetalleVenta
    {
        public int ID { get; set; }
        public int Venta_ID { get; set; }
        public int Producto_Talle_ID { get; set; }
        public int Cantidad { get; set; }
        public decimal Precio_Unitario { get; set; }
        public decimal Subtotal { get; set; }
    }
}
