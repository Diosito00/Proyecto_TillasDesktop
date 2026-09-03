using System;
using System.Collections.Generic;
using System.Text;

namespace TillasDesktop.Entities.Inventario
{
    public class ProductoTalle
    {
        public int ID { get; set; }
        public int Producto_ID { get; set; }
        public decimal Talle { get; set; }
        public string Codigo_Barras_Unico { get; set; }
        public int Stock_Actual { get; set; }
    }
}
