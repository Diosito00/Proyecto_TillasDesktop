namespace TillasDesktop.Entities.Usuarios
{
    public class Usuario
    {
        public int Id_Usuario { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Dni { get; set; }
        public string Email { get; set; }
        public string Nombre_Usuario { get; set; }
        public string Password { get; set; }
        public DateTime Fecha_Nacimiento { get; set; }
        public string Rol { get; set; }
        public bool Activo { get; set; }
    }
}
