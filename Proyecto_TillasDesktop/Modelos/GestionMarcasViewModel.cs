// Importa las colecciones observables de WPF para actualizar automáticamente las grillas (DataGrid) al añadir o quitar elementos
using System.Collections.ObjectModel;
// Importa clases base de WPF para la gestión de la aplicación y cuadros de mensaje interactivos
using System.Windows;
// Importa las utilidades de comandos y eventos de entrada de la interfaz de usuario en WPF
using System.Windows.Input;
// Importa los servicios de la Capa de Negocios encargados de gestionar las reglas del inventario
using TillasDesktop.BLL.Services;
// Importa las entidades del dominio de inventario (específicamente la clase Marca)
using TillasDesktop.Entities.Inventario;

// Definimos el espacio de nombres donde vive este ViewModel de marcas dentro de la aplicación.
namespace TillasDesktop.UI.Modelos
{
    // La clase hereda de 'ViewModelBase' para poder notificar automáticamente a la pantalla cuando los datos cambian (Patrón MVVM).
    public class GestionMarcasViewModel : ViewModelBase
    {
        // Instancia de solo lectura del servicio que maneja la lógica de negocio y se comunica con el repositorio de inventario.
        private readonly InventarioService _inventarioService;

        // Colección observable que almacena la lista de marcas; al agregar o quitar elementos, la tabla visual (DataGrid) se actualiza de forma automática.
        public ObservableCollection<Marca> ListaMarcas { get; set; }

        // Variable privada para guardar el nombre de la nueva marca que el usuario escribe en el cuadro de texto de la interfaz.
        private string _nombreNuevaMarca;

        // Propiedad pública vinculada al cuadro de texto de la vista para leer o escribir el nombre de la marca.
        public string NombreNuevaMarca
        {
            get => _nombreNuevaMarca;
            set { _nombreNuevaMarca = value; OnPropertyChanged(); } // Notifica a la interfaz que el valor cambió (Two-Way Binding).
        }

        // Comandos públicos asociados a los botones de la pantalla para ejecutar las acciones de guardar o eliminar marcas.
        public ICommand GuardarMarcaCommand { get; }
        public ICommand EliminarMarcaCommand { get; }

        // Constructor: se ejecuta automáticamente al abrir la ventana de gestión de marcas.
        public GestionMarcasViewModel()
        {
            // Inicializamos el servicio de inventario de la capa de negocios.
            _inventarioService = new InventarioService();

            // Cargamos la lista inicial de marcas activas desde la base de datos
            CargarLista();

            // Vinculamos cada comando con su respectivo método delegado de acción.
            GuardarMarcaCommand = new RelayCommand(GuardarMarca);
            EliminarMarcaCommand = new RelayCommand(EliminarMarca);
        }

        // Método privado para consultar las marcas activas y actualizar la colección observable de la interfaz
        private void CargarLista()
        {
            // Obtiene las marcas desde el servicio y envuelve el resultado en una nueva ObservableCollection
            ListaMarcas = new ObservableCollection<Marca>(_inventarioService.ObtenerMarcasActivas());
            // Notifica explícitamente a la vista que la colección completa ha sido actualizada
            OnPropertyChanged(nameof(ListaMarcas));
        }

        // Método que se ejecuta al hacer clic en el botón para guardar una nueva marca.
        private void GuardarMarca(object parametro)
        {
            // Creamos un objeto Marca nuevo asignándole el nombre que el usuario escribió en la pantalla.
            var nuevaMarca = new Marca { Nombre = NombreNuevaMarca };

            // Llamamos al servicio para registrarla en la base de datos, obteniendo un booleano de éxito y un mensaje descriptivo (out).
            bool exito = _inventarioService.RegistrarMarca(nuevaMarca, out string mensaje);

            // Si la base de datos confirmó el guardado con éxito...
            if (exito)
            {
                CargarLista(); // Refresca la lista para mostrar la marca recién creada o reactivada

                // Vaciamos el cuadro de texto del formulario para dejarlo limpio para un próximo ingreso.
                NombreNuevaMarca = string.Empty;

                // Mostramos un aviso emergente informando que la operación fue exitosa.
                MessageBox.Show(mensaje, "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                // Si hubo algún error o validación rechazada por la capa de negocios, mostramos el mensaje de advertencia.
                MessageBox.Show(mensaje, "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // Método que se ejecuta al hacer clic en el botón de eliminar de una fila específica de la tabla.
        private void EliminarMarca(object parametro)
        {
            // Verificamos que el parámetro recibido desde el botón de la grilla corresponda efectivamente a una marca válida.
            if (parametro is Marca marcaSeleccionada)
            {
                // Pedimos una confirmación explícita al usuario mediante un cuadro de diálogo antes de proceder con la baja.
                var respuesta = MessageBox.Show($"¿Desea eliminar la marca '{marcaSeleccionada.Nombre}'?", "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                // Si el usuario presiona que SÍ...
                if (respuesta == MessageBoxResult.Yes)
                {
                    // Llamamos al servicio para realizar la baja lógica en la base de datos utilizando el ID de la marca.
                    if (_inventarioService.EliminarMarca(marcaSeleccionada.ID, out string mensaje))
                    {
                        // Si la baja lógica en la BD fue exitosa, removemos el elemento directamente de la lista visual de la pantalla.
                        ListaMarcas.Remove(marcaSeleccionada);
                    }
                    else
                    {
                        // Si ocurrió algún error en la capa de datos (ej. dependencias), mostramos el mensaje correspondiente.
                        MessageBox.Show(mensaje, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
    }
}