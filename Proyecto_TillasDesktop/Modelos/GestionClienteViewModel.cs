using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using TillasDesktop.BLL;
using TillasDesktop.BLL.Services;
using TillasDesktop.Entities.Clientes;
using TillasDesktop.UI.Vistas;

namespace TillasDesktop.UI.Modelos
{
    // Hereda de ViewModelBase para mantener la interfaz conectada y reactiva (avisando cuando cambian los datos).
    public class GestionClienteViewModel : ViewModelBase
    {
        // Instancia del servicio que contiene las reglas de negocio y conexión a datos de clientes.
        private readonly ClienteService _clienteService;

        // ObservableCollection notifica automáticamente a la grilla de la interfaz cuando un elemento se añade o elimina.
        private ObservableCollection<ClienteViewModel> _listaClientes;
        public ObservableCollection<ClienteViewModel> ListaClientes
        {
            get => _listaClientes;
            set { _listaClientes = value; OnPropertyChanged(); }
        }

        // Propiedad bindeada a la barra de búsqueda. Al cambiar, refresca la vista filtrada automáticamente.
        private string _textoBusqueda = string.Empty;
        public string TextoBusqueda
        {
            get => _textoBusqueda;
            set
            {
                _textoBusqueda = value;
                OnPropertyChanged();
                VistaFiltroClientes?.Refresh(); // Dispara la reevaluación del filtro.
            }
        }

        // Una vista especial que envuelve nuestra lista para poder filtrar y ordenar sin romper los datos originales.
        public ICollectionView VistaFiltroClientes { get; set; }

        // Comandos vinculados a los botones de la interfaz.
        public ICommand NuevoClienteCommand { get; }
        public ICommand EditarCommand { get; }
        public ICommand EliminarCommand { get; }

        public GestionClienteViewModel()
        {
            _clienteService = new ClienteService();

            // Configura la vista de filtrado basándose en la lista observable principal.
            ListaClientes = new ObservableCollection<ClienteViewModel>();
            VistaFiltroClientes = CollectionViewSource.GetDefaultView(ListaClientes);
            VistaFiltroClientes.Filter = FiltrarCriteriosClientes; // Asigna el método que evalúa cada fila.

            CargarDatos();

            // Vinculamos cada comando con su respectivo método.
            NuevoClienteCommand = new RelayCommand(EjecutarNuevo);
            EditarCommand = new RelayCommand(EjecutarEditar);
            EliminarCommand = new RelayCommand(EjecutarEliminar);
        }

        // Trae los clientes de la base de datos y los prepara para la pantalla.
        private void CargarDatos()
        {
            try
            {
                ListaClientes.Clear();

                // Pide todas las entidades a la capa de servicios.
                var clientesDeDb = _clienteService.ObtenerClientes();

                // Envolvemos cada cliente en un ViewModel para que la interfaz pueda mostrarlos y editarlos bien.
                foreach (var cliente in clientesDeDb)
                {
                    ListaClientes.Add(new ClienteViewModel(cliente));
                }

                VistaFiltroClientes?.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar los clientes desde la base de datos: {ex.Message}",
                                "Error de Conexión", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Método ejecutado por la vista de filtrado por cada fila de la grilla.
        private bool FiltrarCriteriosClientes(object obj)
        {
            if (obj is ClienteViewModel clienteFila)
            {
                // Si el buscador está vacío, muestra todas las filas.
                if (string.IsNullOrWhiteSpace(TextoBusqueda)) return true;

                // Convierte la búsqueda a minúsculas para que no sea sensible a mayúsculas.
                string filtro = TextoBusqueda.ToLower();

                // Retorna true si el texto coincide con el nombre, apellido, email o CUIT.
                return (clienteFila.Nombre != null && clienteFila.Nombre.ToLower().Contains(filtro)) ||
                       (clienteFila.Apellido != null && clienteFila.Apellido.ToLower().Contains(filtro)) ||
                       (clienteFila.Email != null && clienteFila.Email.ToLower().Contains(filtro)) ||
                       (clienteFila.CUIT != null && clienteFila.CUIT.ToLower().Contains(filtro));
            }
            return false; // Oculta la fila si no hay coincidencias.
        }

        // Abre el formulario en blanco para dar de alta un cliente nuevo.
        private void EjecutarNuevo(object obj)
        {
            var ventana = new FormularioClienteWindow();
            var viewModel = new FormularioClienteViewModel();

            // Acciones para que el formulario se pueda cerrar solo y avise cuando guarde con éxito.
            viewModel.CerrarVentana = () => ventana.Close();
            viewModel.OnClienteGuardado = () =>
            {
                // Agrega el nuevo cliente a la colección observable de inmediato.
                ListaClientes.Add(viewModel.ClienteActual);
                VistaFiltroClientes.Refresh();
            };

            ventana.DataContext = viewModel;
            ventana.ShowDialog(); // Abre la ventana y frena la ejecución hasta que la cierren.
        }

        // Abre el formulario cargando los datos del cliente que seleccionamos en la grilla para modificarlo.
        private void EjecutarEditar(object obj)
        {
            // El objeto viene del botón de la fila en la interfaz. Nos aseguramos que sea un cliente válido.
            if (obj is ClienteViewModel clienteFila)
            {
                // Extrae la entidad pura para tener los datos originales.
                var entidadOriginal = clienteFila.ObtenerEntidadPura();

                // CLONACIÓN: Se crea una instancia completamente nueva con los mismos datos.
                // Esto previene la "edición fantasma" en la interfaz si el usuario cancela la operación.
                var entidadClonada = new Cliente
                {
                    ID = entidadOriginal.ID,
                    Nombre = entidadOriginal.Nombre,
                    Apellido = entidadOriginal.Apellido,
                    CUIT = entidadOriginal.CUIT,
                    Telefono = entidadOriginal.Telefono,
                    Email = entidadOriginal.Email
                };

                var ventana = new FormularioClienteWindow();

                // Se le pasa el CLON al formulario en lugar de la entidad original.
                var viewModel = new FormularioClienteViewModel(entidadClonada);

                viewModel.CerrarVentana = () => ventana.Close();

                viewModel.OnClienteGuardado = () =>
                {
                    // Si la base de datos confirma el éxito, actualizamos las propiedades de la fila original
                    // disparando OnPropertyChanged() para que la grilla se repinte al instante.
                    clienteFila.Nombre = entidadClonada.Nombre;
                    clienteFila.Apellido = entidadClonada.Apellido;
                    clienteFila.CUIT = entidadClonada.CUIT;
                    clienteFila.Telefono = entidadClonada.Telefono;
                    clienteFila.Email = entidadClonada.Email;

                    VistaFiltroClientes.Refresh();
                };

                ventana.DataContext = viewModel;
                ventana.ShowDialog();
            }
        }

        // Da de baja al cliente seleccionado tras pedir una confirmación por seguridad.
        private void EjecutarEliminar(object obj)
        {
            if (obj is ClienteViewModel clienteFila)
            {
                var respuesta = MessageBox.Show($"¿Estás seguro de que deseas eliminar permanentemente al cliente '{clienteFila.Nombre} {clienteFila.Apellido}'?",
                                                "Confirmar Eliminación",
                                                MessageBoxButton.YesNo,
                                                MessageBoxImage.Warning);

                if (respuesta == MessageBoxResult.Yes)
                {
                    try
                    {
                        // Ejecutamos la eliminación directamente (si falla, saltará al catch)
                        _clienteService.Eliminar(clienteFila.ID);
                        
                            ListaClientes.Remove(clienteFila);
                            VistaFiltroClientes.Refresh();
                        
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
    }
}