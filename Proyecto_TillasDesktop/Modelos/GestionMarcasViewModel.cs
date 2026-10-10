using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using TillasDesktop.BLL.Services;
using TillasDesktop.Entities.Inventario;

// Definimos el espacio de nombres donde vive este ViewModel de marcas.
namespace TillasDesktop.UI.Modelos
{

    // La clase hereda de 'ViewModelBase' para poder notificar automáticamente a la pantalla cuando los datos cambian.
    public class GestionMarcasViewModel : ViewModelBase
    {
        // Instancia del servicio que maneja la lógica de negocio y se comunica con el repositorio.
        private readonly InventarioService _inventarioService;

        // Colección observable que almacena la lista de marcas; al agregar o quitar elementos, la tabla visual (DataGrid) se actualiza sola.
        public ObservableCollection<Marca> ListaMarcas { get; set; }

        // Variable privada para guardar el nombre de la nueva marca que el usuario tipea en la interfaz.
        private string _nombreNuevaMarca;

        // Propiedad pública vinculada al cuadro de texto de la vista para leer o escribir el nombre de la marca.
        public string NombreNuevaMarca
        {
            get => _nombreNuevaMarca;
            set { _nombreNuevaMarca = value; OnPropertyChanged(); }
        }

        // Comandos asociados a los botones de la pantalla para guardar o eliminar marcas.
        public ICommand GuardarMarcaCommand { get; }
        public ICommand EliminarMarcaCommand { get; }

        // Constructor: se ejecuta automáticamente al abrir la ventana de gestión de marcas.
        public GestionMarcasViewModel()
        {
            // Inicializamos el servicio de inventario.
            _inventarioService = new InventarioService();

            // Cargamos en la lista visual únicamente las marcas que se encuentran activas en la base de datos.
            ListaMarcas = new ObservableCollection<Marca>(_inventarioService.ObtenerMarcasActivas());

            // Vinculamos cada comando con su respectivo método de acción.
            GuardarMarcaCommand = new RelayCommand(GuardarMarca);
            EliminarMarcaCommand = new RelayCommand(EliminarMarca);
        }

        // Método que se ejecuta al hacer clic en el botón para guardar una nueva marca.
        private void GuardarMarca(object parametro)
        {
            // Creamos un objeto Marca nuevo asignándole el nombre que el usuario escribió en la pantalla.
            var nuevaMarca = new Marca { Nombre = NombreNuevaMarca };

            // Llamamos al servicio para registrarla en la base de datos, obteniendo un booleano de éxito y un mensaje.
            bool exito = _inventarioService.RegistrarMarca(nuevaMarca, out string mensaje);

            // Si la base de datos confirmó el guardado con éxito...
            if (exito)
            {
                // Agregamos la marca a la lista en memoria para que se vea reflejada instantáneamente en la tabla.
                ListaMarcas.Add(nuevaMarca);

                // Vaciamos el cuadro de texto del formulario para dejarlo limpio.
                NombreNuevaMarca = string.Empty;

                // Mostramos un aviso informando que la operación fue exitosa.
                MessageBox.Show(mensaje, "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                // Si hubo algún error o validación rechazada, mostramos el mensaje de advertencia.
                MessageBox.Show(mensaje, "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // Método que se ejecuta al hacer clic en el botón de eliminar de una fila específica de la tabla.
        private void EliminarMarca(object parametro)
        {
            // Verificamos que el parámetro recibido desde el botón corresponda efectivamente a una marca válida.
            if (parametro is Marca marcaSeleccionada)
            {
                // Pedimos una confirmación al usuario mediante un cuadro de diálogo antes de proceder.
                var respuesta = MessageBox.Show($"¿Desea eliminar la marca '{marcaSeleccionada.Nombre}'?", "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                // Si el usuario presiona que SÍ...
                if (respuesta == MessageBoxResult.Yes)
                {
                    // Llamamos al servicio para realizar la baja lógica en la base de datos utilizando el ID.
                    if (_inventarioService.EliminarMarca(marcaSeleccionada.ID, out string mensaje))
                    {
                        // Si la baja lógica en la BD fue exitosa, removemos el elemento de la lista visual de la pantalla.
                        ListaMarcas.Remove(marcaSeleccionada);
                    }
                    else
                    {
                        // Si ocurrió algún error, mostramos el mensaje correspondiente.
                        MessageBox.Show(mensaje, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
    }
}