// Importa las librerías de LiveCharts para la gestión de colecciones de series y gráficos
using LiveCharts;
using LiveCharts.Wpf;
// Importa el espacio de nombres fundamental de .NET para tipos base y manejo de excepciones
using System;
// Importa colecciones genéricas para almacenar listas de elementos en memoria
using System.Collections.Generic;
// Importa colecciones observables de WPF para actualizar automáticamente las grillas (DataGrid) al añadir o quitar elementos
using System.Collections.ObjectModel;
// Importa extensiones de LINQ para realizar consultas, agrupamientos y filtros sobre listas en memoria
using System.Linq;
// Importa clases base de WPF para la gestión de la aplicación, estados de visibilidad y mensajes emergentes
using System.Windows;
// Importa las utilidades de comandos y eventos de entrada de la interfaz de usuario en WPF
using System.Windows.Input;
// Importa las vistas de la interfaz de usuario de la aplicación (como la ventana de detalles de venta)
using TillasDesktop.UI.Vistas;
// Importa los servicios de la Capa de Negocios encargados de procesar la lógica de reportes y consultas del dashboard
using TillasDesktop.BLL.Services;
// Importa las entidades y objetos de transferencia de datos de reportes (como VentaResumenDTO)
using TillasDesktop.Entities.Reportes;

// Define el espacio de nombres (namespace) correspondiente a los modelos de vista (ViewModels) de la interfaz de usuario del sistema
namespace TillasDesktop.UI.Modelos
{
    // Declara la clase pública InicioViewModel que hereda de ViewModelBase, encargada de administrar el Dashboard principal, KPIs personalizados por rol, gráficos y accesos rápidos.
    public class InicioViewModel : ViewModelBase
    {
        // Instancia de solo lectura del servicio de negocio para consultar las ventas y estadísticas generales desde la base de datos
        private readonly ReportesService _reportesService;

        // Propiedades enlazadas a la barra superior del Dashboard para mostrar datos personalizados del usuario activo.
        public string SaludoUsuario { get; set; }
        public string RolUsuario { get; set; }
        public string FechaActual { get; set; }

        // Propiedades públicas para las 3 tarjetas de Indicadores Clave (KPIs) superiores. 
        // Se separa explícitamente el Título del Valor para poder inyectar textos y métricas distintas dependiendo de quién inicie sesión (Vendedor vs Admin/Gerente).
        public string Kpi1Titulo { get; set; }
        public string Kpi1Valor { get; set; }
        public string Kpi2Titulo { get; set; }
        public string Kpi2Valor { get; set; }
        public string Kpi3Titulo { get; set; }
        public string Kpi3Valor { get; set; }

        // VISIBILIDAD Y TEXTOS DINÁMICOS
        // Controlan la estructura y aparición de elementos en la pantalla sin necesidad de tocar el Code-Behind.
        // Visibility es un enumerador de WPF (Visible, Collapsed, Hidden).      
        public Visibility VisibilidadObjetivos { get; set; }
        public string TituloTablaVentas { get; set; }

        // PANEL DE OBJETIVOS (Exclusivo para Vendedores)
        // Asigna la propiedad 'Value' numérica de la barra de progreso (ProgressBar) en el XAML.
        public double ProgresoObjetivo { get; set; }
        // Texto dinámico que acompaña a la barra de progreso detallando el avance mensual.
        public string TextoObjetivo { get; set; }

        // GRÁFICO Y TABLA DE ÚLTIMAS VENTAS
        // Colección que la librería LiveCharts necesita para dibujar las series, líneas y puntos de evolución.
        public SeriesCollection SeriesCollectionVentas { get; set; }
        // Textos y etiquetas que aparecerán en el eje horizontal (X) del gráfico (aunque se inicializa, se complementa con las consultas).
        public string[] LabelsDias { get; set; }
        // Función delegada que le enseña a LiveCharts cómo formatear los valores numéricos del eje vertical (Y) a formato de Moneda local ("C0").
        public Func<double, string> FormatterMoneda { get; set; }

