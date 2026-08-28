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
using TillasDesktop.UI.Vistas;

namespace TillasDesktop.UI.Vistas
{
    /// <summary>
    /// Lógica de interacción para MenuInicio.xaml
    /// </summary>
    public partial class MenuInicio : Window
    {
        public MenuInicio()
        {
            InitializeComponent();
            AreaPrincipal.Content = new VistaInicio();
        }

        private void BtnInicio_Click(object sender, RoutedEventArgs e)
        {
            AreaPrincipal.Content = new VistaInicio();
        }

        private void BtnClientes_Click(object sender, RoutedEventArgs e)
        {
<<<<<<< Updated upstream
            // AreaPrincipal.Content = new VistaClientes();
=======
            AreaPrincipal.Content = new TillasDesktop.UI.Vistas.ClientesView();
>>>>>>> Stashed changes
        }

        private void BtnReportes_Click(object sender, RoutedEventArgs e)
        {
            // AreaPrincipal.Content = new VistaReportes();
        }

        private void BtnPuntoVenta_Click(object sender, RoutedEventArgs e)
        {
            AreaPrincipal.Content = new VistaPuntoDeVenta();
        }

        private void BtnInventario_Click(object sender, RoutedEventArgs e)
        {
            // AreaPrincipal.Content = new VistaInventario();
        }

        private void BtnConfiguracion_Click(object sender, RoutedEventArgs e)
        {
            // AreaPrincipal.Content = new VistaConfiguracion();
        }
    }
}
