using System;
using System.Collections.ObjectModel;
using System.Linq; // Necesario implícitamente para usar .FirstOrDefault() más abajo.
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

            // Inicializamos la lista con datos hardcodeados para pruebas visuales.
            ListaTalles = new ObservableCollection<ProductoTalle>
            {
                new ProductoTalle { Talle = 39, Stock_Actual = 20 },
                new ProductoTalle { Talle = 42, Stock_Actual = 25 }
            };

            // Asignamos los comandos a sus respectivos métodos.
            IngresarNuevoStockCommand = new RelayCommand(AbrirVentanaIngreso);
            EditarTalleCommand = new RelayCommand(EditarTalle);
            EliminarTalleCommand = new RelayCommand(EliminarTalle);
        }

        private void AbrirVentanaIngreso(object parametro)
        {
            // Preparamos el ViewModel de la ventana hija (Ingreso de Stock).
            var formViewModel = new IngresoStockViewModel(_productoVMOriginal);

            // Definimos qué debe pasar cuando la ventana hija termine de guardar el stock exitosamente:
            formViewModel.OnStockIngresado = (movimientoStock) =>
            {
                // 1. Actualizamos el stock total del modelo en la tabla principal.
                _productoVMOriginal.StockTotal += movimientoStock.Stock_Actual;

                // 2. Buscamos si el talle ingresado ya existía en nuestra lista.
                var talleExistente = ListaTalles.FirstOrDefault(t => t.Talle == movimientoStock.Talle);

                if (talleExistente != null)
                {
                    // Si ya existe, NO agregamos una fila nueva. Creamos un registro actualizado sumando las cantidades.
                    var talleActualizado = new ProductoTalle
                    {
                        Producto_ID = talleExistente.Producto_ID,
                        Talle = talleExistente.Talle,
                        Stock_Actual = talleExistente.Stock_Actual + movimientoStock.Stock_Actual
                    };

                    // Reemplazamos el viejo por el nuevo en la colección para refrescar la UI.
                    int index = ListaTalles.IndexOf(talleExistente);
                    ListaTalles[index] = talleActualizado;
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
            };

            // Instanciamos y abrimos la ventana hija pasándole nuestro ViewModel configurado.
            var ventana = new Vistas.IngresoStockWindow();
            formViewModel.CerrarVentana = ventana.Close;
            ventana.DataContext = formViewModel;
            ventana.ShowDialog();
        }

        private void EditarTalle(object parametro)
        {
            // El parámetro viene del CommandParameter del botón en el XAML, asegurando que sea un ProductoTalle válido.
            if (parametro is ProductoTalle talleSeleccionado)
            {
                var ventanaEdicion = new IngresoStockWindow();

                // A diferencia de un nuevo ingreso, aquí le pasamos el talle específico que se va a editar.
                var viewModelEdicion = new IngresoStockViewModel(_productoVMOriginal, talleSeleccionado);

                viewModelEdicion.CerrarVentana = () => ventanaEdicion.Close();

                // Al guardar la edición, reemplazamos exactamente esa fila en nuestra colección.
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
                // Validamos la intención del usuario antes de eliminar datos.
                var respuesta = MessageBox.Show($"¿Estás seguro de que deseas eliminar el talle {talleSeleccionado.Talle}?",
                                                "Confirmar Eliminación", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (respuesta == MessageBoxResult.Yes)
                {
                    // Al removerlo de la ObservableCollection, desaparece instantáneamente de la grilla.
                    ListaTalles.Remove(talleSeleccionado);
                }
            }
        }
    }
}