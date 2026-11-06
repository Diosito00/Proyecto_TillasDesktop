// Importa el espacio de nombres fundamental para tipos base y manejo general de excepciones
using System;
// Importa colecciones genéricas para almacenar listas de elementos (como opciones de filtros)
using System.Collections.Generic;
// Importa las colecciones observables (aunque en este archivo no se usen directamente, suele heredarse por compatibilidad con MVVM)
using System.Collections.ObjectModel;
// Importa extensiones de LINQ para realizar consultas, agrupamientos y filtros sobre listas en memoria
using System.Linq;
// Importa clases base de WPF para la gestión de la aplicación y mensajes emergentes
using System.Windows;
// Importa las utilidades de comandos y eventos de entrada de la interfaz de usuario en WPF
using System.Windows.Input;
// Importa la gestión de configuraciones regionales y formatos de cultura (ej. moneda argentina)
using System.Globalization;
// Importa los espacios de nombres principales y de WPF de la librería de gráficos LiveCharts
using LiveCharts;
using LiveCharts.Wpf;
// Importa los servicios de la Capa de Negocios encargados de procesar la lógica de reportes
using TillasDesktop.BLL.Services;
// Importa las entidades y objetos de transferencia de datos de reportes
using TillasDesktop.Entities.Reportes;

// Define el espacio de nombres (namespace) correspondiente a los modelos de vista (ViewModels) de la interfaz de usuario del sistema
namespace TillasDesktop.UI.Modelos
{
    // Declara la clase pública ReportesViewModel que hereda de ViewModelBase, encargada de administrar los gráficos, filtros, KPIs y la exportación de reportes a PDF
    public class ReportesViewModel : ViewModelBase
    {
        // Instancia de solo lectura del servicio de negocio para consultar datos de reportes desde la base de datos
        private readonly ReportesService _reportesService;

        // Instancia de solo lectura del servicio especializado para generar documentos PDF integrados
        private readonly ReportesPdfService _pdfService; // Servicio de PDFs integrado

        // Bandera booleana interna para evitar la ejecución innecesaria de eventos de actualización durante el proceso de arranque e inicialización
        private bool _inicializando = true;

        // Colección de series para el gráfico de líneas de ventas temporales
        public SeriesCollection GraficoVentas { get; set; }
        // Lista de etiquetas (eje X) correspondientes a las fechas de las ventas
        public List<string> EtiquetasVentas { get; set; }
        // Colección de series para el gráfico de barras del rendimiento de los vendedores
        public SeriesCollection GraficoVendedores { get; set; }
        // Lista de etiquetas (eje X) correspondientes a los nombres de los vendedores
        public List<string> EtiquetasVendedores { get; set; }
        // Colección de series para el gráfico circular (Pie) de productos más vendidos
        public SeriesCollection GraficoProductos { get; set; }
        // Función delegada para formatear los valores numéricos como moneda local (ej. pesos argentinos)
        public Func<double, string> FormatoMoneda { get; set; }

        // Listas desplegables de opciones para filtros en la interfaz
        public List<string> OpcionesPeriodo { get; set; }
        public List<string> OpcionesTipoReporte { get; set; }
        public List<string> OpcionesMarcas { get; set; }
        public List<string> OpcionesCategorias { get; set; }

        // Delegado funcional que permite solicitar a la Vista (Code-behind) una captura de pantalla del gráfico actual en formato de bytes (byte[]) para incrustarla en el PDF
        public Func<byte[]> SolicitarCapturaGrafico { get; set; }

        // PROPIEDADES REACTIVAS Y CAMPOS PRIVADOS DE ESTADO

        // Campo privado que almacena el tipo de reporte seleccionado actualmente
        private string _tipoReporteSeleccionado;
        // Propiedad pública que al cambiar dispara la notificación de cambio y actualiza los gráficos correspondientes
        public string TipoReporteSeleccionado
        {
            get => _tipoReporteSeleccionado;
            set { _tipoReporteSeleccionado = value; OnPropertyChanged(); ActualizarGraficos(); }
        }

        // Campo privado que almacena el período temporal predefinido seleccionado
        private string _periodoSeleccionado;
        // Propiedad pública que al cambiar recalcula automáticamente el rango de fechas de inicio y fin
        public string PeriodoSeleccionado
        {
            get => _periodoSeleccionado;
            set { _periodoSeleccionado = value; OnPropertyChanged(); ActualizarFechasPorPeriodo(); }
        }

