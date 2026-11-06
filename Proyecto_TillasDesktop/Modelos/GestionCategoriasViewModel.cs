// Importa las colecciones observables de WPF para actualizar automáticamente las grillas (DataGrid) al agregar o quitar elementos
using System.Collections.ObjectModel;
// Importa clases base de WPF para la gestión de la aplicación y cuadros de mensaje interactivos
using System.Windows;
// Importa las utilidades de comandos y eventos de entrada de la interfaz de usuario en WPF
using System.Windows.Input;
// Importa los servicios de la Capa de Negocios encargados de gestionar las reglas de inventario
using TillasDesktop.BLL.Services;
// Importa las entidades del dominio de inventario (específicamente la clase Categoria)
using TillasDesktop.Entities.Inventario;

// Definimos el espacio de nombres donde se encuentra este ViewModel de categorías dentro de la aplicación.
namespace TillasDesktop.UI.Modelos
{
    // La clase hereda de 'ViewModelBase' para poder notificar a la interfaz gráfica cuando cambian los datos (Patrón MVVM).
    public class GestionCategoriasViewModel : ViewModelBase
    {
        // Instancia de solo lectura del servicio que se encarga de conectar con la lógica de negocio y la base de datos de inventario.
        private readonly InventarioService _inventarioService;

        // Colección observable que contiene la lista de categorías; al modificarla, la pantalla (DataGrid) se actualiza de forma automática.
        public ObservableCollection<Categoria> ListaCategorias { get; set; }

        // Variable privada para almacenar el nombre de la nueva categoría que el usuario escribe en el formulario de la interfaz.
        private string _nombreNuevaCategoria;

        // Propiedad pública vinculada al cuadro de texto de la vista para leer o escribir el nombre de la nueva categoría.
        public string NombreNuevaCategoria
        {
            get => _nombreNuevaCategoria;
            set
            {
                _nombreNuevaCategoria = value; OnPropertyChanged();  // Avisa a la pantalla que el valor cambió para mantener la interfaz sincronizada (Two-Way Binding).
            }
        }

        // Comandos públicos que se vinculan a los botones de la interfaz para ejecutar las acciones de guardar o eliminar categorías.
        public ICommand GuardarCategoriaCommand { get; }
        public ICommand EliminarCategoriaCommand { get; }

        // Constructor: se ejecuta automáticamente al abrir la ventana de gestión de categorías.
        public GestionCategoriasViewModel()
        {
            // Inicializamos la instancia del servicio de inventario de la capa de negocios
            _inventarioService = new InventarioService();

            // Cargamos la lista inicial de categorías desde la base de datos
            CargarLista();

            // Asociamos los comandos con sus respectivos métodos de acción delegados.
            GuardarCategoriaCommand = new RelayCommand(GuardarCategoria);
            EliminarCategoriaCommand = new RelayCommand(EliminarCategoria);
        }

        // Método privado para consultar las categorías activas y poblar la colección observable de la interfaz
        private void CargarLista()
        {
            // Obtiene las categorías desde el servicio y envuelve el resultado en una ObservableCollection
            ListaCategorias = new ObservableCollection<Categoria>(_inventarioService.ObtenerCategoriasActivas());
            // Notifica explícitamente a la vista que la colección completa ha cambiado o sido reemplazada
            OnPropertyChanged(nameof(ListaCategorias));
        }

        // Método que se ejecuta al hacer clic en el botón de guardar una nueva categoría.
        private void GuardarCategoria(object parametro)
        {
            // Creamos un objeto Categoria nuevo asignándole el nombre ingresado por el usuario en la pantalla.
            var nuevaCat = new Categoria { Nombre = NombreNuevaCategoria };

            // Llamamos al servicio para registrarla en la base de datos; devuelve un booleano de éxito y un mensaje descriptivo (out).
            bool exito = _inventarioService.RegistrarCategoria(nuevaCat, out string mensaje);

            // Si la base de datos guardó el registro correctamente...
            if (exito)
            {
                CargarLista(); // Refresca y actualiza la lista visual en la grilla

                // Limpiamos el cuadro de texto de entrada para que quede en blanco para un próximo uso.
                NombreNuevaCategoria = string.Empty;

                // Mostramos un aviso emergente de éxito al operador.
                MessageBox.Show(mensaje, "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                // Si hubo algún error o validación rechazada por la capa de negocios, mostramos el mensaje de advertencia.
                MessageBox.Show(mensaje, "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // Método que se ejecuta al hacer clic en el botón de eliminar de alguna fila específica de la tabla.
        private void EliminarCategoria(object parametro)
        {
            // Verificamos que el parámetro recibido desde el botón de la grilla corresponda a un objeto Categoria válido.
            if (parametro is Categoria catSeleccionada)
            {
                // Solicitamos una confirmación explícita al usuario antes de proceder con la baja.
                var respuesta = MessageBox.Show($"¿Desea eliminar la categoría '{catSeleccionada.Nombre}'?", "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                // Si el usuario presiona que SÍ...
                if (respuesta == MessageBoxResult.Yes)
                {
                    // Llamamos al servicio para realizar la baja lógica o física en la base de datos utilizando el ID de la categoría.
                    if (_inventarioService.EliminarCategoria(catSeleccionada.ID, out string mensaje))
                    {
                        // Si se borró con éxito en la BD, la quitamos directamente de la lista visual observable para refrescar la pantalla.
                        ListaCategorias.Remove(catSeleccionada);
                    }
                    else
                    {
                        // Si ocurrió un error en la capa de datos (ej. dependencias con productos), mostramos el mensaje correspondiente.
                        MessageBox.Show(mensaje, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
    }
}