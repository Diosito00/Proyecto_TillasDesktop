using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TillasDesktop.UI.Modelos;

namespace TillasDesktop.UI.Vistas
{
    public partial class ReportesView : UserControl
    {
        public ReportesView()
        {
            InitializeComponent();

            var viewModel = new ReportesViewModel();
            // Conectamos la función que captura el gráfico activo desde la interfaz
            viewModel.SolicitarCapturaGrafico = ObtenerBytesGraficoActivo;
            this.DataContext = viewModel;
        }

        private byte[] ObtenerBytesGraficoActivo()
        {
            FrameworkElement chartControl = null;
            var vm = this.DataContext as ReportesViewModel;
            if (vm == null) return null;

            if (vm.TipoReporteSeleccionado == "Reporte de Ventas")
                chartControl = GraficoVentasControl;
            else if (vm.TipoReporteSeleccionado == "Rendimiento de Vendedores")
                chartControl = GraficoVendedoresControl;
            else if (vm.TipoReporteSeleccionado == "Productos Más Vendidos")
                chartControl = GraficoProductosControl;

            if (chartControl == null) return null;

            // DEFINIMOS UN TAMAÑO FIJO PARA LA EXPORTACIÓN
            // Esto evita los cortes por redimensionamiento dinámico
            int width = 1100;
            int height = 380;

            // Guardamos los valores originales para restaurarlos después
            var originalWidth = chartControl.Width;
            var originalHeight = chartControl.Height;

            try
            {
                // Desacoplamos temporalmente el control forzando un tamaño fijo
                chartControl.Width = width;
                chartControl.Height = height;

                // Forzamos al sistema de WPF a medir y dibujar el control con este nuevo tamaño exacto
                chartControl.Measure(new Size(width, height));
                chartControl.Arrange(new Rect(new Size(width, height)));
                chartControl.UpdateLayout();

                // Capturamos el mapa de bits
                var rtb = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
                rtb.Render(chartControl);

                // Codificamos a PNG
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(rtb));

                using (var ms = new MemoryStream())
                {
                    encoder.Save(ms);
                    return ms.ToArray();
                }
            }
            finally
            {
                // Devolvemos el control a su estado normal
                chartControl.Width = originalWidth;
                chartControl.Height = originalHeight;

                // Devolvemos el diseño a la normalidad en la vista
                chartControl.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                chartControl.Arrange(new Rect(new Size(chartControl.ActualWidth, chartControl.ActualHeight)));
                chartControl.UpdateLayout();
            }
        }
    }
}
