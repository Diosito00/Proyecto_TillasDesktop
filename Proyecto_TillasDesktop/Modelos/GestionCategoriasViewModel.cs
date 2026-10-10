using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using TillasDesktop.BLL.Services;
using TillasDesktop.Entities.Inventario;

// Definimos el espacio de nombres donde se encuentra este ViewModel de categorías.
namespace TillasDesktop.UI.Modelos
{
    // La clase hereda de 'ViewModelBase' para poder notificar a la interfaz gráfica cuando cambian los datos.
    public class GestionCategoriasViewModel : ViewModelBase
    {
        // Instancia del servicio que se encarga de conectar con la lógica de negocio y la base de datos.
        private readonly InventarioService _inventarioService;

        // Colección observable que contiene la lista de categorías; al modificarla, la pantalla (DataGrid) se actualiza sola.
        public ObservableCollection<Categoria> ListaCategorias { get; set; }

        // Variable privada para almacenar el nombre de la nueva categoría que el usuario escribe en el formulario.
        private string _nombreNuevaCategoria;

        // Propiedad pública vinculada al cuadro de texto de la vista para leer o escribir el nombre de la categoría.
        public string NombreNuevaCategoria
        {
            get => _nombreNuevaCategoria;
            set { 
                _nombreNuevaCategoria = value; OnPropertyChanged();  // Avisa a la pantalla que el valor cambió para mantenerla sincronizada.
            }
        }

        // Comandos que se vinculan a los botones de la interfaz para guardar o eliminar categorías.
        public ICommand GuardarCategoriaCommand { get; }
        public ICommand EliminarCategoriaCommand { get; }

        // Constructor: se ejecuta al abrir la ventana de gestión de categorías.
        public GestionCategoriasViewModel()
        {
            // Inicializamos el servicio de inventario
            _inventarioService = new InventarioService();

            // Cargamos en la lista visual únicamente las categorías que están activas en la base de datos.
            ListaCategorias = new ObservableCollection<Categoria>(_inventarioService.ObtenerCategoriasActivas());

            // Asociamos los comandos con sus respectivos métodos de acción.
            GuardarCategoriaCommand = new RelayCommand(GuardarCategoria);
            EliminarCategoriaCommand = new RelayCommand(EliminarCategoria);
        }

        // Método que se ejecuta al hacer clic en el botón de guardar una nueva categoría.
        private void GuardarCategoria(object parametro)
        {
            // Creamos un objeto Categoria nuevo asignándole el nombre escrito en la pantalla.
            var nuevaCat = new Categoria { Nombre = NombreNuevaCategoria };

            // Llamamos al servicio para registrarla en la base de datos y obtenemos un booleano de éxito y un mensaje.
            bool exito = _inventarioService.RegistrarCategoria(nuevaCat, out string mensaje);

            // Si la base de datos la guardó correctamente..
            if (exito)
            {
                // La agregamos a la lista en memoria para que aparezca instantáneamente en la tabla.
                ListaCategorias.Add(nuevaCat);

                // Limpiamos el cuadro de texto para que quede en blanco.
                NombreNuevaCategoria = string.Empty;

                // Mostramos un aviso de éxito en pantalla.
                MessageBox.Show(mensaje, "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                // Si hubo algún error o validación rechazada, mostramos el mensaje de advertencia.
                MessageBox.Show(mensaje, "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // Método que se ejecuta al hacer clic en el botón de eliminar de alguna fila de la tabla.
        private void EliminarCategoria(object parametro)
        {
            // Verificamos que el parámetro recibido desde el botón corresponda a una categoría válida.
            if (parametro is Categoria catSeleccionada)
            {
                // Pedimos una confirmación al usuario antes de proceder con la baja.
                var respuesta = MessageBox.Show($"¿Desea eliminar la categoría '{catSeleccionada.Nombre}'?", "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                // Si el usuario presiona que SÍ...
                if (respuesta == MessageBoxResult.Yes)
                {
                    // Llamamos al servicio para realizar la baja lógica en la base de datos usando el ID.
                    if (_inventarioService.EliminarCategoria(catSeleccionada.ID, out string mensaje))
                    {
                        // Si se borró con éxito en la BD, la quitamos de la lista visual de la pantalla.
                        ListaCategorias.Remove(catSeleccionada);
                    }
                    else
                    {
                        // Si ocurrió un error, mostramos el mensaje correspondiente.
                        MessageBox.Show(mensaje, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
    }
}