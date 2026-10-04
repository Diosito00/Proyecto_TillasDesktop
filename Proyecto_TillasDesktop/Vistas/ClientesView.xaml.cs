using System.Windows.Controls;
using TillasDesktop.UI.Modelos;

namespace TillasDesktop.UI.Vistas
{
    // Declaración de la clase parcial ClientesView, que hereda de UserControl
    public partial class ClientesView : UserControl
    {
        public ClientesView()
        {
            InitializeComponent();

            // Asignación directa del ViewModel mediante código (igual que en UsuariosView)
            this.DataContext = new GestionClienteViewModel();
        }
    }
}