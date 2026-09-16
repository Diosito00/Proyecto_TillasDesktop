using System;
using System.Windows;
using System.Windows.Input;
using TillasDesktop.Entities.Inventario;
using TillasDesktop.BLL.Services;

namespace TillasDesktop.UI.Modelos
{
    public class IngresoStockViewModel : ViewModelBase
    {
        private string _productoResumen;
        private readonly Producto _productoPuro;
        private readonly InventarioService _inventarioService;
        private readonly ProductoTalle _nuevoMovimiento;

        public string ProductoResumen { get => _productoResumen; set { _productoResumen = value; OnPropertyChanged(); } }

        public decimal Talle
        {
            get => _nuevoMovimiento.Talle;
            set { _nuevoMovimiento.Talle = value; OnPropertyChanged(); }
        }

        public int Cantidad
        {
            get => _nuevoMovimiento.Stock_Actual;
            set { _nuevoMovimiento.Stock_Actual = value; OnPropertyChanged(); }
        }

        public ICommand GuardarCommand { get; }
        public Action CerrarVentana { get; set; }
        public Action<ProductoTalle> OnStockIngresado { get; set; }

        public IngresoStockViewModel(ProductoViewModel productoVM)
        {
            _inventarioService = new InventarioService();
            _productoPuro = productoVM.ObtenerEntidadPura();

            _nuevoMovimiento = new ProductoTalle
            {
                Producto_ID = _productoPuro.ID
            };

            ProductoResumen = $"Producto: {_productoPuro.Codigo_Modelo} - {_productoPuro.Nombre}";

            GuardarCommand = new RelayCommand(Guardar, PuedeGuardar);
        }

        private void Guardar(object parametro)
        {
            bool exito = _inventarioService.RegistrarIngresoStock(_nuevoMovimiento, out string mensaje);

            if (exito)
            {
                OnStockIngresado?.Invoke(_nuevoMovimiento);
                MessageBox.Show(mensaje, "Stock Actualizado", MessageBoxButton.OK, MessageBoxImage.Information);
                CerrarVentana?.Invoke();
            }
            else
            {
                MessageBox.Show(mensaje, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private bool PuedeGuardar(object parametro)
        {
            return Talle > 0 && Cantidad > 0;
        }
    }
}