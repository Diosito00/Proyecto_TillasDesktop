using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using TillasDesktop.Entities.Reportes;

namespace TillasDesktop.UI.Modelos
{
    public class ReportesViewModel : ViewModelBase
    {
        // === COLECCIÓN DE SOLO LECTURA ===
        public ObservableCollection<VentaResumenDTO> HistorialVentas { get; set; }
        public List<string> OpcionesPeriodo { get; set; }

        // === FILTROS ===
        private string _periodoSeleccionado;
        public string PeriodoSeleccionado
        {
            get => _periodoSeleccionado;
            set
            {
                _periodoSeleccionado = value;
                OnPropertyChanged();
                // Cada vez que el usuario cambia la opción, recalculamos las fechas
                ActualizarFechasPorPeriodo();
            }
        }
        private DateTime _fechaInicio;
        public DateTime FechaInicio
        {
            get => _fechaInicio;
            set { _fechaInicio = value; OnPropertyChanged(); }
        }

        private DateTime _fechaFin;
        public DateTime FechaFin
        {
            get => _fechaFin;
            set { _fechaFin = value; OnPropertyChanged(); }
        }

        // === COMANDOS ===
        public ICommand GenerarReporteCommand { get; }
        public ICommand ExportarCommand { get; }
        public ICommand VerDetalleCommand { get; }

        public ReportesViewModel()
        {
            OpcionesPeriodo = new List<string> { "Último Trimestre", "Este Mes", "Esta Semana" };
            HistorialVentas = new ObservableCollection<VentaResumenDTO>();

            PeriodoSeleccionado = "Último Trimestre";
            FechaFin = DateTime.Now;
            FechaInicio = DateTime.Now.AddMonths(-3);

            CargarHistorialSimulado();
            GenerarReporteCommand = new RelayCommand(GenerarReporte);
            ExportarCommand = new RelayCommand(ExportarReporte);
            VerDetalleCommand = new RelayCommand(VerDetalle);
        }

        // === LÓGICA DE BOTONES ===
        private void GenerarReporte(object parametro)
        {
            if (FechaInicio > FechaFin)
            {
                MessageBox.Show("La fecha de inicio no puede ser mayor a la fecha de fin.", "Error de fechas", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show($"Generando reporte desde {FechaInicio.ToShortDateString()} hasta {FechaFin.ToShortDateString()}...", "Procesando");
            }
        }

        private void ExportarReporte(object parametro)
        {
            MessageBox.Show("El reporte se ha exportado exitosamente a PDF.", "Exportación Completa", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ActualizarFechasPorPeriodo()
        {
            DateTime hoy = DateTime.Now;

            switch (PeriodoSeleccionado)
            {
                case "Esta Semana":
                    // Calculamos cuántos días pasaron desde el lunes
                    int diasDesdeLunes = (7 + (hoy.DayOfWeek - DayOfWeek.Monday)) % 7;
                    FechaInicio = hoy.AddDays(-diasDesdeLunes).Date;
                    FechaFin = hoy.Date;
                    break;

                case "Este Mes":
                    // Primer día del mes actual
                    FechaInicio = new DateTime(hoy.Year, hoy.Month, 1);
                    FechaFin = hoy.Date;
                    break;

                case "Último Trimestre":
                    // Restamos 3 meses exactos al día de hoy
                    FechaInicio = hoy.AddMonths(-3).Date;
                    FechaFin = hoy.Date;
                    break;
            }
        }

        private void VerDetalle(object parametro)
        {
            if (parametro is VentaResumenDTO ventaSeleccionada)
            {
                // Creamos la ventana y le inyectamos su ViewModel con la venta específica
                var ventanaDetalle = new Vistas.DetalleVentaView();
                ventanaDetalle.DataContext = new DetalleVentaViewModel(ventaSeleccionada);

                // ShowDialog oscurece la pantalla de atrás y obliga al usuario a cerrar el ticket
                ventanaDetalle.ShowDialog();
            }
        }

        private void CargarHistorialSimulado()
        {
            HistorialVentas.Add(new VentaResumenDTO
            {
                ID = 1045,
                Fecha_Hora = DateTime.Now.AddHours(-2),
                NombreVendedor = "Admin",
                NombreCliente = "Consumidor Final",
                MetodoPago = "Efectivo",
                Total = 125000m
            });

            HistorialVentas.Add(new VentaResumenDTO
            {
                ID = 1044,
                Fecha_Hora = DateTime.Now.AddDays(-1),
                NombreVendedor = "Cajero_1",
                NombreCliente = "Juan Pérez",
                MetodoPago = "Tarjeta Débito",
                Total = 235000m
            });
        }
    }
}
