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

            // Cargar la vista de Inicio por defecto al arrancar el programa
            AreaPrincipal.Content = new VistaInicio();
        }

        private void BtnInicio_Click(object sender, RoutedEventArgs e)
        {
            AreaPrincipal.Content = new VistaInicio();
        }

        private void BtnClientes_Click(object sender, RoutedEventArgs e)
        {
            // Instancia el UserControl de Clientes (asegúrate de haberlo creado en la carpeta Views)
            // AreaPrincipal.Content = new VistaClientes();
        }

        private void BtnReportes_Click(object sender, RoutedEventArgs e)
        {
            // Instancia el UserControl de Reportes
            // AreaPrincipal.Content = new VistaReportes();
        }

        // Métodos vacíos listos para cuando crees el resto de las vistas
        private void BtnPuntoVenta_Click(object sender, RoutedEventArgs e)
        {
            // AreaPrincipal.Content = new VistaPuntoVenta();
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
