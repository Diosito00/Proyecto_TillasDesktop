using System;
using System.Collections.Generic;
using System.Text;

namespace TillasDesktop.Entities.Facturacion
{
    public class TipoPago
    {
        public int ID { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public bool Activo { get; set; }
    }
}
