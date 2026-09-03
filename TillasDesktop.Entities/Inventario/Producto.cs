using System;
using System.Collections.Generic;
using System.Text;

namespace TillasDesktop.Entities.Inventario
{
    public class Producto
    {
        public int ID { get; set; }
        public string Codigo_Modelo { get; set; }
        public string Nombre { get; set; }
        public int Marca_ID { get; set; }
        public int Categoria_ID { get; set; }
        public decimal Precio_Venta { get; set; }
        public bool Activo { get; set; }
    }
}
