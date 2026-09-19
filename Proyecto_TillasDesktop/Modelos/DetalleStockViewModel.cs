using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using TillasDesktop.BLL.Services;
using TillasDesktop.Entities.Inventario;
using TillasDesktop.UI.Vistas;

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
        public ICommand EditarTalleCommand { get; }
        public ICommand EliminarTalleCommand { get; }
        public Action CerrarVentana { get; set; }

        public DetalleStockViewModel(ProductoViewModel productoVM)
        {
            _inventarioService = new InventarioService();
            _productoVMOriginal = productoVM;
            _productoPuro = productoVM.ObtenerEntidadPura();

            TituloVentana = $"Stock detallado: {_productoPuro.Codigo_Modelo} - {_productoPuro.Nombre}";

            ListaTalles = new ObservableCollection<ProductoTalle>
            {
                new ProductoTalle { Talle = 39, Stock_Actual = 20 },
                new ProductoTalle { Talle = 42, Stock_Actual = 25 }
            };

            IngresarNuevoStockCommand = new RelayCommand(AbrirVentanaIngreso);
            EditarTalleCommand = new RelayCommand(EditarTalle);
            EliminarTalleCommand = new RelayCommand(EliminarTalle);
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

        private void EditarTalle(object parametro)
        {
            // Verificamos que el parámetro que llega del XAML sea de tu clase ProductoTalle
            if (parametro is ProductoTalle talleSeleccionado)
            {
                var ventanaEdicion = new IngresoStockWindow();

                var viewModelEdicion = new IngresoStockViewModel(_productoVMOriginal, talleSeleccionado);

                viewModelEdicion.CerrarVentana = () => ventanaEdicion.Close();

                viewModelEdicion.OnStockIngresado = (talleActualizado) =>
                {
                    int index = ListaTalles.IndexOf(talleSeleccionado);

                    if (index >= 0)
                    {
                        ListaTalles[index] = talleActualizado;
                    }
                };

                ventanaEdicion.DataContext = viewModelEdicion;
                ventanaEdicion.ShowDialog();
            }
        }

        private void EliminarTalle(object parametro)
        {
            if (parametro is ProductoTalle talleSeleccionado)
            {
                // Validamos si realmente desea eliminarlo
                var respuesta = MessageBox.Show($"¿Estás seguro de que deseas eliminar el talle {talleSeleccionado.Talle}?",
                                                "Confirmar Eliminación", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (respuesta == MessageBoxResult.Yes)
                {
                    ListaTalles.Remove(talleSeleccionado);
                }
            }
        }
    }
}