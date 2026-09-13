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

        // 1. Instanciamos la entidad pura desde el principio para guardar los datos ahí
        private readonly ProductoTalle _nuevoMovimiento;

        public string ProductoResumen { get => _productoResumen; set { _productoResumen = value; OnPropertyChanged(); } }

        // 2. Las propiedades de la UI leen y escriben directamente en la entidad pura
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
        public Action<int> OnStockIngresado { get; set; }

        public IngresoStockViewModel(ProductoViewModel productoVM)
        {
            _inventarioService = new InventarioService();
            _productoPuro = productoVM.ObtenerEntidadPura();

            // 3. Preparamos la entidad con el ID correcto apenas se abre la ventana
            _nuevoMovimiento = new ProductoTalle
            {
                Producto_ID = _productoPuro.ID
            };

            ProductoResumen = $"Producto: {_productoPuro.Codigo_Modelo} - {_productoPuro.Nombre}";

            GuardarCommand = new RelayCommand(Guardar, PuedeGuardar);
        }

        private void Guardar(object parametro)
        {
            // 4. Como los datos ya se guardaron en _nuevoMovimiento al tipear, lo enviamos directo a la BLL
            bool exito = _inventarioService.RegistrarIngresoStock(_nuevoMovimiento, out string mensaje);

            if (exito)
            {
                OnStockIngresado?.Invoke(Cantidad);
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