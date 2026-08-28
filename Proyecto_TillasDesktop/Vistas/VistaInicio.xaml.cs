using LiveCharts;
using LiveCharts.Wpf;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TillasDesktop.UI.Vistas
{
    /// <summary>
    /// Lógica de interacción para VistaInicio.xaml
    /// </summary>
    public partial class VistaInicio : UserControl
    {
        public SeriesCollection SeriesCollection { get; set; }
        public string[] Labels { get; set; }
        public Func<double, string> Formatter { get; set; }
        public VistaInicio()
        {
            InitializeComponent();
            
            // 1. Cargar los datos de la curva
            SeriesCollection = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Ventas",
                    Values = new ChartValues<double> { 8200, 21300, 44500, 12300, 55500, 32500, 41000 },
                    PointGeometrySize = 12,
                    LineSmoothness = 0.6 // Hace que la curva sea suave
                }
            };

            // 2. Configurar el eje X (Días) y el eje Y (Moneda)
            Labels = new[] { "Lun", "Mar", "Mié", "Jue", "Vie", "Sáb", "Dom" };
            Formatter = value => value.ToString("C0"); // "C0" formatea como Moneda sin decimales

            // 3. Vincular los datos a la interfaz gráfica
            DataContext = this;
        }
    }
}