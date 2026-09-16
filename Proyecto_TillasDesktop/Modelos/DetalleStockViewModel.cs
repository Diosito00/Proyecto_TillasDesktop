using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using TillasDesktop.BLL.Services;
using TillasDesktop.Entities.Inventario;

namespace TillasDesktop.UI.Modelos
{
    public class DetalleStockViewModel : ViewModelBase
    {
        private string _tituloVentana;
        private readonly Producto _productoPuro;
        private readonly ProductoViewModel _productoVMOriginal; // Guardamos la referencia para actualizar la tabla principal
        private readonly InventarioService _inventarioService;

        public string TituloVentana
        {
            get => _tituloVentana;
            set { _tituloVentana = value; OnPropertyChanged(); }
        }

        public ObservableCollection<ProductoTalle> ListaTalles { get; set; }

        public ICommand IngresarNuevoStockCommand { get; }
        public Action CerrarVentana { get; set; }

        public DetalleStockViewModel(ProductoViewModel productoVM)
        {
            _inventarioService = new InventarioService();
            _productoVMOriginal = productoVM;
            _productoPuro = productoVM.ObtenerEntidadPura();

            TituloVentana = $"Stock detallado: {_productoPuro.Codigo_Modelo} - {_productoPuro.Nombre}";

            // Simulamos datos temporales para que puedas ver la tabla armada
            ListaTalles = new ObservableCollection<ProductoTalle>
            {
                new ProductoTalle { Talle = 39, Stock_Actual = 20 },
                new ProductoTalle { Talle = 42, Stock_Actual = 25 }
            };

            IngresarNuevoStockCommand = new RelayCommand(AbrirVentanaIngreso);
        }

        private void AbrirVentanaIngreso(object parametro)
        {
            var formViewModel = new IngresoStockViewModel(_productoVMOriginal);

            formViewModel.OnStockIngresado = (movimientoStock) =>
            {
                _productoVMOriginal.StockTotal += movimientoStock.Stock_Actual;

                var talleExistente = ListaTalles.FirstOrDefault(t => t.Talle == movimientoStock.Talle);

                if (talleExistente != null)
                {
                    var talleActualizado = new ProductoTalle
                    {
                        Producto_ID = talleExistente.Producto_ID,
                        Talle = talleExistente.Talle,
                        Stock_Actual = talleExistente.Stock_Actual + movimientoStock.Stock_Actual
                    };

                    int index = ListaTalles.IndexOf(talleExistente);
                    ListaTalles[index] = talleActualizado;
                }
                else
                {
                    ListaTalles.Add(new ProductoTalle
                    {
                        Producto_ID = movimientoStock.Producto_ID,
                        Talle = movimientoStock.Talle,
                        Stock_Actual = movimientoStock.Stock_Actual
                    });
                }
            };

            var ventana = new Vistas.IngresoStockWindow();
            formViewModel.CerrarVentana = ventana.Close;
            ventana.DataContext = formViewModel;
            ventana.ShowDialog();
        }
    }
}