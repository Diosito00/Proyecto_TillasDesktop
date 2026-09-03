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
    /// Lógica de interacción para NuevoModeloWindow.xaml
    /// </summary>
    public partial class NuevoModeloWindow : Window
    {
        public NuevoModeloWindow()
        {
            InitializeComponent();
            var vm = new NuevoModeloViewModel();
            vm.CerrarVentana = this.Close; // Le enseñamos al ViewModel cómo cerrar esta ventana
            this.DataContext = vm;
        }
    }
}
