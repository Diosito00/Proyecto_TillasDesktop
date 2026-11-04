using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using TillasDesktop.UI.Vistas;
using TillasDesktop.BLL.Services;
using TillasDesktop.Entities.Reportes;

namespace TillasDesktop.UI.Modelos
{
    // Hereda de ViewModelBase para mantener la arquitectura MVVM. 
    // Esto permite que cualquier cambio en las propiedades notifique automáticamente a la vista (XAML).
    public class InicioViewModel : ViewModelBase
    {
        private readonly ReportesService _reportesService;


        // Propiedades enlazadas a la barra superior del Dashboard.
        public string SaludoUsuario { get; set; }
        public string RolUsuario { get; set; }
        public string FechaActual { get; set; }

        // Propiedades para las 3 tarjetas superiores. 
        // Separamos el Título del Valor para poder inyectar textos distintos dependiendo de quién inicie sesión.
        public string Kpi1Titulo { get; set; }
        public string Kpi1Valor { get; set; }
        public string Kpi2Titulo { get; set; }
        public string Kpi2Valor { get; set; }
        public string Kpi3Titulo { get; set; }
        public string Kpi3Valor { get; set; }

        // VISIBILIDAD Y TEXTOS DINÁMICOS
        // Controlan la estructura de la pantalla sin necesidad de tocar el Code-Behind.
        // Visibility es un enumerador de WPF (Visible, Collapsed, Hidden).      
        public Visibility VisibilidadObjetivos { get; set; }
        public string TituloTablaVentas { get; set; }

        // PANEL DE OBJETIVOS (Vendedores)
        // Asigna la propiedad 'Value' del ProgressBar en el XAML.
        public double ProgresoObjetivo { get; set; }

        // Texto que acompaña a la barra de progreso.
        public string TextoObjetivo { get; set; }

        // GRÁFICO Y TABLA 
        // Colección que la librería LiveCharts necesita para dibujar las líneas y puntos.
        public SeriesCollection SeriesCollectionVentas { get; set; }

        // Textos que aparecerán en el eje horizontal (X) del gráfico.
        public string[] LabelsDias { get; set; }

        // Función delegada que le enseña a LiveCharts cómo formatear los números del eje vertical (Y) a formato Moneda.
        public Func<double, string> FormatterMoneda { get; set; }

        // ObservableCollection avisa al DataGrid (Tabla) si un nuevo registro se añade o elimina en tiempo real (aun no hay conexion con la tabla Ventas).
        // Reutilizamos la entidad VentaResumenDTO de la capa de reportes.
        public ObservableCollection<VentaResumenDTO> UltimasVentasList { get; set; }

        public ICommand VerDetallesCommand { get; }

        public InicioViewModel()
        {
            _reportesService = new ReportesService();


            // CONTEXTO DE SESIÓN
            // Operadores ternarios (? :) Si la variable global viene vacía, le asigna "Admin" por defecto para que la interfaz no se rompa mostrando textos en blanco.
            string nombre = string.IsNullOrWhiteSpace(Proyecto_TillasDesktop.App.NombreUsuarioActual) ? "Admin" : Proyecto_TillasDesktop.App.NombreUsuarioActual;
            string rol = string.IsNullOrWhiteSpace(Proyecto_TillasDesktop.App.RolUsuarioActual) ? "Admin" : Proyecto_TillasDesktop.App.RolUsuarioActual;

            SaludoUsuario = $"¡Hola de nuevo, {nombre}!";
            RolUsuario = rol;

            // Formateamos la fecha. Ej: "21 septiembre 2026 | 22:30"
            FechaActual = DateTime.Now.ToString("dd MMMM yyyy | HH:mm");



            // INICIALIZAR COLECCIONES
            SeriesCollectionVentas = new SeriesCollection();
            UltimasVentasList = new ObservableCollection<VentaResumenDTO>();
            FormatterMoneda = value => value.ToString("C0");

            // CARGAR DATOS REALES DESDE LA BASE DE DATOS
            CargarDatosDesdeBaseDeDatos(nombre);

            VerDetallesCommand = new RelayCommand(AbrirDetallesVenta);
        }


