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
    // Declaración de la clase parcial MenuInicioView, que hereda de Window.
    // Funciona como la ventana contenedora principal del sistema una vez que el usuario inicia sesión.
    public partial class MenuInicioView : Window
    {
        // Constructor de la ventana: Recibe el rol del usuario como parámetro para aplicar restricciones dinámicas.
        public MenuInicioView(string rolUsuario)
        {
            InitializeComponent(); // Carga y dibuja los elementos visuales definidos en el archivo XAML.

            // Aplicación del Control de Acceso Basado en Roles (RBAC) para limitar la visibilidad de los módulos del menú lateral.
            if (rolUsuario.ToLower() == "vendedor")
            {
                // Si el usuario es Vendedor, se ocultan los módulos de inventario, reportes y configuración/usuarios.
                btnInventario.Visibility = Visibility.Collapsed;
                btnReportes.Visibility = Visibility.Collapsed;
                btnConfiguracion.Visibility = Visibility.Collapsed; // Módulo bloqueado para el perfil Vendedor.
                btnBackup.Visibility = Visibility.Collapsed;
            }
            else if (rolUsuario.ToLower() == "gerente")
            {
                // Si el usuario es Gerente, se ocultan los módulos de punto de venta, clientes y configuración/usuarios.
                btnPuntoVenta.Visibility = Visibility.Collapsed;
                btnClientes.Visibility = Visibility.Collapsed;
                btnConfiguracion.Visibility = Visibility.Collapsed; // Módulo bloqueado para el perfil Gerente.
                btnBackup.Visibility = Visibility.Collapsed;
            }
            else if (rolUsuario.ToLower() == "admin")
            {
                // Si el usuario es Admin, se ocultan los módulos de punto de venta
                btnPuntoVenta.Visibility = Visibility.Collapsed;
            }

            // Carga por defecto la vista de bienvenida o inicio dentro del contenedor central al abrir la ventana.
            AreaPrincipal.Content = new InicioView();
        }

        // Evento que permite desplazar (arrastrar) la ventana por la pantalla al mantener presionado el clic izquierdo sobre la barra superior sin marco.
        private void BarraSuperior_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove(); // Método nativo de WPF para mover ventanas sin bordes.
            }
        }

        // Evento que se ejecuta al hacer clic en el botón de minimizar, reduciendo la ventana a la barra de tareas.
        private void BtnMinimizar_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        // Evento que se ejecuta al hacer clic en el botón de cierre, deteniendo y cerrando la aplicación por completo.
        private void BtnCerrar_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        // Evento de navegación: Cambia el contenido del ContentControl central para mostrar la vista de Inicio.
        private void BtnInicio_Click(object sender, RoutedEventArgs e)
        {
            AreaPrincipal.Content = new InicioView();
        }

        // Evento de navegación: Carga y muestra la vista de gestión de Clientes en el área principal.
        private void BtnClientes_Click(object sender, RoutedEventArgs e)
        {
            AreaPrincipal.Content = new ClientesView();
        }

        // Evento de navegación: Carga y muestra la vista de Reportes y estadísticas en el área principal.
        private void BtnReportes_Click(object sender, RoutedEventArgs e)
        {
            AreaPrincipal.Content = new TillasDesktop.UI.Vistas.ReportesView();
        }

        // Evento de navegación: Carga y muestra la vista del Punto de Venta (POS) en el área principal.
        private void BtnPuntoVenta_Click(object sender, RoutedEventArgs e)
        {
            AreaPrincipal.Content = new PuntoVentaView();
        }

        // Evento de navegación: Carga y muestra la vista de control de Inventario y stock en el área principal.
        private void BtnInventario_Click(object sender, RoutedEventArgs e)
        {
            AreaPrincipal.Content = new InventarioView();
        }

        // Evento de navegación: Carga y muestra la vista de administración de Usuarios y Roles en el área principal.
        private void BtnConfiguracion_Click(object sender, RoutedEventArgs e)
        {
            AreaPrincipal.Content = new UsuariosView();
        }

        // Evento de navegación: Carga y muestra la vista de Copias de Respaldo (Backup) en el área principal.
        private void BtnBackup_Click(object sender, RoutedEventArgs e)
        {
            AreaPrincipal.Content = new BackupView();
        }

        // Evento que se ejecuta al hacer clic en el botón de cerrar sesión.
        private void BtnCerrarSesion_Click(object sender, RoutedEventArgs e)
        {
            // Opcional: Preguntar al usuario si está seguro
            var confirmacion = MessageBox.Show("¿Está seguro que desea cerrar la sesión actual?", "Confirmar Cierre de Sesión", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (confirmacion == MessageBoxResult.Yes)
            {
                // Instancia una nueva ventana de Login
                LoginWindow ventanaLogin = new LoginWindow();
                ventanaLogin.Show();

                // Cierra la ventana del menú principal
                this.Close();
            }
        }
    }
}