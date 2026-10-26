using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using LiveCharts;
using LiveCharts.Wpf;
using TillasDesktop.BLL.Services;
using TillasDesktop.Entities.Reportes;

namespace TillasDesktop.UI.Modelos
{
    public class ReportesViewModel : ViewModelBase
    {
        private readonly ReportesService _reportesService;

        private readonly ReportesPdfService _pdfService; // Servicio de PDFs integrado


        private bool _inicializando = true;

        public SeriesCollection GraficoVentas { get; set; }
        public List<string> EtiquetasVentas { get; set; }
        public SeriesCollection GraficoVendedores { get; set; }
        public List<string> EtiquetasVendedores { get; set; }
        public SeriesCollection GraficoProductos { get; set; }
        public Func<double, string> FormatoMoneda { get; set; }

        public List<string> OpcionesPeriodo { get; set; }
        public List<string> OpcionesTipoReporte { get; set; }
        public List<string> OpcionesMarcas { get; set; }
        public List<string> OpcionesCategorias { get; set; }

        // Delegado para solicitar la captura de pantalla del gráfico desde la Vista
        public Func<byte[]> SolicitarCapturaGrafico { get; set; }

        // PROPIEDADES REACTIVAS
        private string _tipoReporteSeleccionado;
        public string TipoReporteSeleccionado
        {
            get => _tipoReporteSeleccionado;
            set { _tipoReporteSeleccionado = value; OnPropertyChanged(); ActualizarGraficos(); }
        }

        private string _periodoSeleccionado;
        public string PeriodoSeleccionado
        {
            get => _periodoSeleccionado;
            set { _periodoSeleccionado = value; OnPropertyChanged(); ActualizarFechasPorPeriodo(); }
        }

        private DateTime _fechaInicio;
        public DateTime FechaInicio
        {
            get => _fechaInicio;
            set { _fechaInicio = value; OnPropertyChanged(); ActualizarGraficos(); }
        }

        private DateTime _fechaFin;
        public DateTime FechaFin
        {
            get => _fechaFin;
            set { _fechaFin = value; OnPropertyChanged(); ActualizarGraficos(); }
        }

        private string _marcaSeleccionada;
        public string MarcaSeleccionada
        {
            get => _marcaSeleccionada;
            set { _marcaSeleccionada = value; OnPropertyChanged(); ActualizarGraficos(); }
        }

        private string _categoriaSeleccionada;
        public string CategoriaSeleccionada
        {
            get => _categoriaSeleccionada;
            set { _categoriaSeleccionada = value; OnPropertyChanged(); ActualizarGraficos(); }
        }

        // Propiedades para las tarjetas (KPIs)
        private string _kpiTituloIngresos;
        public string KpiTituloIngresos { get => _kpiTituloIngresos; set { _kpiTituloIngresos = value; OnPropertyChanged(); } }

        private string _kpiIngresos;
        public string KpiIngresos { get => _kpiIngresos; set { _kpiIngresos = value; OnPropertyChanged(); } }

        private string _kpiVentas;
        public string KpiVentas { get => _kpiVentas; set { _kpiVentas = value; OnPropertyChanged(); } }

        private string _kpiProductoEstrellaNombre;
        public string KpiProductoEstrellaNombre { get => _kpiProductoEstrellaNombre; set { _kpiProductoEstrellaNombre = value; OnPropertyChanged(); } }

        private string _kpiProductoEstrellaDetalle;
        public string KpiProductoEstrellaDetalle { get => _kpiProductoEstrellaDetalle; set { _kpiProductoEstrellaDetalle = value; OnPropertyChanged(); } }

        // COMANDOS
        public ICommand GenerarReportePdfCommand { get; }
        public ICommand VerDetalleCommand { get; }

        public ReportesViewModel()
        {
            _reportesService = new ReportesService();

            _pdfService = new ReportesPdfService(); // Inicialización del servicio PDF

            GraficoVentas = new SeriesCollection();
            EtiquetasVentas = new List<string>();
            GraficoVendedores = new SeriesCollection();
            EtiquetasVendedores = new List<string>();
            GraficoProductos = new SeriesCollection();
            FormatoMoneda = valor => valor.ToString("C");

            OpcionesPeriodo = new List<string> { "Último Trimestre", "Este Mes", "Esta Semana" };
            OpcionesTipoReporte = new List<string> { "Reporte de Ventas", "Rendimiento de Vendedores", "Productos Más Vendidos" };

            CargarFiltrosDesdeBD();

            _tipoReporteSeleccionado = "Reporte de Ventas";
            _marcaSeleccionada = "Todas";
            _categoriaSeleccionada = "Todas";

            PeriodoSeleccionado = "Este Mes";

            GenerarReportePdfCommand = new RelayCommand(GenerarPdf);

            _inicializando = false;
            ActualizarGraficos();
        }

        // MÉTODOS DE ACTUALIZACIÓN
        private void ActualizarFechasPorPeriodo()
        {
            DateTime hoy = DateTime.Now;
            switch (PeriodoSeleccionado)
            {
                case "Esta Semana":
                    int diasDesdeLunes = (7 + (hoy.DayOfWeek - DayOfWeek.Monday)) % 7;
                    FechaInicio = hoy.AddDays(-diasDesdeLunes).Date;
                    FechaFin = hoy.Date;
                    break;
                case "Este Mes":
                    FechaInicio = new DateTime(hoy.Year, hoy.Month, 1);
                    FechaFin = hoy.Date;
                    break;
                case "Último Trimestre":
                    FechaInicio = hoy.AddMonths(-3).Date;
                    FechaFin = hoy.Date;
                    break;
            }
        }

