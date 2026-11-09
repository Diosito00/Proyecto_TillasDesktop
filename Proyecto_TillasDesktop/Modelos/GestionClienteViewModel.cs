// Importa el espacio de nombres fundamental de .NET para tipos base y manejo de excepciones
using System;
// Importa las colecciones observables de WPF para actualizar automáticamente las grillas (DataGrid) al añadir o quitar elementos
using System.Collections.ObjectModel;
// Importa las interfaces de componentes y soporte de metadatos (necesario para ICollectionView)
using System.ComponentModel;
// Importa las extensiones de LINQ para consultas y operaciones sobre colecciones en memoria
using System.Linq;
// Importa las clases base de WPF para la gestión de la aplicación y cuadros de diálogo interactivos
using System.Windows;
// Importa las vistas de colección de WPF para permitir filtrado y ordenamiento avanzado sin romper los datos originales
using System.Windows.Data;
// Importa las utilidades de comandos y eventos de entrada de la interfaz de usuario en WPF
using System.Windows.Input;
// Importa las clases de la Capa de Negocios
using TillasDesktop.BLL;
// Importa los servicios de la capa de negocios dedicados a la gestión de clientes
using TillasDesktop.BLL.Services;
// Importa las entidades del dominio de clientes
using TillasDesktop.Entities.Clientes;
// Importa las vistas de la interfaz de usuario de la aplicación (formularios de cliente)
using TillasDesktop.UI.Vistas;

// Define el espacio de nombres (namespace) correspondiente a los modelos de vista (ViewModels) de la interfaz de usuario del sistema
namespace TillasDesktop.UI.Modelos
{
    // Declara la clase pública GestionClienteViewModel que hereda de ViewModelBase, encargada de administrar el listado, filtrado, altas, ediciones y bajas de clientes.
    public class GestionClienteViewModel : ViewModelBase
    {
        // Instancia de solo lectura del servicio que contiene las reglas de negocio y la conexión a datos de clientes.
        private readonly ClienteService _clienteService;

        // Campo privado que almacena la colección observable de clientes envueltos en ViewModels.
        private ObservableCollection<ClienteViewModel> _listaClientes;
        // Propiedad pública ObservableCollection: notifica automáticamente a la grilla de la interfaz cuando un elemento se añade o se elimina.
        public ObservableCollection<ClienteViewModel> ListaClientes
        {
            get => _listaClientes;
            set { _listaClientes = value; OnPropertyChanged(); }
        }

        // Campo privado que almacena el texto ingresado en la barra de búsqueda en tiempo real.
        private string _textoBusqueda = string.Empty;
        // Propiedad pública bindeada a la barra de búsqueda de la interfaz. Al cambiar, refresca la vista filtrada automáticamente.
        public string TextoBusqueda
        {
            get => _textoBusqueda;
            set
            {
                _textoBusqueda = value;
                OnPropertyChanged();
                VistaFiltroClientes?.Refresh(); // Dispara la reevaluación del método de filtrado por cada fila en tiempo real.
            }
        }

        // Una vista especial (ICollectionView) que envuelve nuestra lista para poder filtrar y ordenar dinámicamente sin alterar los datos originales subyacentes.
        public ICollectionView VistaFiltroClientes { get; set; }

        // Comandos públicos vinculados a los botones de la interfaz para las acciones principales.
        public ICommand NuevoClienteCommand { get; }
        public ICommand EditarCommand { get; }
        public ICommand EliminarCommand { get; }

        // Constructor principal que inicializa el ViewModel, servicios, vistas de filtrado y comandos.
        public GestionClienteViewModel()
        {
            // Instancia el servicio de clientes de la capa de negocios
            _clienteService = new ClienteService();

            // Configura la vista de filtrado basándose en la lista observable principal.
            ListaClientes = new ObservableCollection<ClienteViewModel>();
            VistaFiltroClientes = CollectionViewSource.GetDefaultView(ListaClientes);
            VistaFiltroClientes.Filter = FiltrarCriteriosClientes; // Asigna el método delegado que evalúa fila por fila.

            // Carga los datos iniciales desde la base de datos
            CargarDatos();

            // Vinculamos cada comando con su respectivo método delegado de acción.
            NuevoClienteCommand = new RelayCommand(EjecutarNuevo);
            EditarCommand = new RelayCommand(EjecutarEditar);
            EliminarCommand = new RelayCommand(EjecutarEliminar);
        }

