using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using TillasDesktop.Entities.Reportes;

namespace TillasDesktop.UI.Modelos
{
    public class InicioViewModel : ViewModelBase
    {
        // === CABECERA Y CONTEXTO ===
        public string SaludoUsuario { get; set; }
        public string RolUsuario { get; set; }
        public string FechaActual { get; set; }

        // === KPIs DINÁMICOS ===
        public string Kpi1Titulo { get; set; }
        public string Kpi1Valor { get; set; }
        public string Kpi2Titulo { get; set; }
        public string Kpi2Valor { get; set; }
        public string Kpi3Titulo { get; set; }
        public string Kpi3Valor { get; set; }

        // === VISIBILIDAD DE BOTONES ===
        public Visibility VisibilidadBotonCaja { get; set; }
        public Visibility VisibilidadBotonNuevo { get; set; }

        // === GRÁFICO Y TABLA ===
        public SeriesCollection SeriesCollectionVentas { get; set; }
        public string[] LabelsDias { get; set; }
        public Func<double, string> FormatterMoneda { get; set; }
        public ObservableCollection<VentaResumenDTO> UltimasVentasList { get; set; }

        public InicioViewModel()
        {
            string nombre = string.IsNullOrWhiteSpace(Proyecto_TillasDesktop.App.NombreUsuarioActual) ? "Admin" : Proyecto_TillasDesktop.App.NombreUsuarioActual;
            string rol = string.IsNullOrWhiteSpace(Proyecto_TillasDesktop.App.RolUsuarioActual) ? "Admin" : Proyecto_TillasDesktop.App.RolUsuarioActual;
            SaludoUsuario = $"¡Hola de nuevo, {nombre}!";

            // TODO: Conectar con el rol real del usuario logueado
            RolUsuario = rol;
            FechaActual = DateTime.Now.ToString("dd MMMM yyyy | HH:mm");

            ConfigurarKpisPorRol();

            SeriesCollectionVentas = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Recaudación",
                    Values = new ChartValues<double> { 82000, 213000, 445000, 123000, 555000, 325000, 410000 },
                    PointGeometrySize = 12,
                    LineSmoothness = 0.6
                }
            };
            LabelsDias = new[] { "Lun", "Mar", "Mié", "Jue", "Vie", "Sáb", "Dom" };
            FormatterMoneda = value => value.ToString("C0");

            UltimasVentasList = new ObservableCollection<VentaResumenDTO>
            {
                new VentaResumenDTO {
                    ID = 1045,
                    Fecha_Hora = DateTime.Now.AddMinutes(-15),
                    NombreCliente = "Consumidor Final",
                    MetodoPago = "Efectivo",
                    Total = 125000m,
                    NombreVendedor = "Cajero_1"
                },
                new VentaResumenDTO {
                    ID = 1044,
                    Fecha_Hora = DateTime.Now.AddHours(-2),
                    NombreCliente = "Juan Pérez",
                    MetodoPago = "Tarjeta Débito",
                    Total = 235000m,
                    NombreVendedor = "Cajero_1"
                }
            };
        }

        private void ConfigurarKpisPorRol()
        {
            if (RolUsuario.ToLower() == "vendedor")
            {
                Kpi1Titulo = "VENTAS DE HOY";
                Kpi1Valor = "12 Pares";
                Kpi2Titulo = "TICKET PROMEDIO";
                Kpi2Valor = "$ 115.000";
                Kpi3Titulo = "TUS COMISIONES";
                Kpi3Valor = "$ 45.000";

                // Vendedor ve la caja, pero no puede crear modelos nuevos
                VisibilidadBotonCaja = Visibility.Visible;
                VisibilidadBotonNuevo = Visibility.Collapsed;
            }
            else // Gerente o Admin
            {
                Kpi1Titulo = "RECAUDACIÓN DEL DÍA";
                Kpi1Valor = "$ 850.000";
                Kpi2Titulo = "PRODUCTOS VENDIDOS";
                Kpi2Valor = "38 Pares";
                Kpi3Titulo = "ALERTAS DE STOCK";
                Kpi3Valor = "5 Modelos";

                // Gerente/Admin pueden crear modelos, pero la caja se oculta
                VisibilidadBotonCaja = Visibility.Collapsed;
                VisibilidadBotonNuevo = Visibility.Visible;
            }
        }
    }
}