        private void ActualizarGraficos()
        {
            if (_inicializando || FechaInicio > FechaFin) return;

            ActualizarKpis();

            GraficoVentas.Clear(); EtiquetasVentas.Clear();
            GraficoVendedores.Clear(); EtiquetasVendedores.Clear();
            GraficoProductos.Clear();

            try
            {
                switch (TipoReporteSeleccionado)
                {
                    case "Reporte de Ventas":
                        var ventas = _reportesService.GenerarReporteVentas(FechaInicio, FechaFin);
                        var ventasPorDia = ventas.GroupBy(v => v.Fecha_Hora.Date).OrderBy(g => g.Key).ToList();
                        var valoresVentas = new ChartValues<decimal>();

                        foreach (var grupo in ventasPorDia)
                        {
                            valoresVentas.Add(grupo.Sum(v => v.Total));
                            EtiquetasVentas.Add(grupo.Key.ToString("dd/MMM"));
                        }

                        GraficoVentas.Add(new LineSeries { Title = "Recaudación Diaria", Values = valoresVentas, PointGeometrySize = 15 });
                        break;

                    case "Rendimiento de Vendedores":
                        var vendedores = _reportesService.GenerarReporteVendedores(FechaInicio, FechaFin);
                        var valoresVendedores = new ChartValues<decimal>();

                        foreach (var v in vendedores)
                        {
                            valoresVendedores.Add(v.TotalRecaudado);
                            EtiquetasVendedores.Add(v.Vendedor);
                        }

                        GraficoVendedores.Add(new ColumnSeries { Title = "Total Vendido", Values = valoresVendedores, DataLabels = true });
                        break;

                    case "Productos Más Vendidos":
                        var productos = _reportesService.GenerarReporteProductos(FechaInicio, FechaFin, MarcaSeleccionada, CategoriaSeleccionada);
                        var topProductos = productos.Take(5).ToList();

                        foreach (var p in topProductos)
                        {
                            GraficoProductos.Add(new PieSeries { Title = p.Nombre, Values = new ChartValues<decimal> { p.TotalGenerado }, DataLabels = true });
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al actualizar gráficos: " + ex.Message, "Error de DB", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // GENERACIÓN DE PDF
        private void GenerarPdf(object parametro)
        {
            if (FechaInicio > FechaFin)
            {
                MessageBox.Show("Rango de fechas inválido.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Obtenemos la imagen del gráfico actual en formato byte[]
                byte[] graficoBytes = SolicitarCapturaGrafico?.Invoke();

                switch (TipoReporteSeleccionado)
                {
                    case "Reporte de Ventas":
                        var ventas = _reportesService.GenerarReporteVentas(FechaInicio, FechaFin);
                        _pdfService.GenerarReporteVentasPdf(FechaInicio, FechaFin, ventas, graficoBytes); // <--- Agregado graficoBytes
                        break;

                    case "Rendimiento de Vendedores":
                        var vendedores = _reportesService.GenerarReporteVendedores(FechaInicio, FechaFin);
                        _pdfService.GenerarReporteVendedoresPdf(FechaInicio, FechaFin, vendedores, graficoBytes); // <--- Agregado graficoBytes
                        break;

                    case "Productos Más Vendidos":
                        var productos = _reportesService.GenerarReporteProductos(FechaInicio, FechaFin, MarcaSeleccionada, CategoriaSeleccionada);
                        _pdfService.GenerarReporteProductosPdf(FechaInicio, FechaFin, MarcaSeleccionada, CategoriaSeleccionada, productos, graficoBytes); // <--- Agregado graficoBytes
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al generar el documento PDF: " + ex.Message, "Error de Exportación", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CargarFiltrosDesdeBD()
        {
            try
            {
                var marcasDB = _reportesService.ObtenerMarcasParaFiltro();
                marcasDB.Insert(0, "Todas");
                OpcionesMarcas = marcasDB;

                var categoriasDB = _reportesService.ObtenerCategoriasParaFiltro();
                categoriasDB.Insert(0, "Todas");
                OpcionesCategorias = categoriasDB;
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudieron cargar los filtros de Marcas y Categorías.\n\nDetalle: " + ex.Message, "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);

                OpcionesMarcas = new List<string> { "Todas" };
                OpcionesCategorias = new List<string> { "Todas" };
            }
        }

        private void ActualizarKpis()
        {
            if (_inicializando || FechaInicio > FechaFin) return;

            try
            {
                var ventasPeriodo = _reportesService.GenerarReporteVentas(FechaInicio, FechaFin);

                KpiTituloIngresos = $"INGRESOS TOTALES ({PeriodoSeleccionado.ToUpper()})";
                KpiIngresos = ventasPeriodo.Sum(v => v.Total).ToString("C");
                KpiVentas = ventasPeriodo.Count.ToString();

                var productosVendidos = _reportesService.GenerarReporteProductos(FechaInicio, FechaFin, "Todas", "Todas");
                var productoEstrella = productosVendidos.FirstOrDefault();

                if (productoEstrella != null)
                {
                    KpiProductoEstrellaNombre = productoEstrella.Nombre;
                    KpiProductoEstrellaDetalle = $"{productoEstrella.CantidadVendida} pares vendidos";
                }
                else
                {
                    KpiProductoEstrellaNombre = "Sin movimientos";
                    KpiProductoEstrellaDetalle = "0 pares vendidos";
                }
            }
            catch (Exception)
            {
                KpiIngresos = "$0,00";
                KpiVentas = "0";
                KpiProductoEstrellaNombre = "Error de conexión";
            }
        }
    }
}