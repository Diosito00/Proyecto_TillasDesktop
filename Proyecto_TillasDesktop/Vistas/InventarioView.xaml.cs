using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TillasDesktop.UI.Modelos;

namespace TillasDesktop.UI.Vistas
{
    /// <summary>
    /// Lógica de interacción para VistaInventario.xaml
    /// </summary>
    public partial class InventarioView : UserControl
    {
        public InventarioView()
        {
            InitializeComponent();
            this.DataContext = new InventarioViewModel();
        }
    }
}
