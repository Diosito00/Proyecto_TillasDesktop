using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using TillasDesktop.Entities.Reportes;

namespace TillasDesktop.UI.Modelos
{
    // Hereda de ViewModelBase para mantener la arquitectura MVVM. 
    // Esto permite que cualquier cambio en las propiedades notifique automáticamente a la vista (XAML).
    public class InicioViewModel : ViewModelBase
    {
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
        public Visibility VisibilidadGrafico { get; set; }
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

        public InicioViewModel()
        {
            // CONTEXTO DE SESIÓN
            // Operadores ternarios (? :) Si la variable global viene vacía, le asigna "Admin" por defecto para que la interfaz no se rompa mostrando textos en blanco.
            string nombre = string.IsNullOrWhiteSpace(Proyecto_TillasDesktop.App.NombreUsuarioActual) ? "Admin" : Proyecto_TillasDesktop.App.NombreUsuarioActual;
            string rol = string.IsNullOrWhiteSpace(Proyecto_TillasDesktop.App.RolUsuarioActual) ? "Admin" : Proyecto_TillasDesktop.App.RolUsuarioActual;

            SaludoUsuario = $"¡Hola de nuevo, {nombre}!";
            RolUsuario = rol;

            // Formateamos la fecha. Ej: "21 septiembre 2026 | 22:30"
            FechaActual = DateTime.Now.ToString("dd MMMM yyyy | HH:mm");

            // CONFIGURACIÓN ESTRUCTURAL
            // Delegamos la lógica de qué tarjetas mostrar a un método privado.
            ConfigurarKpisPorRol();

            // DATOS DEL GRÁFICO (LiveCharts - Mockeado por ahora)
            SeriesCollectionVentas = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Recaudación",
                    Values = new ChartValues<double> { 82000, 213000, 445000, 123000, 555000, 325000, 410000 },
                    PointGeometrySize = 12, // Tamaño de los "puntos" en las intersecciones
                    LineSmoothness = 0.6    // Curva las líneas rígidas para un diseño más moderno y fluido
                }
            };
            LabelsDias = new[] { "Lun", "Mar", "Mié", "Jue", "Vie", "Sáb", "Dom" };
            FormatterMoneda = value => value.ToString("C0"); // "C0" = Moneda (Currency) con cero (0) decimales.

            // DATOS DE LA TABLA (Mockeado)
            // Llenamos la colección asegurando que los tipos de datos (DateTime, decimal) sean correctos.
            UltimasVentasList = new ObservableCollection<VentaResumenDTO>
            {
                new VentaResumenDTO {
                    ID = 1045,
                    Fecha_Hora = DateTime.Now.AddMinutes(-15),
                    NombreCliente = "Consumidor Final",
                    MetodoPago = "Efectivo",
                    Total = 125000m, // La 'm' indica explícitamente a C# que es un valor decimal, no un entero
                    NombreVendedor = nombre // Usamos la variable local generada arriba
                },
                new VentaResumenDTO {
                    ID = 1044,
                    Fecha_Hora = DateTime.Now.AddHours(-2),
                    NombreCliente = "Juan Pérez",
                    MetodoPago = "Tarjeta Débito",
                    Total = 235000m,
                    NombreVendedor = nombre
                }
            };
        }

        // MÉTODOS DE NEGOCIO Y LÓGICA VISUAL
        private void ConfigurarKpisPorRol()
        {
            // Convertimos a minúsculas (.ToLower()) para evitar errores si en la base de datos dice "VENDEDOR" o "vendedor"
            if (RolUsuario.ToLower() == "vendedor")
            {
                // Tarjetas enfocadas en rendimiento individual
                Kpi1Titulo = "VENTAS DE HOY";
                Kpi1Valor = "12 Pares";
                Kpi2Titulo = "TICKET PROMEDIO";
                Kpi2Valor = "$ 115.000";
                Kpi3Titulo = "TUS COMISIONES";
                Kpi3Valor = "$ 45.000";

                // Personalizamos el título del DataGrid
                TituloTablaVentas = "MIS ÚLTIMAS VENTAS";

                // Ocultamos el bloque del gráfico
                VisibilidadGrafico = Visibility.Collapsed;

                // Habilitamos el bloque de la barra de progreso
                VisibilidadObjetivos = Visibility.Visible;

                // Datos simulados para la barra de progreso
                ProgresoObjetivo = 75;
                TextoObjetivo = "¡Excelente ritmo! Has vendido 30 de 40 pares para alcanzar tu bono mensual.";
            }
            else // Si es Gerente o Admin
            {
                // Tarjetas enfocadas en el estado global del negocio
                Kpi1Titulo = "RECAUDACIÓN DEL DÍA";
                Kpi1Valor = "$ 850.000";
                Kpi2Titulo = "PRODUCTOS VENDIDOS";
                Kpi2Valor = "38 Pares";
                Kpi3Titulo = "ALERTAS DE STOCK";
                Kpi3Valor = "5 Modelos";

                // Personalizamos el título para que sea de alcance general
                TituloTablaVentas = "ÚLTIMAS VENTAS";

                // Mostramos el gráfico de dinero para los administradores
                VisibilidadGrafico = Visibility.Visible;

                // Ocultamos la barra de progreso de ventas individuales
                VisibilidadObjetivos = Visibility.Collapsed;
            }
        }
    }
}