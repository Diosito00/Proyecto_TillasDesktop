using System;
using System.Collections.ObjectModel;
using System.Linq; 
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

        // Guardamos la referencia del envoltorio original. Esto es clave: 
        // si modificamos el stock aquí, la tabla de la ventana principal de Inventario se actualizará automáticamente.
        private readonly ProductoViewModel _productoVMOriginal;
        private readonly InventarioService _inventarioService;

        public string TituloVentana
        {
            get => _tituloVentana;
            set { _tituloVentana = value; OnPropertyChanged(); }
        }

        // ObservableCollection a diferencia de un List normal, 
        // esta colección avisa a la grilla automáticamente cuando se agrega, edita o elimina una fila.
        public ObservableCollection<ProductoTalle> ListaTalles { get; set; }

        // Comandos para interactuar con la grilla de talles.
        public ICommand IngresarNuevoStockCommand { get; }
        public ICommand EditarTalleCommand { get; }
        public ICommand EliminarTalleCommand { get; }

        // Delegado que permite a este ViewModel dar la orden de cerrar la ventana sin conocer directamente a la vista (Mantiene puro el MVVM).
        public Action CerrarVentana { get; set; }

        // El constructor recibe el producto seleccionado en la tabla principal.
        public DetalleStockViewModel(ProductoViewModel productoVM)
        {
            _inventarioService = new InventarioService();
            _productoVMOriginal = productoVM;
            _productoPuro = productoVM.ObtenerEntidadPura();

            // Construimos el título dinámico usando las propiedades de la entidad.
            TituloVentana = $"Stock detallado: {_productoPuro.Codigo_Modelo} - {_productoPuro.Nombre}";

            // Consultamos al servicio los talles reales que pertenecen a este ID de producto específico
            var tallesDesdeDb = _inventarioService.ObtenerTallesPorProducto(_productoPuro.ID);

            // Si la base de datos devuelve resultados, los cargamos; si no, iniciará vacía
            ListaTalles = new ObservableCollection<ProductoTalle>(tallesDesdeDb);

            // Y llamamos al método para que calcule el StockTotal basándose en estos datos reales
            RecalcularStockTotal();

            // Asignamos los comandos a sus respectivos métodos.
            IngresarNuevoStockCommand = new RelayCommand(AbrirVentanaIngreso);
            EditarTalleCommand = new RelayCommand(EditarTalle);
            EliminarTalleCommand = new RelayCommand(EliminarTalle);
        }

        //  Agregamos método auxiliar para sumar todo el stock actual de los talles
        private void RecalcularStockTotal()
        {
            _productoVMOriginal.StockTotal = ListaTalles.Sum(t => t.Stock_Actual);
        }


        private void AbrirVentanaIngreso(object parametro)
        {
            // Instanciamos el ViewModel encargado del formulario para agregar stock.
            var formViewModel = new IngresoStockViewModel(_productoVMOriginal);

            // Definimos qué pasa cuando se guarda con éxito en la ventana secundaria
            formViewModel.OnStockIngresado = (movimientoStock) =>
            {
                //  Actualizamos el stock total del modelo en la tabla principal.
                _productoVMOriginal.StockTotal += movimientoStock.Stock_Actual;

                //  Buscamos si el talle ingresado ya existía en nuestra lista.
                var talleExistente = ListaTalles.FirstOrDefault(t => t.Talle == movimientoStock.Talle);

                if (talleExistente != null)
                {
                    // Si ya existe, solo le sumamos la cantidad nueva al stock que ya tenía.
                    talleExistente.Stock_Actual += movimientoStock.Stock_Actual;
                }
                else
                {
                    // Si es un talle que no teníamos, agregamos una fila completamente nueva a la colección.
                    ListaTalles.Add(new ProductoTalle
                    {
                        Producto_ID = movimientoStock.Producto_ID,
                        Talle = movimientoStock.Talle,
                        Stock_Actual = movimientoStock.Stock_Actual
                    });
                }

                //  Recalculamos el total automáticamente basado en la lista real
                RecalcularStockTotal();
            };

            // Instanciamos y abrimos la ventana hija pasándole nuestro ViewModel configurado.
            var ventana = new Vistas.IngresoStockWindow();
            formViewModel.CerrarVentana = ventana.Close;
            ventana.DataContext = formViewModel;
            ventana.ShowDialog();
        }

        private void EditarTalle(object parametro)
        {
            // Validamos que el parámetro que llega desde el botón de la grilla sea realmente un ProductoTalle.
            if (parametro is ProductoTalle talleSeleccionado)
            {
                var ventanaEdicion = new IngresoStockWindow();

                // Le pasamos al ViewModel el producto y el talle puntual que queremos modificar.
                var viewModelEdicion = new IngresoStockViewModel(_productoVMOriginal, talleSeleccionado);

                viewModelEdicion.CerrarVentana = () => ventanaEdicion.Close();

                // Al guardar la edición, reemplazamos exactamente esa fila en nuestra colección.
                viewModelEdicion.OnStockIngresado = (talleActualizado) =>
                {
                    int index = ListaTalles.IndexOf(talleSeleccionado);

                    if (index >= 0)
                    {
                        // Reemplazamos el objeto en la misma posición para forzar a la grilla a redibujar los cambios. para que la grilla lo redibuje:
                        ListaTalles[index] = new ProductoTalle
                        {
                            ID = talleActualizado.ID,
                            Producto_ID = talleActualizado.Producto_ID,
                            Talle = talleActualizado.Talle,
                            Stock_Actual = talleActualizado.Stock_Actual
                        };
                    }

                    //  Recalculamos el total al editar
                    RecalcularStockTotal();
                };

                ventanaEdicion.DataContext = viewModelEdicion;
                ventanaEdicion.ShowDialog();
            }
        }

        private void EliminarTalle(object parametro)
        {
            if (parametro is ProductoTalle talleSeleccionado)
            {
                // Validamos la intención del usuario antes de eliminar datos.
                var respuesta = MessageBox.Show($"¿Estás seguro de que deseas eliminar el talle {talleSeleccionado.Talle}?",
                                                "Confirmar Eliminación", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (respuesta == MessageBoxResult.Yes)
                {
                    // Al removerlo de la ObservableCollection, desaparece instantáneamente de la grilla.
                    ListaTalles.Remove(talleSeleccionado);

                    // Recalculamos el total al eliminar un talle
                    RecalcularStockTotal();
                }
            }
        }
    }
}