        // Método privado que trae los clientes desde la base de datos y los envuelve para prepararlos en la pantalla.
        private void CargarDatos()
        {
            try
            {
                // Limpia la colección existente por seguridad
                ListaClientes.Clear();

                // Pide todas las entidades de clientes a la capa de servicios.
                var clientesDeDb = _clienteService.ObtenerClientes();

                // Envolvemos cada cliente en un ClienteViewModel para que la interfaz pueda mostrarlos y enlazarlos correctamente.
                foreach (var cliente in clientesDeDb)
                {
                    ListaClientes.Add(new ClienteViewModel(cliente));
                }

                // Refresca la vista de filtrado para reflejar los nuevos datos cargados
                VistaFiltroClientes?.Refresh();
            }
            catch (Exception ex)
            {
                // Muestra un mensaje de error detallado si falla la conexión o la consulta a la base de datos
                MessageBox.Show($"Error al cargar los clientes desde la base de datos: {ex.Message}",
                                "Error de Conexión", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Método delegado ejecutado por la vista de filtrado por cada fila de la grilla de datos.
        private bool FiltrarCriteriosClientes(object obj)
        {
            // Verifica que el objeto recibido corresponda a un ClienteViewModel válido
            if (obj is ClienteViewModel clienteFila)
            {
                // Si el buscador está vacío o en blanco, muestra todas las filas sin restricciones.
                if (string.IsNullOrWhiteSpace(TextoBusqueda)) return true;

                // Convierte la cadena de búsqueda a minúsculas para garantizar que no sea sensible a mayúsculas/minúsculas.
                string filtro = TextoBusqueda.ToLower();

                // Retorna true si el texto de búsqueda coincide de forma parcial con el nombre, apellido, email o CUIT del cliente.
                return (clienteFila.Nombre != null && clienteFila.Nombre.ToLower().Contains(filtro)) ||
                       (clienteFila.Apellido != null && clienteFila.Apellido.ToLower().Contains(filtro)) ||
                       (clienteFila.Email != null && clienteFila.Email.ToLower().Contains(filtro)) ||
                       (clienteFila.CUIT != null && clienteFila.CUIT.ToLower().Contains(filtro));
            }
            return false; // Oculta la fila de la grilla si no hay coincidencias o el objeto no es válido.
        }

        // Método privado que abre el formulario en blanco para dar de alta un cliente nuevo.
        private void EjecutarNuevo(object obj)
        {
            // Instancia la ventana visual del formulario y el ViewModel correspondiente para un nuevo registro
            var ventana = new FormularioClienteWindow();
            var viewModel = new FormularioClienteViewModel();

            // Configura acciones (delegados) para que el formulario se pueda cerrar de forma limpia y avise cuando guarde con éxito.
            viewModel.CerrarVentana = () => ventana.Close();
            viewModel.OnClienteGuardado = () =>
            {
                // Agrega el nuevo cliente a la colección observable de inmediato para reflejarlo en la grilla principal.
                ListaClientes.Add(viewModel.ClienteActual);
                VistaFiltroClientes.Refresh();
            };

            // Asigna el DataContext y abre la ventana en modalidad modal bloqueante
            ventana.DataContext = viewModel;
            ventana.ShowDialog();
        }

        // Método privado que abre el formulario cargando los datos del cliente seleccionado en la grilla para permitir su modificación.
        private void EjecutarEditar(object obj)
        {
            // El objeto llega desde el botón de la fila en la interfaz. Nos aseguramos de que sea un ClienteViewModel válido.
            if (obj is ClienteViewModel clienteFila)
            {
                // Extrae la entidad pura subyacente para obtener los datos originales.
                var entidadOriginal = clienteFila.ObtenerEntidadPura();

                // TÉCNICA DE CLONACIÓN: Se crea una instancia completamente nueva con los mismos valores de propiedad.
                // Esto previene la "edición fantasma" en la interfaz si el usuario decide cancelar la operación a mitad de camino.
                var entidadClonada = new Cliente
                {
                    ID = entidadOriginal.ID,
                    Nombre = entidadOriginal.Nombre,
                    Apellido = entidadOriginal.Apellido,
                    CUIT = entidadOriginal.CUIT,
                    Telefono = entidadOriginal.Telefono,
                    Email = entidadOriginal.Email
                };

                // Instancia la ventana de formulario
                var ventana = new FormularioClienteWindow();

                // Se le pasa el CLON al ViewModel del formulario en lugar de la entidad original para aislar los cambios temporales.
                var viewModel = new FormularioClienteViewModel(entidadClonada);

                // Configura el delegado para cerrar la ventana
                viewModel.CerrarVentana = () => ventana.Close();

                // Configura el callback que se ejecuta al confirmar el guardado exitoso
                viewModel.OnClienteGuardado = () =>
                {
                    // Si la base de datos confirmó el éxito, actualizamos manualmente las propiedades de la fila original
                    // disparando OnPropertyChanged() a través de las asignaciones para que la grilla principal se repinte al instante.
                    clienteFila.Nombre = entidadClonada.Nombre;
                    clienteFila.Apellido = entidadClonada.Apellido;
                    clienteFila.CUIT = entidadClonada.CUIT;
                    clienteFila.Telefono = entidadClonada.Telefono;
                    clienteFila.Email = entidadClonada.Email;

                    // Refresca la vista de filtrado
                    VistaFiltroClientes.Refresh();
                };

                // Asigna el DataContext y abre la ventana modal de edición
                ventana.DataContext = viewModel;
                ventana.ShowDialog();
            }
        }

        // Método privado que da de baja al cliente seleccionado tras solicitar una confirmación explícita por seguridad.
        private void EjecutarEliminar(object obj)
        {
            // Valida que el parámetro recibido corresponda a un cliente válido de la grilla
            if (obj is ClienteViewModel clienteFila)
            {
                // Muestra un cuadro de diálogo de advertencia pidiendo confirmación al operador
                var respuesta = MessageBox.Show($"¿Estás seguro de que deseas dar de baja al cliente '{clienteFila.Nombre} {clienteFila.Apellido}'?",
                                                "Confirmar Baja",
                                                MessageBoxButton.YesNo,
                                                MessageBoxImage.Warning);

                // Si el usuario responde que SÍ...
                if (respuesta == MessageBoxResult.Yes)
                {
                    try
                    {
                        // Ejecuta la eliminación directamente en la capa de servicios (si ocurre una restricción o error, saltará al bloque catch)
                        _clienteService.Eliminar(clienteFila.ID);

                        // Si se elimina correctamente de la base de datos, lo quitamos de la colección observable local y refrescamos la vista
                        ListaClientes.Remove(clienteFila);
                        VistaFiltroClientes.Refresh();
                    }
                    catch (Exception ex)
                    {
                        // Muestra un mensaje de advertencia o error si la baja no pudo completarse (ej. dependencias con ventas existentes)
                        MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
    }
}