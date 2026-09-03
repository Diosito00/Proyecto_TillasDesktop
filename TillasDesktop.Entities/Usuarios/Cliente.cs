using System;
using System.Collections.Generic;
using System.Text;

namespace TillasDesktop.Entities.Usuarios
{
    public class Cliente
    {
        public int ID { get; set; }
        public string Apellido { get; set; }
        public string Nombre { get; set; }
        public string DNI { get; set; }
        public string CUIT { get; set; }
        public string Telefono { get; set; }
        public string Email { get; set; }
    }
}
