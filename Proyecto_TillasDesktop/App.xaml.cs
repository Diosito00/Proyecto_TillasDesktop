using System.Configuration;
using System.Data;
using System.Windows;

namespace Proyecto_TillasDesktop
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        // Variables estáticas accesibles desde cualquier parte del sistema
        public static string NombreUsuarioActual { get; set; }
        public static int IdUsuarioActual { get; set; }
    }

}