        // Campo privado para la fecha de inicio del filtro de reportes
        private DateTime _fechaInicio;
        // Propiedad pública que al modificarse actualiza los gráficos y KPIs
        public DateTime FechaInicio
        {
            get => _fechaInicio;
            set { _fechaInicio = value; OnPropertyChanged(); ActualizarGraficos(); }
        }

        // Campo privado para la fecha de fin del filtro de reportes
        private DateTime _fechaFin;
        // Propiedad pública que al modificarse actualiza los gráficos y KPIs
        public DateTime FechaFin
        {
            get => _fechaFin;
            set { _fechaFin = value; OnPropertyChanged(); ActualizarGraficos(); }
        }

        // Campo privado para la marca seleccionada en el filtro de productos
        private string _marcaSeleccionada;
        // Propiedad pública que actualiza los gráficos al cambiar de marca
        public string MarcaSeleccionada
        {
            get => _marcaSeleccionada;
            set { _marcaSeleccionada = value; OnPropertyChanged(); ActualizarGraficos(); }
        }

        // Campo privado para la categoría seleccionada en el filtro de productos
        private string _categoriaSeleccionada;
        // Propiedad pública que actualiza los gráficos al cambiar de categoría
        public string CategoriaSeleccionada
        {
            get => _categoriaSeleccionada;
            set { _categoriaSeleccionada = value; OnPropertyChanged(); ActualizarGraficos(); }
        }

        // PROPIEDADES PARA LAS TARJETAS DE INDICADORES CLAVE (KPIs)

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

        // COMANDOS PÚBLICOS
        public ICommand GenerarReportePdfCommand { get; }
        public ICommand VerDetalleCommand { get; } // Declarado para posibles expansiones de interfaz

        // Constructor principal que inicializa servicios, colecciones, valores por defecto y dibuja el reporte inicial
        public ReportesViewModel()
        {
            // Instancia el servicio de reportes de la capa de negocios
            _reportesService = new ReportesService();
            // Instancia el servicio de generación de PDFs
            _pdfService = new ReportesPdfService();

            // Inicializa las colecciones de series y etiquetas para LiveCharts
            GraficoVentas = new SeriesCollection();
            EtiquetasVentas = new List<string>();
            GraficoVendedores = new SeriesCollection();
            EtiquetasVendedores = new List<string>();
            GraficoProductos = new SeriesCollection();
            // Configura el formateador de moneda para utilizar la cultura de Argentina (es-AR)
            FormatoMoneda = valor => valor.ToString("C", new CultureInfo("es-AR"));

            // Define las opciones fijas para los filtros desplegables de período y tipo de reporte
            OpcionesPeriodo = new List<string> { "Último Trimestre", "Este Mes", "Esta Semana" };
            OpcionesTipoReporte = new List<string> { "Reporte de Ventas", "Rendimiento de Vendedores", "Productos Más Vendidos" };

            // Carga dinámicamente las marcas y categorías desde la base de datos
            CargarFiltrosDesdeBD();

            // Configura los valores iniciales predeterminados
            _tipoReporteSeleccionado = "Reporte de Ventas";
            _marcaSeleccionada = "Todas";
            _categoriaSeleccionada = "Todas";

            // Asigna el período inicial, lo cual activará en cascada el cálculo de fechas
            PeriodoSeleccionado = "Este Mes";

            // Asocia el comando de generación de PDF con su método delegado
            GenerarReportePdfCommand = new RelayCommand(GenerarPdf);

            // Finaliza la etapa de inicialización y habilita la reactividad completa
            _inicializando = false;
            ActualizarGraficos();
        }

