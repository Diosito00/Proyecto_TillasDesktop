using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using TillasDesktop.Entities.Reportes;

namespace TillasDesktop.UI.Modelos
{
    // Hereda de ViewModelBase para que la interfaz reaccione a los cambios de fechas o filtros.
    public class ReportesViewModel : ViewModelBase
    {
       
        // Colección reactiva que mostrará el listado de ventas en la grilla principal.
        public ObservableCollection<VentaResumenDTO> HistorialVentas { get; set; }

        // Opciones estáticas para el ComboBox de filtrado rápido.
        public List<string> OpcionesPeriodo { get; set; }

        
        private string _periodoSeleccionado;
        public string PeriodoSeleccionado
        {
            get => _periodoSeleccionado;
            set
            {
                _periodoSeleccionado = value;
                OnPropertyChanged();

                // Cada vez que el usuario elige una nueva opción (ej: "Este Mes"), 
                // se dispara automáticamente el recálculo de las fechas de inicio y fin.
                ActualizarFechasPorPeriodo();
            }
        }

        // Fechas vinculadas a los controles DatePicker de la interfaz.
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

        
        public ICommand GenerarReporteCommand { get; }
        public ICommand ExportarCommand { get; }
        public ICommand VerDetalleCommand { get; }

        public ReportesViewModel()
        {
            OpcionesPeriodo = new List<string> { "Último Trimestre", "Este Mes", "Esta Semana" };
            HistorialVentas = new ObservableCollection<VentaResumenDTO>();

            // Valores por defecto al abrir la pantalla.
            PeriodoSeleccionado = "Último Trimestre";
            FechaFin = DateTime.Now;
            FechaInicio = DateTime.Now.AddMonths(-3);

            CargarHistorialSimulado();
            GenerarReporteCommand = new RelayCommand(GenerarReporte);
            ExportarCommand = new RelayCommand(ExportarReporte);
            VerDetalleCommand = new RelayCommand(VerDetalle);
        }

        private void GenerarReporte(object parametro)
        {
            // Validación básica para evitar que el usuario busque rangos ilógicos (ej: desde el 20 de mayo hasta el 10 de mayo).
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
            // Integración futura con librerías como iTextSharp o PDFsharp para generar el PDF.
            MessageBox.Show("El reporte se ha exportado exitosamente a PDF.", "Exportación Completa", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // Método auxiliar que traduce el texto del ComboBox en fechas matemáticas reales.
        private void ActualizarFechasPorPeriodo()
        {
            DateTime hoy = DateTime.Now;

            switch (PeriodoSeleccionado)
            {
                case "Esta Semana":
                    // Fórmula matemática para encontrar el lunes de la semana actual.
                    int diasDesdeLunes = (7 + (hoy.DayOfWeek - DayOfWeek.Monday)) % 7;
                    FechaInicio = hoy.AddDays(-diasDesdeLunes).Date;
                    FechaFin = hoy.Date;
                    break;

                case "Este Mes":
                    // Crea una fecha forzando el día a '1' manteniendo el año y mes actual.
                    FechaInicio = new DateTime(hoy.Year, hoy.Month, 1);
                    FechaFin = hoy.Date;
                    break;

                case "Último Trimestre":
                    FechaInicio = hoy.AddMonths(-3).Date;
                    FechaFin = hoy.Date;
                    break;
            }
        }

        // Método que se ejecuta al presionar el botón "Ver" (el ojo) en una fila de la grilla de ventas.
        private void VerDetalle(object parametro)
        {
            // Se asegura de que el parámetro enviado desde el XAML sea un resumen de venta válido.
            if (parametro is VentaResumenDTO ventaSeleccionada)
            {
                // Instancia la ventana hija (diseñada para ser un ticket de solo lectura).
                var ventanaDetalle = new Vistas.DetalleVentaView();

                // Le inyecta la venta seleccionada al ViewModel de esa ventana hija.
                ventanaDetalle.DataContext = new DetalleVentaViewModel(ventaSeleccionada);

                // ShowDialog() oscurece el fondo y bloquea la ventana principal hasta que el usuario cierre el ticket.
                ventanaDetalle.ShowDialog();
            }
        }

        private void CargarHistorialSimulado()
        {
            // Datos de prueba para poder visualizar el diseño de la grilla.
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