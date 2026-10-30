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

            if (chartControl == null || chartControl.ActualWidth <= 0 || chartControl.ActualHeight <= 0)
                return null;

            // Forzar que el layout esté al día antes de capturar
            chartControl.UpdateLayout();

            // DPI real del monitor
            var dpi = VisualTreeHelper.GetDpi(chartControl);
            double dpiX = dpi.PixelsPerInchX;
            double dpiY = dpi.PixelsPerInchY;

            // Tamaño lógico del control (en unidades WPF)
            double logicalWidth = chartControl.ActualWidth;
            double logicalHeight = chartControl.ActualHeight;

            // Tamaño en píxeles físicos, redondeando hacia arriba
            int pixelWidth = (int)Math.Ceiling(logicalWidth * dpi.DpiScaleX);
            int pixelHeight = (int)Math.Ceiling(logicalHeight * dpi.DpiScaleY);

            var rtb = new RenderTargetBitmap(pixelWidth, pixelHeight, dpiX, dpiY, PixelFormats.Pbgra32);

            // Dibujar mediante VisualBrush: evita desplazamientos por Margin/transformaciones
            var dv = new DrawingVisual();
            using (DrawingContext ctx = dv.RenderOpen())
            {
                var brush = new VisualBrush(chartControl)
                {
                    Stretch = Stretch.None,
                    AlignmentX = AlignmentX.Left,
                    AlignmentY = AlignmentY.Top
                };
                ctx.DrawRectangle(Brushes.White, null, new Rect(0, 0, logicalWidth, logicalHeight));
                ctx.DrawRectangle(brush, null, new Rect(0, 0, logicalWidth, logicalHeight));
            }
            rtb.Render(dv);

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(rtb));

            using (var ms = new MemoryStream())
            {
                encoder.Save(ms);
                return ms.ToArray();
            }
        }
    }
}
