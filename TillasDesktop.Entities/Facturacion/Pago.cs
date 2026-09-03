using System;
using System.Collections.Generic;
using System.Text;

namespace TillasDesktop.Entities.Facturacion
{
    public class Pago
    {
        public int ID { get; set; }
        public int Tipo_Pago_ID { get; set; }
        public decimal Monto { get; set; }
        public DateTime Fecha_Pago { get; set; }
        public bool Activo { get; set; }
    }
}
