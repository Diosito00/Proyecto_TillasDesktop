using System.Collections.ObjectModel;
using TillasDesktop.Entities.Reportes;

namespace TillasDesktop.UI.Modelos
{
    public class DetalleVentaViewModel : ViewModelBase
    {
        // El encabezado del ticket
        public VentaResumenDTO VentaGeneral { get; set; }

        // La lista de productos comprados
        public ObservableCollection<LineaDetalleVentaDTO> Lineas { get; set; }

        public DetalleVentaViewModel(VentaResumenDTO ventaSeleccionada)
        {
            VentaGeneral = ventaSeleccionada;
            Lineas = new ObservableCollection<LineaDetalleVentaDTO>();

            CargarDetallesSimulados();
        }

        private void CargarDetallesSimulados()
        {
            // En el futuro, aquí llamarás a: _reportesService.ObtenerLineasPorTicket(VentaGeneral.ID);

            // Simulamos un par de zapatillas para este ticket
            Lineas.Add(new LineaDetalleVentaDTO
            {
                CodigoModelo = "NK-AF1-01",
                NombreProducto = "Nike Air Force 1",
                Marca = "Nike",
                Talle = 42m,
                Cantidad = 1,
                PrecioUnitario = 125000m
            });
        }
    }
}