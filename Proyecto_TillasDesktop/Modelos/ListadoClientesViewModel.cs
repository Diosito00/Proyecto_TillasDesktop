using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection.Metadata;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using TillasDesktop.UI.Vistas;

namespace TillasDesktop.UI.Modelos
{
    public class ListadoClientesViewModel : ViewModelBase
    {
        private ObservableCollection<ClienteViewModel> _listaClientes;
        private ClienteViewModel _clienteSeleccionado;
        private string _textoBusqueda = string.Empty;

        public ObservableCollection<ClienteViewModel> ListaClientes
        {
            get => _listaClientes;
            set { _listaClientes = value; OnPropertyChanged(); }
        }

        public ICollectionView VistaFiltroClientes { get; set; }

        public string TextoBusqueda
        {
            get => _textoBusqueda;
            set
            {
                _textoBusqueda = value;
                OnPropertyChanged();
                VistaFiltroClientes?.Refresh();
            }
        }

        public ClienteViewModel ClienteSeleccionado
        {
            get => _clienteSeleccionado;
            set { _clienteSeleccionado = value; OnPropertyChanged(); }
        }

        public ICommand NuevoClienteCommand { get; }
        public ICommand EditarCommand { get; }
        public ICommand EliminarCommand { get; }

        public ListadoClientesViewModel()
        {
            CargarDatos();

            // Configuración del filtro de búsqueda
            VistaFiltroClientes = CollectionViewSource.GetDefaultView(ListaClientes);
            VistaFiltroClientes.Filter = FiltrarCriteriosClientes;

            NuevoClienteCommand = new RelayCommand(EjecutarNuevo);
            EditarCommand = new RelayCommand(EjecutarEditar, CanExecuteSeleccionado);
            EliminarCommand = new RelayCommand(EjecutarEliminar, CanExecuteSeleccionado);
        }

        private void CargarDatos()
        {
            ListaClientes = new ObservableCollection<ClienteViewModel>
            {
                new ClienteViewModel { Id = 101, Nombre = "Carlos", Apellido = "Rodríguez", CUIT = "23457268992", Telefono = "379 4551122", Email = "carlos.rod@mail.com" },
                new ClienteViewModel { Id = 102, Nombre = "María", Apellido = "Gómez", CUIT = "27223344558", Telefono = "379 5119988", Email = "maria.g@mail.com" }
            };
        }

        private bool FiltrarCriteriosClientes(object obj)
        {
            if (obj is ClienteViewModel cliente)
            {
                if (string.IsNullOrWhiteSpace(TextoBusqueda)) return true;

                string filtro = TextoBusqueda.ToLower();
                return (cliente.Nombre != null && cliente.Nombre.ToLower().Contains(filtro)) ||
                       (cliente.Apellido != null && cliente.Apellido.ToLower().Contains(filtro)) ||
                       (cliente.Email != null && cliente.Email.ToLower().Contains(filtro)) ||
                       (cliente.CUIT != null && cliente.CUIT.ToLower().Contains(filtro));
            }
            return false;
        }

        private void EjecutarNuevo(object obj)
        {
            FormularioClienteWindow ventana = new FormularioClienteWindow();

            if (ventana.ShowDialog() == true && ventana.NuevoCliente != null)
            {
                int nuevoId = ListaClientes.Count > 0 ? ListaClientes.Max(c => c.Id) + 1 : 1;
                ventana.NuevoCliente.Id = nuevoId;

                ListaClientes.Add(ventana.NuevoCliente);
                VistaFiltroClientes.Refresh();
            }
        }

        private void EjecutarEditar(object obj)
        {
            if (ClienteSeleccionado != null)
            {
                FormularioClienteWindow ventana = new FormularioClienteWindow(ClienteSeleccionado);

                if (ventana.ShowDialog() == true)
                {
                    // Refresca la vista si se editó el elemento
                    VistaFiltroClientes.Refresh();
                }
            }
        }

        private void EjecutarEliminar(object obj)
        {
            if (obj is ClienteViewModel clienteFila)
            {
                var respuesta = MessageBox.Show($"¿Estás seguro de que deseas eliminar permanentemente el cliente '{clienteFila.Nombre} {clienteFila.Apellido}'?",
                                                "Confirmar Eliminación",
                                                MessageBoxButton.YesNo,
                                                MessageBoxImage.Warning);

                if (respuesta == MessageBoxResult.Yes)
                {
                    
                    ListaClientes.Remove(clienteFila);
                    VistaFiltroClientes.Refresh();
                }
            }
        }

        private bool CanExecuteSeleccionado(object obj)
        {
            return ClienteSeleccionado != null;
        }
    }
}