        // MÉTODOS DE NEGOCIO Y LÓGICA VISUAL
        private void CargarDatosDesdeBaseDeDatos(string usuarioActual)
        {
            try
            {
                // Definimos el rango de fechas (Por ejemplo, las ventas de todo el mes actual o histórico reciente)
                DateTime hoy = DateTime.Now;
                DateTime inicioMes = new DateTime(hoy.Year, hoy.Month, 1);

                // Obtenemos todas las ventas del período a través del servicio existente
                var todasLasVentas = _reportesService.GenerarReporteVentas(inicioMes, hoy);

                // Llenar el DataGrid con las últimas ventas reales ordenadas por fecha descendente
                var ultimasVentasBD = todasLasVentas
                    .OrderByDescending(v => v.Fecha_Hora)
                    .Take(5) // Tomamos las últimas 5 ventas
                    .ToList();

                UltimasVentasList.Clear();
                foreach (var venta in ultimasVentasBD)
                {
                    UltimasVentasList.Add(venta);
                }

                // CONFIGURACIÓN SEGÚN ROL (Admin vs Vendedor)
                if (RolUsuario.ToLower() == "vendedor")
                {
                    // Filtramos las ventas correspondientes al vendedor logueado
                    var ventasVendedor = todasLasVentas
                        .Where(v => !string.IsNullOrEmpty(v.NombreVendedor) &&
                                    v.NombreVendedor.Equals(usuarioActual, StringComparison.OrdinalIgnoreCase))
                        .ToList();

                    decimal recaudacionVendedorHoy = ventasVendedor
                        .Where(v => v.Fecha_Hora.Date == hoy.Date)
                        .Sum(v => v.Total);

                    int cantidadVentasHoy = ventasVendedor.Count(v => v.Fecha_Hora.Date == hoy.Date);
                    decimal ticketPromedio = cantidadVentasHoy > 0 ? recaudacionVendedorHoy / cantidadVentasHoy : 0;

                    Kpi1Titulo = "VENTAS DE HOY";
                    Kpi1Valor = $"{cantidadVentasHoy} Operaciones";
                    Kpi2Titulo = "TICKET PROMEDIO";
                    Kpi2Valor = ticketPromedio.ToString("C0");
                    Kpi3Titulo = "RECAUDACIÓN HOY";
                    Kpi3Valor = recaudacionVendedorHoy.ToString("C0");

                    TituloTablaVentas = "MIS ÚLTIMAS VENTAS";                   
                    VisibilidadObjetivos = Visibility.Visible;

                    // Lógica para la barra de objetivos (ejemplo basado en metas de 40 pares mensuales)
                    int totalParesVendedorMes = ventasVendedor.Count;
                    ProgresoObjetivo = Math.Min((totalParesVendedorMes / 40.0) * 100, 100);
                    TextoObjetivo = $"¡Buen trabajo! Has registrado {totalParesVendedorMes} ventas este mes.";
                }
                else // Administrador o Gerente
                {
                    decimal recaudacionDia = todasLasVentas
                        .Where(v => v.Fecha_Hora.Date == hoy.Date)
                        .Sum(v => v.Total);

                    int ventasTotalesMes = todasLasVentas.Count;

                    int alertasStock = _reportesService.ObtenerCantidadProductosBajoStock(5);

                    Kpi1Titulo = "RECAUDACIÓN DEL DÍA";
                    Kpi1Valor = recaudacionDia.ToString("C0");

                    Kpi2Titulo = "VENTAS DEL MES";
                    Kpi2Valor = $"{ventasTotalesMes} Pares";

                    Kpi3Titulo = "ALERTAS DE STOCK";
                    Kpi3Valor = $"{alertasStock} Modelos";

                    TituloTablaVentas = "ÚLTIMAS VENTAS";
                    VisibilidadObjetivos = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al conectar con la base de datos para cargar el inicio: " + ex.Message,
                                "Error de Conexión", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AbrirDetallesVenta(object parametro)
        {
            // El parámetro es el objeto de la fila seleccionada en el DataGrid
            if (parametro != null)
            {
                // Instanciamos la nueva vista pasándole la venta completa
                var ventanaDetalle = new DetalleVentaWindow(parametro);

                // ShowDialog() abre la ventana de forma modal (bloquea el fondo hasta que se cierre)
                ventanaDetalle.ShowDialog();
            }
        }
    }
}