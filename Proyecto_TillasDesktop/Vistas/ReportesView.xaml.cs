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

            // Identificamos cuál gráfico está visible según el reporte seleccionado
            if (vm.TipoReporteSeleccionado == "Reporte de Ventas")
                chartControl = GraficoVentasControl;
            else if (vm.TipoReporteSeleccionado == "Rendimiento de Vendedores")
                chartControl = GraficoVendedoresControl;
            else if (vm.TipoReporteSeleccionado == "Productos Más Vendidos")
                chartControl = GraficoProductosControl;

            if (chartControl == null || chartControl.ActualWidth <= 0 || chartControl.ActualHeight <= 0)
                return null;

            // Forzamos actualización de diseño y renderizamos a mapa de bits
            chartControl.UpdateLayout();

            int width = (int)chartControl.ActualWidth;
            int height = (int)chartControl.ActualHeight;

            var rtb = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            rtb.Render(chartControl);

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
