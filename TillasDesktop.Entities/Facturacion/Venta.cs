using System;
using System.Collections.Generic;
using System.Text;

namespace TillasDesktop.Entities.Facturacion
{
    public class Venta
    {
        public int ID { get; set; }
        public DateTime Fecha_Hora { get; set; }
        public int Usuario_ID { get; set; }
        public int Cliente_ID { get; set; }
        public decimal Total { get; set; }
        public int Pago_ID { get; set; }
    }
}
