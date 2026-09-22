using System.Collections.ObjectModel;
using TillasDesktop.Entities.Reportes;

namespace TillasDesktop.UI.Modelos
{
    // Hereda de ViewModelBase para mantener la consistencia del proyecto, 
    // aunque al ser una vista de solo lectura casi no requiera notificar cambios dinámicos.
    public class DetalleVentaViewModel : ViewModelBase
    {
        // Contiene la info principal del ticket (fecha, cajero, total, etc.).
        // Usamos un DTO (Data Transfer Object) porque es un objeto optimizado y armado específicamente para reportes o consultas.
        public VentaResumenDTO VentaGeneral { get; set; }

        // Colección observable con los ítems individuales que forman parte de esta venta.
        public ObservableCollection<LineaDetalleVentaDTO> Lineas { get; set; }

        // El constructor recibe obligatoriamente la venta que el usuario seleccionó en la grilla principal.
        public DetalleVentaViewModel(VentaResumenDTO ventaSeleccionada)
        {
            VentaGeneral = ventaSeleccionada;
            Lineas = new ObservableCollection<LineaDetalleVentaDTO>();

            // Llama al método que buscará los detalles de esa venta.
            CargarDetallesSimulados();
        }

        private void CargarDetallesSimulados()
        {
            // Simulación temporal para diseñar la ventana (UI) sin depender de la base de datos.
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