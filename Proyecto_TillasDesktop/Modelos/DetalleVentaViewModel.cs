using System.Collections.ObjectModel;
using TillasDesktop.Entities.Reportes;

namespace TillasDesktop.UI.Modelos
{
    // Hereda de ViewModelBase para mantener el estándar, aunque al ser de solo lectura rara vez usará el OnPropertyChanged aquí.
    public class DetalleVentaViewModel : ViewModelBase
    {
        // El encabezado del ticket (Contiene la fecha, el cajero, el total, etc.).
        // Usamos DTOs (Data Transfer Objects) porque son versiones "procesadas" de las entidades, ideales para reportes.
        public VentaResumenDTO VentaGeneral { get; set; }

        // La lista de productos que componen este ticket específico.
        public ObservableCollection<LineaDetalleVentaDTO> Lineas { get; set; }

        // El constructor exige que le pases la venta seleccionada desde la tabla principal.
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