using System.Windows;
using System.Windows.Input;

namespace TillasDesktop.UI.Vistas
{
    public partial class GestionCategoriasWindow : Window
    {
        public GestionCategoriasWindow()
        {
            InitializeComponent();
        }

        private void SoloLetras_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            foreach (char c in e.Text)
            {
                // Si el carácter ingresado es un número, se bloquea la entrada
                if (char.IsDigit(c))
                {
                    e.Handled = true;
                    return;
                }
            }
        }
    }
}