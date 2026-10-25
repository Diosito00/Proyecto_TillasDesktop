using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TillasDesktop.UI.Vistas
{
    /// <summary>
    /// Lógica de interacción para IngresoStockWindow.xaml
    /// </summary>
    public partial class IngresoStockWindow : Window
    {
        public IngresoStockWindow()
        {
            InitializeComponent();
        }

        private void SoloNumeros_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
