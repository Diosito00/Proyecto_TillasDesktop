
using System.Configuration;
using Microsoft.Data.SqlClient; // Paquete NuGet que instalaste antes

namespace TillasDesktop.DAL.Base
{
    public abstract class ConexionDb
    {
        // Variable privada que guarda la ruta a la base de datos
        private readonly string _cadenaConexion;

        public ConexionDb()
        {
            // Va a buscar la ruta que se llama "ConexionTillas" en el archivo App.config
            _cadenaConexion = ConfigurationManager.ConnectionStrings["ConexionTillas"].ConnectionString;
        }

        // Método protegido: entrega la llave del servidor solo a los repositorios (tablas) que lo pidan
        protected SqlConnection ObtenerConexion()
        {
            return new SqlConnection(_cadenaConexion);
        }
    }
}

