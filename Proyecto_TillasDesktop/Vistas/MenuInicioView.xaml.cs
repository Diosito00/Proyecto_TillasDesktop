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
    public partial class MenuInicioView : Window
    {
        public MenuInicioView(string rolUsuario)
        {
            InitializeComponent();
            // Aplicación del Control de Acceso Basado en Roles (RBAC)
            if (rolUsuario == "Vendedor")
            {
                btnInventario.Visibility = Visibility.Collapsed;
                btnReportes.Visibility = Visibility.Collapsed; 
            }
            else if (rolUsuario == "Gerente")
            {
                btnPuntoVenta.Visibility = Visibility.Collapsed;
                btnClientes.Visibility = Visibility.Collapsed; 
            }
            AreaPrincipal.Content = new InicioView();
        }

        // 1. Permite arrastrar la ventana al mantener el clic presionado sobre la barra
        private void BarraSuperior_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        // 2. Botón de minimizar
        private void BtnMinimizar_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        // 3. Botón de cerrar
        private void BtnCerrar_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void BtnInicio_Click(object sender, RoutedEventArgs e)
        {
            AreaPrincipal.Content = new InicioView();
        }

        private void BtnClientes_Click(object sender, RoutedEventArgs e)
        {

            AreaPrincipal.Content = new ClientesView();

        }

        private void BtnReportes_Click(object sender, RoutedEventArgs e)
        {
            AreaPrincipal.Content = new TillasDesktop.UI.Vistas.ReportesView();
        }

        private void BtnPuntoVenta_Click(object sender, RoutedEventArgs e)
        {
            AreaPrincipal.Content = new PuntoVentaView();
        }

        private void BtnInventario_Click(object sender, RoutedEventArgs e)
        {
            AreaPrincipal.Content = new InventarioView();
        }

        private void BtnBackup_Click(object sender, RoutedEventArgs e)
        {
            AreaPrincipal.Content = new BackupView();
        }
    }
}
