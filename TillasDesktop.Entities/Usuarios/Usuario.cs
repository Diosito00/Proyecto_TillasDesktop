using System;
using System.Collections.Generic;
using System.Text;

namespace TillasDesktop.Entities.Usuarios
{
    public class Usuario
    {
        public int ID { get; set; }
        public string Nombre { get; set; }
        public string DNI { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Rol { get; set; }
        public bool Activo { get; set; }
    }
}
