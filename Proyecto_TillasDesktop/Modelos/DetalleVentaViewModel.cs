using TillasDesktop.Entities.Facturacion;

namespace TillasDesktop.UI.Modelos
{
    public class DetalleVentaViewModel : ViewModelBase
    {
        private readonly DetalleVenta _detallePuro;

        private string _nombreProducto;

        public DetalleVentaViewModel(DetalleVenta detalle, string nombreProducto)
        {
            _detallePuro = detalle;
            _nombreProducto = nombreProducto;
        }

        public string NombreProducto
        {
            get => _nombreProducto;
            set { _nombreProducto = value; OnPropertyChanged(); }
        }

        public int Cantidad
        {
            get => _detallePuro.Cantidad;
            set
            {
                _detallePuro.Cantidad = value;
                OnPropertyChanged();

                // Al cambiar la cantidad, notificamos a WPF que repinte el texto del Subtotal
                OnPropertyChanged(nameof(Subtotal));
            }
        }

        public decimal PrecioUnitario
        {
            get => _detallePuro.Precio_Unitario;
            set
            {
                _detallePuro.Precio_Unitario = value;
                OnPropertyChanged();

                // Agregado: Si el precio cambia (ej. descuento manual), el subtotal también debe repintarse.
                OnPropertyChanged(nameof(Subtotal));
            }
        }

        // Propiedad de solo lectura para la vista. Al no tener "set", WPF sabe que solo debe leerla.
        public decimal Subtotal => Cantidad * PrecioUnitario;

        // PUENTE A LA BLL: Método de utilidad para devolver la entidad pura fácilmente.
        public DetalleVenta ObtenerEntidadPura()
        {
            return _detallePuro;
        }
    }
}