        // MÉTODOS AUXILIARES DE ACTUALIZACIÓN DE FECHAS SEGÚN EL PERÍODO
        private void ActualizarFechasPorPeriodo()
        {
            DateTime hoy = DateTime.Now;
            switch (PeriodoSeleccionado)
            {
                case "Esta Semana":
                    // Calcula el desplazamiento en días hasta el lunes anterior para definir el inicio de semana
                    int diasDesdeLunes = (7 + (hoy.DayOfWeek - DayOfWeek.Monday)) % 7;
                    FechaInicio = hoy.AddDays(-diasDesdeLunes).Date;
                    FechaFin = hoy.Date;
                    break;
                case "Este Mes":
                    // Define el rango desde el primer día del mes actual hasta hoy
                    FechaInicio = new DateTime(hoy.Year, hoy.Month, 1);
                    FechaFin = hoy.Date;
                    break;
                case "Último Trimestre":
                    // Define el rango abarcando los últimos 3 meses hasta la fecha actual
                    FechaInicio = hoy.AddMonths(-3).Date;
                    FechaFin = hoy.Date;
                    break;
            }
        }

        // Método central que procesa y actualiza los gráficos estadísticos según los filtros activos
        private void ActualizarGraficos()
        {
            // Sale inmediatamente si el ViewModel se está inicializando o si el rango de fechas es inválido
            if (_inicializando || FechaInicio > FechaFin) return;

            // Actualiza primero los indicadores clave (KPIs) superiores
            ActualizarKpis();

            // Limpia las colecciones de todos los gráficos antes de volver a poblarlas
            GraficoVentas.Clear(); EtiquetasVentas.Clear();
            GraficoVendedores.Clear(); EtiquetasVendedores.Clear();
            GraficoProductos.Clear();

            try
            {
                // Evalúa qué tipo de reporte está seleccionado para poblar la vista correspondiente
                switch (TipoReporteSeleccionado)
                {
                    case "Reporte de Ventas":
                        // Obtiene los datos de ventas en el rango de fechas
                        var ventas = _reportesService.GenerarReporteVentas(FechaInicio, FechaFin);
                        // Agrupa las ventas por día y ordena cronológicamente
                        var ventasPorDia = ventas.GroupBy(v => v.Fecha_Hora.Date).OrderBy(g => g.Key).ToList();
                        var valoresVentas = new ChartValues<decimal>();

                        foreach (var grupo in ventasPorDia)
                        {
                            valoresVentas.Add(grupo.Sum(v => v.Total));
                            EtiquetasVentas.Add(grupo.Key.ToString("dd/MMM/yy"));
                        }

                        // Agrega la serie de líneas al gráfico de ventas
                        GraficoVentas.Add(new LineSeries { Title = "Recaudación Diaria", Values = valoresVentas, PointGeometrySize = 15 });
                        break;

                    case "Rendimiento de Vendedores":
                        // Obtiene el rendimiento de los operadores en el período
                        var vendedores = _reportesService.GenerarReporteVendedores(FechaInicio, FechaFin);
                        var valoresVendedores = new ChartValues<decimal>();

                        foreach (var v in vendedores)
                        {
                            valoresVendedores.Add(v.TotalRecaudado);
                            EtiquetasVendedores.Add(v.Vendedor);
                        }

                        // Agrega la serie de columnas (barras) al gráfico de vendedores
                        GraficoVendedores.Add(new ColumnSeries { Title = "Total Vendido", Values = valoresVendedores, DataLabels = true });
                        break;

                    case "Productos Más Vendidos":
                        // Obtiene los productos más vendidos aplicando filtros de marca y categoría
                        var productos = _reportesService.GenerarReporteProductos(FechaInicio, FechaFin, MarcaSeleccionada, CategoriaSeleccionada);
                        // Toma los primeros 5 productos (Top 5) para mostrar en el gráfico circular
                        var topProductos = productos.Take(5).ToList();
                        var formatoAr = new CultureInfo("es-AR");

                        foreach (var p in topProductos)
                        {
                            // Agrega cada sector (rebanada) al gráfico circular (PieSeries) con formato detallado
                            GraficoProductos.Add(new PieSeries
                            {
                                Title = $"{p.Nombre} ({p.CantidadVendida} pares - ${p.TotalGenerado.ToString("N2", formatoAr)})",
                                Values = new ChartValues<decimal> { p.TotalGenerado },
                                DataLabels = true,
                                LabelPoint = chartPoint => chartPoint.Y.ToString("C", formatoAr)
                            });
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                // Muestra un mensaje de error en caso de fallas con la base de datos durante el dibujado
                MessageBox.Show("Error al actualizar gráficos: " + ex.Message, "Error de DB", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // LÓGICA DE GENERACIÓN Y EXPORTACIÓN A PDF
        private void GenerarPdf(object parametro)
        {
            // Valida la coherencia del rango de fechas antes de exportar
            if (FechaInicio > FechaFin)
            {
                MessageBox.Show("Rango de fechas inválido.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Muestra un cuadro de diálogo de confirmación detallado al usuario
            var resultado = MessageBox.Show(
                $"¿Desea generar el documento PDF correspondiente al reporte de '{TipoReporteSeleccionado}'?\n\nPeríodo: {FechaInicio:dd/MM/yyyy} al {FechaFin:dd/MM/yyyy}",
                "Confirmar Generación de PDF",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (resultado != MessageBoxResult.Yes)
            {
                return; // Cancela la operación si el usuario responde "No"
            }

            try
            {
                // Invoca al delegado de la Vista para obtener la captura de pantalla actual del gráfico en formato byte[]
                byte[] graficoBytes = SolicitarCapturaGrafico?.Invoke();

                // Delega la creación del documento PDF al servicio correspondiente según el tipo de reporte activo
                switch (TipoReporteSeleccionado)
                {
                    case "Reporte de Ventas":
                        var ventas = _reportesService.GenerarReporteVentas(FechaInicio, FechaFin);
                        _pdfService.GenerarReporteVentasPdf(FechaInicio, FechaFin, ventas, graficoBytes);
                        break;

                    case "Rendimiento de Vendedores":
                        var vendedores = _reportesService.GenerarReporteVendedores(FechaInicio, FechaFin);
                        _pdfService.GenerarReporteVendedoresPdf(FechaInicio, FechaFin, vendedores, graficoBytes);
                        break;

                    case "Productos Más Vendidos":
                        var productos = _reportesService.GenerarReporteProductos(FechaInicio, FechaFin, MarcaSeleccionada, CategoriaSeleccionada);
                        _pdfService.GenerarReporteProductosPdf(FechaInicio, FechaFin, MarcaSeleccionada, CategoriaSeleccionada, productos, graficoBytes);
                        break;
                }
            }
            catch (Exception ex)
            {
                // Atrapa y muestra cualquier error surgido durante la generación o guardado del archivo PDF
                MessageBox.Show("Error al generar el documento PDF: " + ex.Message, "Error de Exportación", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // CARGA DE FILTROS DESDE LA BASE DE DATOS
        private void CargarFiltrosDesdeBD()
        {
            try
            {
                // Obtiene las marcas registradas e inserta "Todas" en la primera posición como opción general
                var marcasDB = _reportesService.ObtenerMarcasParaFiltro();
                marcasDB.Insert(0, "Todas");
                OpcionesMarcas = marcasDB;

                // Obtiene las categorías registradas e inserta "Todas" en la primera posición
                var categoriasDB = _reportesService.ObtenerCategoriasParaFiltro();
                categoriasDB.Insert(0, "Todas");
                OpcionesCategorias = categoriasDB;
            }
            catch (Exception ex)
            {
                // Si falla la conexión, advierte al operador y establece valores por defecto seguros
                MessageBox.Show("No se pudieron cargar los filtros de Marcas y Categorías.\n\nDetalle: " + ex.Message, "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);

                OpcionesMarcas = new List<string> { "Todas" };
                OpcionesCategorias = new List<string> { "Todas" };
            }
        }

        // CÁLCULO Y ACTUALIZACIÓN DE TARJETAS DE INDICADORES (KPIs)
        private void ActualizarKpis()
        {
            if (_inicializando || FechaInicio > FechaFin) return;

            try
            {
                // Obtiene las ventas del período para calcular los totales monetarios y volumétricos
                var ventasPeriodo = _reportesService.GenerarReporteVentas(FechaInicio, FechaFin);

                KpiTituloIngresos = $"INGRESOS TOTALES ({PeriodoSeleccionado.ToUpper()})";
                KpiIngresos = ventasPeriodo.Sum(v => v.Total).ToString("C");
                KpiVentas = ventasPeriodo.Count.ToString();

                // Consulta el producto estrella o más vendido del período actual
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
                // Establece valores seguros por defecto en caso de error en los cálculos de KPIs
                KpiIngresos = "$0,00";
                KpiVentas = "0";
                KpiProductoEstrellaNombre = "Error de conexión";
            }
        }
    }
}