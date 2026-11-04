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
using System.Windows.Shapes;
using TillasDesktop.UI.Modelos;

namespace TillasDesktop.UI.Vistas
{
    /// <summary>
    /// Lógica de interacción para DetalleVentaWindow.xaml
    /// </summary>
    public partial class DetalleVentaWindow : Window
    {
        public DetalleVentaWindow(dynamic ventaSeleccionada)
        {
            InitializeComponent();
            DataContext = new DetalleVentaViewModel(ventaSeleccionada);
        }
    }
}
