using System.Collections.Generic; // Importa las colecciones genéricas estándar de C#.
using System.Collections.ObjectModel; // Importa ObservableCollection, que avisa automáticamente a la UI cuando cambian los elementos de la lista.
using System.Windows; // Importa clases base de WPF como MessageBox y RoutedEventArgs.
using System.Windows.Controls; // Importa controles de interfaz como UserControl y DataGrid.
using TillasDesktop.UI.Modelos;// Importa los modelos del proyecto (como la clase Cliente).
using TillasDesktop.Entities.Usuarios;

namespace TillasDesktop.UI.Vistas
{
    // Clase que maneja la lógica detrás de la vista de gestión de clientes. Hereda de UserControl.
    public partial class ClientesView : UserControl
    {
        // Colección observable global que almacena la lista de clientes vinculada a la interfaz. Puede ser nula inicialmente (?).
        public ObservableCollection<ClienteViewModel> ListaClientes { get; set; } = new();
        private ObservableCollection<ClienteViewModel> ListaClientesCompleta = new();

        // Constructor de la vista: se ejecuta al inicializar el componente y carga los datos de prueba.
        public ClientesView()
        {
            InitializeComponent();
            CargarClientesPrueba();
        }

        // Método privado para rellenar la lista de clientes con datos iniciales estáticos.
        private void CargarClientesPrueba()
        {
            // Inicializa la colección observable con tres clientes de ejemplo precargados.
            ListaClientesCompleta = new ObservableCollection<ClienteViewModel>
            {
                new ClienteViewModel { Id = 101, Nombre = "Carlos", Apellido = "Rodríguez", DNI = "15.223.102", CUIT = "27-22334455-8", Telefono = "+54 379 455-1122", Email = "carlos.rod@mail.com" },
                new ClienteViewModel { Id = 102, Nombre = "María", Apellido = "Gómez", DNI = "15.223.103", CUIT = "27-22334455-8", Telefono = "+54 379 511-9988", Email = "maria.g@mail.com" },
                new ClienteViewModel { Id = 103, Nombre = "María", Apellido = "Gómez", DNI = "15.223.103", CUIT = "27-22334455-8", Telefono = "+54 379 511-9988", Email = "maria.m@mail.com" }
            };

            // Conecta la colección directamente al origen de datos (ItemsSource) del DataGrid visual.
            ListaClientes = new ObservableCollection<ClienteViewModel>(ListaClientesCompleta);
            dgClientes.ItemsSource = ListaClientes;
        }

        private void TxtBuscar_TextChanged(object sender, TextChangedEventArgs e)
        {
            ActualizarVistaTabla();
        }

        private void ActualizarVistaTabla()
        {
            string filtro = txtBuscar.Text.ToLower().Trim();

            if (string.IsNullOrEmpty(filtro))
            {
                // Si no hay texto de búsqueda, mostramos toda la lista maestra
                dgClientes.ItemsSource = null;
                dgClientes.ItemsSource = ListaClientesCompleta;
            }
            else
            {
                // Si hay filtro, filtramos sobre la lista completa
                var resultado = ListaClientesCompleta.Where(c =>
                    (c.Nombre != null && c.Nombre.ToLower().Contains(filtro)) ||
                    (c.Apellido != null && c.Apellido.ToLower().Contains(filtro)) ||
                    (c.DNI != null && c.DNI.Contains(filtro)) ||
                    (c.CUIT != null && c.CUIT.Contains(filtro))
                ).ToList();

                dgClientes.ItemsSource = null;
                dgClientes.ItemsSource = resultado;
            }
        }

        // Evento que se dispara al hacer clic en el botón "Agregar Cliente".
        private void BtnAgregarCliente_Click(object sender, RoutedEventArgs e)
        {
            // Instancia la ventana emergente del formulario en modo de creación.
            FormularioClienteWindow formulario = new FormularioClienteWindow();

            // Abre el formulario de forma modal (bloquea la ventana principal hasta cerrarlo) y captura si el usuario presionó guardar (true).
            bool? resultado = formulario.ShowDialog();

            // Si el resultado fue exitoso y el cliente creado no es nulo...
            if (resultado == true && formulario.NuevoCliente != null)
            {

                // Calcula el ID dinámicamente basado en la lista completa
                int nuevoId = ListaClientesCompleta.Count > 0 ? ListaClientesCompleta.Max(c => c.Id) + 1 : 1;
                formulario.NuevoCliente.Id = nuevoId;

                // Lo agregamos a la lista maestra
                ListaClientesCompleta.Add(formulario.NuevoCliente);

                // Limpiamos el buscador para que se muestre toda la lista (incluyendo el nuevo)
                txtBuscar.Text = string.Empty;

                // Refresca la tabla llamando al método centralizado
                ActualizarVistaTabla();
            }
        }

        // Evento que se dispara al hacer clic en el botón de editar de una fila.
        private void BtnEditar_Click(object sender, RoutedEventArgs e)
        {
            // Obtiene el objeto de la fila seleccionada actualmente en el DataGrid y lo convierte a tipo Cliente.
            ClienteViewModel clienteSeleccionado = (ClienteViewModel)dgClientes.SelectedItem;

            // Verifica que realmente se haya seleccionado un cliente.
            if (clienteSeleccionado != null)
            {
                FormularioClienteWindow formulario = new FormularioClienteWindow(clienteSeleccionado);
                bool? resultado = formulario.ShowDialog();

                if (resultado == true)
                {
                    dgClientes.Items.Refresh();
                }
            }
            else
            {
                MessageBox.Show("Selecciona un cliente de la tabla para editar.", "Aviso");
            }
        }

        // Evento que se dispara al hacer clic en el botón de eliminar de una fila.
        private void BtnEliminar_Click(object sender, RoutedEventArgs e)
        {
            // Obtiene el cliente seleccionado de la fila de la tabla.
            ClienteViewModel clienteSeleccionado = (ClienteViewModel)dgClientes.SelectedItem;

            // Valida que exista un cliente seleccionado.
            if (clienteSeleccionado != null)
            {
                // Muestra una ventana de confirmación antes de proceder a borrar el registro.
                MessageBoxResult resultado = MessageBox.Show($"¿Desea eliminar a {clienteSeleccionado.Nombre} {clienteSeleccionado.Apellido }?", "Confirmar eliminación", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                // Si el usuario confirma haciendo clic en "Sí"...
                if (resultado == MessageBoxResult.Yes)
                {
                    // Remueve el cliente directamente de la colección observable, eliminándolo de la tabla al instante.
                    ListaClientesCompleta.Remove(clienteSeleccionado);
                    TxtBuscar_TextChanged(txtBuscar, null!);
                }
            }
            else
            {
                // Muestra un aviso si se intentó eliminar sin seleccionar ningún elemento.
                MessageBox.Show("Selecciona un cliente para eliminar.", "Aviso");
            }
        }
    }
}