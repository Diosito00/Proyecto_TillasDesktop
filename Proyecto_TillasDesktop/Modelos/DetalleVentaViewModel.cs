using System.Collections.ObjectModel;
using TillasDesktop.BLL.Services;
using TillasDesktop.Entities.Reportes;

namespace TillasDesktop.UI.Modelos
{
    public class DetalleVentaViewModel : ViewModelBase
    {
        private readonly VentasService _ventasService;

        public int VentaID { get; set; }
        public string Fecha { get; set; }
        public string Cliente { get; set; }
        public string Vendedor { get; set; }
        public decimal Total { get; set; }

        // Colección tipada con el DTO para mostrar los productos específicos de esta venta
        public ObservableCollection<LineaDetalleVentaDTO> ProductosVendidos { get; set; }

        public DetalleVentaViewModel(dynamic ventaSeleccionada)
        {
            _ventasService = new VentasService();

            // Asignamos los datos principales del encabezado desde el objeto seleccionado en la grilla
            VentaID = ventaSeleccionada.ID;
            Fecha = ventaSeleccionada.Fecha_Hora.ToString("dd/MM/yyyy HH:mm");
            Cliente = ventaSeleccionada.NombreCliente;
            Vendedor = ventaSeleccionada.NombreVendedor;
            Total = ventaSeleccionada.Total;

            ProductosVendidos = new ObservableCollection<LineaDetalleVentaDTO>();

            CargarDetallesVenta();
        }

        private void CargarDetallesVenta()
        {
            // Llamamos al servicio pasando el ID de la venta actual
            var detallesDb = _ventasService.ObtenerDetallesVenta(VentaID);

            ProductosVendidos.Clear();

            // Poblamos la ObservableCollection para que la vista gráfica (DataGrid) se actualice
            foreach (var detalle in detallesDb)
            {
                ProductosVendidos.Add(detalle);
            }
        }
    }
}