        // ObservableCollection tipada con VentaResumenDTO: avisa al DataGrid (Tabla) de la interfaz si un nuevo registro se añade o elimina en tiempo real.
        public ObservableCollection<VentaResumenDTO> UltimasVentasList { get; set; }

        // Comando público vinculado para abrir la ventana de detalles al hacer clic en una venta del historial.
        public ICommand VerDetallesCommand { get; }

        // Constructor principal que inicializa servicios, contexto de sesión, colecciones y carga los datos del dashboard
        public InicioViewModel()
        {
            // Instancia el servicio de reportes de la capa de negocios
            _reportesService = new ReportesService();

            // CONTEXTO DE SESIÓN
            // Operadores ternarios (? :): Si la variable de sesión global viene vacía o nula, le asigna "Admin" por defecto para evitar romper la interfaz con textos en blanco.
            string nombre = string.IsNullOrWhiteSpace(Proyecto_TillasDesktop.App.NombreUsuarioActual) ? "Admin" : Proyecto_TillasDesktop.App.NombreUsuarioActual;
            string rol = string.IsNullOrWhiteSpace(Proyecto_TillasDesktop.App.RolUsuarioActual) ? "Admin" : Proyecto_TillasDesktop.App.RolUsuarioActual;

            // Asigna los textos formateados para el saludo superior y el rol del operador
            SaludoUsuario = $"¡Hola de nuevo, {nombre}!";
            RolUsuario = rol;

            // Formatea y asigna la fecha y hora actual en la barra superior (ej: "21 septiembre 2026 | 22:30")
            FechaActual = DateTime.Now.ToString("dd MMMM yyyy | HH:mm");

            // INICIALIZAR COLECCIONES Y FORMATOS VISUALES
            SeriesCollectionVentas = new SeriesCollection();
            UltimasVentasList = new ObservableCollection<VentaResumenDTO>();
            // Define la regla para formatear valores monetarios sin decimales en los gráficos
            FormatterMoneda = value => value.ToString("C0");

            // CARGAR DATOS REALES DESDE LA BASE DE DATOS filtrando según el usuario logueado
            CargarDatosDesdeBaseDeDatos(nombre);

            // Asocia el comando de visualización de detalles con su método delegado
            VerDetallesCommand = new RelayCommand(AbrirDetallesVenta);
        }

