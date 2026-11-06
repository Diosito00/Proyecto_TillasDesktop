// Importa el espacio de nombres fundamental de .NET para tipos base y utilidades generales
using System;
// Importa las colecciones observables de WPF para actualizar automáticamente las grillas de la interfaz al agregar o quitar elementos
using System.Collections.ObjectModel;
// Importa las extensiones de LINQ para realizar consultas y operaciones sobre colecciones en memoria (como sumas o búsquedas)
using System.Linq;
// Importa las clases base de WPF para la gestión de la aplicación y cuadros de diálogo interactivos
using System.Windows;
// Importa las utilidades de comandos y eventos de entrada de la interfaz de usuario en WPF
using System.Windows.Input;
// Importa los servicios de la Capa de Negocios (BLL) encargados de gestionar las reglas e inventario de productos
using TillasDesktop.BLL.Services;
// Importa las entidades del dominio de inventario (como Producto y ProductoTalle)
using TillasDesktop.Entities.Inventario;
// Importa las vistas de la interfaz de usuario de la aplicación
using TillasDesktop.UI.Vistas;


// Define el espacio de nombres (namespace) correspondiente a los modelos de vista (ViewModels) de la interfaz de usuario del sistema
namespace TillasDesktop.UI.Modelos
{
    // Declara la clase pública DetalleStockViewModel que hereda de ViewModelBase, encargada de administrar la lógica de talles y stock de un producto específico
    public class DetalleStockViewModel : ViewModelBase
    {
        // Campo privado que almacena el título dinámico de la ventana de detalles de stock
        private string _tituloVentana;

        // Referencia privada al modelo de datos puro del producto seleccionado
        private readonly Producto _productoPuro;

        // Guardamos la referencia del envoltorio original. Esto es clave: 
        // si modificamos el stock aquí, la tabla de la ventana principal de Inventario se actualizará automáticamente.
        private readonly ProductoViewModel _productoVMOriginal;

        // Instancia de servicio de la Capa de Negocios para consultar y persistir operaciones de inventario y talles
        private readonly InventarioService _inventarioService;


        // Propiedad pública enlazada a la interfaz para mostrar y actualizar el título dinámico de la ventana
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
            // Instancia el servicio de inventario de la capa de negocio
            _inventarioService = new InventarioService();
            // Guarda la referencia del ViewModel original para propagar cambios de stock globales
            _productoVMOriginal = productoVM;
            // Extrae la entidad pura  del producto
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


        // Método privado que maneja la apertura de la ventana secundaria para ingresar nuevo stock o talles
        private void AbrirVentanaIngreso(object parametro)
        {
            // Instanciamos el ViewModel encargado del formulario para agregar stock, pasándole el producto original.
            var formViewModel = new IngresoStockViewModel(_productoVMOriginal);

            // Definimos qué pasa cuando se guarda con éxito en la ventana secundaria
            formViewModel.OnStockIngresado = (movimientoStock) =>
            {
                
                //  Buscamos si el talle ingresado ya existía en nuestra lista.
                var talleExistente = ListaTalles.FirstOrDefault(t => t.Talle == movimientoStock.Talle);

                if (talleExistente != null)
                {
                    // Obtenemos el índice y reemplazamos la fila para forzar el redibujado
                    int index = ListaTalles.IndexOf(talleExistente);
                    ListaTalles[index] = new ProductoTalle
                    {
                        ID = talleExistente.ID,
                        Producto_ID = talleExistente.Producto_ID,
                        Talle = talleExistente.Talle,
                        Stock_Actual = movimientoStock.Stock_Actual // Valor exacto ya sumado en la BLL
                    };
                }
                else
                {
                    // Si es un talle que no teníamos, agregamos una fila completamente nueva a la colección.
                    ListaTalles.Add(new ProductoTalle
                    {
                        ID = movimientoStock.ID,
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

        // Método privado que gestiona la edición de un talle y su stock existente
        private void EditarTalle(object parametro)
        {
            // Validamos que el parámetro que llega desde el botón de la grilla sea realmente un ProductoTalle.
            if (parametro is ProductoTalle talleSeleccionado)
            {
                // Instanciamos la ventana de edición
                var ventanaEdicion = new IngresoStockWindow();

                // Le pasamos al ViewModel el producto y el talle puntual que queremos modificar.
                var viewModelEdicion = new IngresoStockViewModel(_productoVMOriginal, talleSeleccionado);

                // Configura la acción para cerrar la ventana hija
                viewModelEdicion.CerrarVentana = () => ventanaEdicion.Close();

                // Al guardar la edición, reemplazamos exactamente esa fila en nuestra colección para reflejar los cambios.
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
                // Asigna el DataContext y muestra la ventana modal de edición
                ventanaEdicion.DataContext = viewModelEdicion;
                ventanaEdicion.ShowDialog();
            }
        }


        // Método privado que maneja la baja o eliminación física/lógica de un talle de producto
        private void EliminarTalle(object parametro)
        {
            // Verifica que el parámetro recibido corresponda a un ProductoTalle
            if (parametro is ProductoTalle talleSeleccionado)
            {
                // Validamos la intención del usuario antes de eliminar datos.
                var respuesta = MessageBox.Show($"¿Estás seguro de que deseas eliminar el talle {talleSeleccionado.Talle}?",
                                                "Confirmar Eliminación", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (respuesta == MessageBoxResult.Yes)
                {
                    // Ejecuta la baja en la capa de servicios y recibe un parámetro booleano de éxito junto con un mensaje descriptivo (out)
                    bool exito = _inventarioService.EliminarTalle(talleSeleccionado.ID, out string mensaje);

                    if (exito)
                    {
                        // Si se eliminó con éxito de la base de datos, lo quitamos de la colección observable local
                        ListaTalles.Remove(talleSeleccionado);
                        // Recalculamos el total de stock del producto
                        RecalcularStockTotal();
                        // Notificamos el éxito al operador mediante un cuadro de diálogo informativo
                        MessageBox.Show(mensaje, "Operación Exitosa", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        // Si ocurrió un inconveniente (ej. dependencias o restricciones), informamos mediante una advertencia
                        MessageBox.Show(mensaje, "Error al Eliminar", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
        }
    }
}
