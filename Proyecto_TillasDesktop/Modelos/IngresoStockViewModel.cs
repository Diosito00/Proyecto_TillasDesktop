using System;
using System.Windows;
using System.Windows.Input;

namespace TillasDesktop.UI.Modelos
{
    public class IngresoStockViewModel : ViewModelBase
    {
        private decimal _talle;
        private int _cantidad;
        private string _productoResumen;

        public string ProductoResumen { get => _productoResumen; set { _productoResumen = value; OnPropertyChanged(); } }
        public decimal Talle { get => _talle; set { _talle = value; OnPropertyChanged(); } }
        public int Cantidad { get => _cantidad; set { _cantidad = value; OnPropertyChanged(); } }

        public ICommand GuardarCommand { get; }
        public Action CerrarVentana { get; set; }

        // Acción para devolver la cantidad agregada al inventario
        public Action<int> OnStockIngresado { get; set; }

        public IngresoStockViewModel(ProductoViewModel producto)
        {
            // Mostramos de qué producto estamos ingresando stock
            ProductoResumen = $"Producto: {producto.Codigo_Modelo} - {producto.Nombre}";

            GuardarCommand = new RelayCommand(Guardar, PuedeGuardar);
        }

        private void Guardar(object parametro)
        {
            OnStockIngresado?.Invoke(Cantidad);
            MessageBox.Show($"Se agregaron {Cantidad} unidades exitosamente.", "Stock Actualizado");
            CerrarVentana?.Invoke();
        }

        private bool PuedeGuardar(object parametro)
        {
            // Solo habilitamos el botón si el talle y la cantidad son mayores a 0
            return Talle > 0 && Cantidad > 0;
        }
    }
}
