using System.Windows.Controls;
using TillasDesktop.UI.Modelos;

namespace TillasDesktop.UI.Vistas
{
    // Declaración de la clase parcial UsuariosView, que hereda de UserControl (control de usuario reutilizable de WPF).
    // Funciona como la vista o interfaz visual principal del módulo de gestión de usuarios.
    public partial class UsuariosView : UserControl
    {
        // Constructor de la clase: Se ejecuta cuando se instancia o carga la vista dentro de la aplicación.
        public UsuariosView()
        {
            InitializeComponent(); // Carga y dibuja todos los elementos gráficos definidos en el archivo XAML asociado.

            // Asigna una nueva instancia de UsuariosViewModel al DataContext de la vista.
            // Esto establece el enlace de datos (Data Binding) permitiendo que la interfaz se comunique con la lógica y los comandos del ViewModel.
            this.DataContext = new GestionUsuarioViewModel();
        }
    }
}