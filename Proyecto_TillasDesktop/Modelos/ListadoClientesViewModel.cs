using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection.Metadata;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using TillasDesktop.Entities.Usuarios;
using TillasDesktop.UI.Vistas;

namespace TillasDesktop.UI.Modelos
{
    // Hereda de ViewModelBase para mantener la pantalla sincronizada con los datos mediante notificaciones automáticas.
    public class ListadoClientesViewModel : ViewModelBase
    {
        // Colección privada que almacena todos los clientes cargados.
        private ObservableCollection<ClienteViewModel> _listaClientes;

        // Almacena el cliente que el usuario seleccionó actualmente en la tabla.
        private ClienteViewModel _clienteSeleccionado;

        // Texto que el operador va escribiendo en la barra de búsqueda para filtrar la tabla.
        private string _textoBusqueda = string.Empty;

        // Propiedad pública para la lista de clientes; avisa a la pantalla cuando cambia por completo.
        public ObservableCollection<ClienteViewModel> ListaClientes
        {
            get => _listaClientes;
            set { _listaClientes = value; OnPropertyChanged(); }
        }

        // Vista especial de colección que permite filtrar y ordenar los clientes sin modificar la lista original.
        public ICollectionView VistaFiltroClientes { get; set; }

        // Propiedad vinculada al cuadro de texto de búsqueda de la interfaz.
        public string TextoBusqueda
        {
            get => _textoBusqueda;
            set
            {
                _textoBusqueda = value;
                OnPropertyChanged();
                // Cada vez que cambia el texto, le avisamos a la vista que vuelva a evaluar el filtro renglón por renglón.
                VistaFiltroClientes?.Refresh();
            }
        }

        // Propiedad que guarda el cliente seleccionado en la tabla. Al cambiar, avisa para actualizar botones dependientes.
        public ClienteViewModel ClienteSeleccionado
        {
            get => _clienteSeleccionado;
            set { _clienteSeleccionado = value; OnPropertyChanged(); }
        }

        // Comandos asociados a los botones principales de la interfaz (nuevo, editar, eliminar).
        public ICommand NuevoClienteCommand { get; }
        public ICommand EditarCommand { get; }
        public ICommand EliminarCommand { get; }

        // Constructor principal: carga los datos, configura el filtro y asigna los comandos.
        public ListadoClientesViewModel()
        {
            CargarDatos();

            // Configuramos la vista de filtrado utilizando la lista observable de clientes como base.
            VistaFiltroClientes = CollectionViewSource.GetDefaultView(ListaClientes);

            // Asignamos el método que decide qué clientes mostrar según lo que se escriba en el buscador.
            VistaFiltroClientes.Filter = FiltrarCriteriosClientes;

            // Vinculamos cada comando con su método correspondiente. 
            // Los de editar y eliminar llevan un validador (CanExecuteSeleccionado) para que el botón esté desactivado si no hay nadie seleccionado.
            NuevoClienteCommand = new RelayCommand(EjecutarNuevo);
            EditarCommand = new RelayCommand(EjecutarEditar, CanExecuteSeleccionado);
            EliminarCommand = new RelayCommand(EjecutarEliminar, CanExecuteSeleccionado);
        }

        // Carga inicial de datos de prueba para poder maquetar y probar la tabla de clientes.
        private void CargarDatos()
        {
            ListaClientes = new ObservableCollection<ClienteViewModel>
            {
                new ClienteViewModel { Id = 101, Nombre = "Carlos", Apellido = "Rodríguez", CUIT = "23457268992", Telefono = "379 4551122", Email = "carlos.rod@mail.com" },
                new ClienteViewModel { Id = 102, Nombre = "María", Apellido = "Gómez", CUIT = "27223344558", Telefono = "379 5119988", Email = "maria.g@mail.com" }
            };
        }

        // Método que evalúa renglón por renglón si el cliente coincide con lo escrito en la barra de búsqueda.
        private bool FiltrarCriteriosClientes(object obj)
        {
            if (obj is ClienteViewModel cliente)
            {
                // Si la barra está vacía, dejamos pasar a todos los clientes sin filtrar.
                if (string.IsNullOrWhiteSpace(TextoBusqueda)) return true;

                // Pasamos el texto a minúsculas para que no importe si buscan con mayúsculas o minúsculas.
                string filtro = TextoBusqueda.ToLower();

                // Devuelve true (muestra la fila) si el filtro coincide con el Nombre, Apellido, Email o CUIT.
                return (cliente.Nombre != null && cliente.Nombre.ToLower().Contains(filtro)) ||
                       (cliente.Apellido != null && cliente.Apellido.ToLower().Contains(filtro)) ||
                       (cliente.Email != null && cliente.Email.ToLower().Contains(filtro)) ||
                       (cliente.CUIT != null && cliente.CUIT.ToLower().Contains(filtro));
            }
            return false; // Si no es un cliente válido, lo ocultamos.
        }

        // Abre el formulario para dar de alta un cliente nuevo.
        private void EjecutarNuevo(object obj)
        {
            FormularioClienteWindow ventana = new FormularioClienteWindow();

            // Si el usuario completa el formulario y hace clic en guardar (DialogResult == true)
            if (ventana.ShowDialog() == true && ventana.NuevoCliente != null)
            {
                // Calculamos un ID autoincremental buscando el mayor ID actual y sumándole 1 (o arrancamos en 1 si está vacía).
                int nuevoId = ListaClientes.Count > 0 ? ListaClientes.Max(c => c.Id) + 1 : 1;
                ventana.NuevoCliente.Id = nuevoId;

                // Agregamos el cliente nuevo a la lista y refrescamos la tabla para que aparezca al toque.
                ListaClientes.Add(ventana.NuevoCliente);
                VistaFiltroClientes.Refresh();
            }
        }

        // Abre el formulario cargando los datos del cliente que seleccionamos para modificarlo.
        private void EjecutarEditar(object obj)
        {
            if (ClienteSeleccionado != null)
            {
                FormularioClienteWindow ventana = new FormularioClienteWindow(ClienteSeleccionado);

                if (ventana.ShowDialog() == true)
                {
                    // Si se guardaron los cambios, refrescamos la vista para reflejar las modificaciones en la grilla.
                    VistaFiltroClientes.Refresh();
                }
            }
        }

        // Da de baja al cliente seleccionado tras pedir una confirmación por seguridad.
        private void EjecutarEliminar(object obj)
        {
            // El parámetro puede venir directo del botón de la fila en la tabla.
            if (obj is ClienteViewModel clienteFila)
            {
                // Ventana emergente para evitar que borren un cliente por error.
                var respuesta = MessageBox.Show($"¿Estás seguro de que deseas eliminar permanentemente el cliente '{clienteFila.Nombre} {clienteFila.Apellido}'?",
                                                "Confirmar Eliminación",
                                                MessageBoxButton.YesNo,
                                                MessageBoxImage.Warning);

                if (respuesta == MessageBoxResult.Yes)
                {
                    // Lo sacamos de nuestra lista observable y la tabla se actualiza sola.
                    ListaClientes.Remove(clienteFila);
                    VistaFiltroClientes.Refresh();
                }
                    
                }
         }

        // Método de validación que habilita o desgrisa los botones de Editar y Eliminar únicamente si hay un cliente seleccionado en la tabla.
        private bool CanExecuteSeleccionado(object obj)
        {
            return ClienteSeleccionado != null;
        }
    }
}