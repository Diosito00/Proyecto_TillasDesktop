using System.Windows.Controls;
using TillasDesktop.UI.Modelos;

namespace TillasDesktop.UI.Vistas
{
    public partial class InicioView : UserControl
    {
        public InicioView()
        {
            InitializeComponent();

            // Inyectamos el ViewModel
            this.DataContext = new InicioViewModel();
        }
    }
}