        // MÉTODO DE NEGOCIO Y LÓGICA VISUAL PARA CARGAR EL DASHBOARD
        private void CargarDatosDesdeBaseDeDatos(string usuarioActual)
        {
            try
            {
                // Definimos el rango temporal: desde el primer día del mes actual hasta la fecha y hora de hoy
                DateTime hoy = DateTime.Now;
                DateTime inicioMes = new DateTime(hoy.Year, hoy.Month, 1);

                // Obtenemos todas las ventas del período a través del servicio de reportes
                var todasLasVentas = _reportesService.GenerarReporteVentas(inicioMes, hoy);

                // CONFIGURACIÓN POLIMÓRFICA SEGÚN ROL (Vendedor vs Administrador/Gerente)
                if (RolUsuario.ToLower() == "vendedor")
                {
                    // Filtramos únicamente las ventas correspondientes al operador logueado actual
                    var ventasVendedor = todasLasVentas
                        .Where(v => !string.IsNullOrEmpty(v.NombreVendedor) &&
                                    v.NombreVendedor.Equals(usuarioActual, StringComparison.OrdinalIgnoreCase))
                        .ToList();

                    // Llenar el DataGrid SOLO con las últimas 5 ventas de ESTE vendedor
                    var ultimasVentasBD = ventasVendedor
                        .OrderByDescending(v => v.Fecha_Hora)
                        .Take(5)
                        .ToList();

                    UltimasVentasList.Clear();
                    foreach (var venta in ultimasVentasBD)
                    {
                        UltimasVentasList.Add(venta);
                    }

                    // Calcula la recaudación personal obtenida en el día de hoy
                    decimal recaudacionVendedorHoy = ventasVendedor
                        .Where(v => v.Fecha_Hora.Date == hoy.Date)
                        .Sum(v => v.Total);

                    // Conteo de operaciones realizadas por el vendedor en el día
                    int cantidadVentasHoy = ventasVendedor.Count(v => v.Fecha_Hora.Date == hoy.Date);
                    // Calcula el ticket promedio de venta del operador (evitando división por cero)
                    decimal ticketPromedio = cantidadVentasHoy > 0 ? recaudacionVendedorHoy / cantidadVentasHoy : 0;

                    // Configura los textos y valores orientados a métricas personales del vendedor
                    Kpi1Titulo = "VENTAS DE HOY";
                    Kpi1Valor = $"{cantidadVentasHoy} Operaciones";
                    Kpi2Titulo = "TICKET PROMEDIO";
                    Kpi2Valor = ticketPromedio.ToString("C0");
                    Kpi3Titulo = "RECAUDACIÓN HOY";
                    Kpi3Valor = recaudacionVendedorHoy.ToString("C0");

                    // Personaliza la interfaz para mostrar el título de sus propias ventas y habilitar el panel de objetivos
                    TituloTablaVentas = "MIS ÚLTIMAS VENTAS";
                    VisibilidadObjetivos = Visibility.Visible;

                    // Lógica para calcular la barra de progreso de objetivos (ejemplo basado en una meta corporativa de 40 pares mensuales)
                    int totalParesVendedorMes = ventasVendedor.Count;
                    ProgresoObjetivo = Math.Min((totalParesVendedorMes / 40.0) * 100, 100);
                    TextoObjetivo = $"¡Buen trabajo! Has registrado {totalParesVendedorMes} ventas este mes.";
                }
                else // Si el rol es Administrador o Gerente
                {
                    // Llenar el DataGrid con las últimas 5 ventas GLOBALES (de todos los vendedores)
                    var ultimasVentasGlobales = todasLasVentas
                        .OrderByDescending(v => v.Fecha_Hora)
                        .Take(5)
                        .ToList();

                    UltimasVentasList.Clear();
                    foreach (var venta in ultimasVentasGlobales)
                    {
                        UltimasVentasList.Add(venta);
                    }

                    // Calcula la recaudación global de la tienda en el día actual
                    decimal recaudacionDia = todasLasVentas
                        .Where(v => v.Fecha_Hora.Date == hoy.Date)
                        .Sum(v => v.Total);

                    // Conteo total de operaciones en el mes
                    int ventasTotalesMes = todasLasVentas.Count;

                    // Consulta al servicio las alertas críticas de stock bajo (ej. umbral menor a 5 unidades)
                    int alertasStock = _reportesService.ObtenerCantidadProductosBajoStock(5);

                    // Configura los KPIs gerenciales orientados al negocio global
                    Kpi1Titulo = "RECAUDACIÓN DEL DÍA";
                    Kpi1Valor = recaudacionDia.ToString("C0");

                    Kpi2Titulo = "VENTAS DEL MES";
                    Kpi2Valor = $"{ventasTotalesMes} Operaciones";

                    Kpi3Titulo = "ALERTAS DE STOCK";
                    Kpi3Valor = $"{alertasStock} Modelos";

                    // Configura los textos estándar y oculta el panel de objetivos personales de vendedor
                    TituloTablaVentas = "ÚLTIMAS VENTAS";
                    VisibilidadObjetivos = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                // Muestra un mensaje emergente de error si falla la comunicación con la base de datos
                MessageBox.Show("Error al conectar con la base de datos para cargar el inicio: " + ex.Message,
                                "Error de Conexión", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Método privado que maneja la apertura de la ventana de detalle al hacer clic en una venta de la grilla
        private void AbrirDetallesVenta(object parametro)
        {
            // Valida que el parámetro recibido desde la fila seleccionada en el DataGrid no sea nulo
            if (parametro != null)
            {
                // Instanciamos la nueva vista de detalle pasándole el objeto de la venta completa
                var ventanaDetalle = new DetalleVentaWindow(parametro);

                // ShowDialog() abre la ventana de forma modal y bloqueante (congela el fondo hasta que el usuario la cierre)
                ventanaDetalle.ShowDialog();
            }
        }
    }
}