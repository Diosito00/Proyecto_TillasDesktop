using System.Collections.Generic; // Importa las colecciones genéricas estándar de C#.
using System.Windows; // Importa clases base de WPF como MessageBox y RoutedEventArgs.
using System.Windows.Controls; // Importa controles de interfaz como UserControl y DataGrid.
using TillasDesktop.UI.Modelos; // Importa los modelos del proyecto (como la clase Cliente).
using System.Collections.ObjectModel; // Importa ObservableCollection, que avisa automáticamente a la UI cuando cambian los elementos de la lista.

namespace TillasDesktop.UI.Vistas
{
    // Clase que maneja la lógica detrás de la vista de gestión de clientes. Hereda de UserControl.
    public partial class ClientesView : UserControl
    {
        // Colección observable global que almacena la lista de clientes vinculada a la interfaz. Puede ser nula inicialmente (?).
        public ObservableCollection<Cliente>? ListaClientes { get; set; }

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
            ListaClientes = new ObservableCollection<Cliente>
            {
                new Cliente { Id = 101, NombreCompleto = "Carlos Alberto Rodríguez", Documento = "DNI 15.223.102", Telefono = "+54 379 455-1122", Email = "carlos.rod@mail.com" },
                new Cliente { Id = 102, NombreCompleto = "María Elena Gómez", Documento = "CUIT 27-22334455-8", Telefono = "+54 379 511-9988", Email = "maria.g@mail.com" },
                new Cliente { Id = 103, NombreCompleto = "María Gómez", Documento = "DNI 15.223.103", Telefono = "+54 379 511-9988", Email = "maria.m@mail.com" }
            };

            // Conecta la colección directamente al origen de datos (ItemsSource) del DataGrid visual.
            dgClientes.ItemsSource = ListaClientes;
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
                // Se añade el nuevo cliente a la lista observable para que aparezca automáticamente en la tabla.
                ListaClientes.Add(formulario.NuevoCliente);
            }
        }

        // Evento que se dispara al hacer clic en el botón de editar de una fila.
        private void BtnEditar_Click(object sender, RoutedEventArgs e)
        {
            // Obtiene el objeto de la fila seleccionada actualmente en el DataGrid y lo convierte a tipo Cliente.
            Cliente clienteSeleccionado = (Cliente)dgClientes.SelectedItem;

            // Verifica que realmente se haya seleccionado un cliente.
            if (clienteSeleccionado != null)
            {
                // Guarda la posición (índice) que ocupa el cliente seleccionado dentro de la colección.
                int index = ListaClientes.IndexOf(clienteSeleccionado);

                // Abre el formulario pasándole el cliente existente para rellenar sus datos actuales.
                FormularioClienteWindow formulario = new FormularioClienteWindow(clienteSeleccionado);
                bool? resultado = formulario.ShowDialog();

                // Si se guardaron los cambios, el cliente no es nulo y el índice es válido...
                if (resultado == true && formulario.NuevoCliente != null && index != -1)
                {
                    // Reemplaza el elemento en la misma posición de la lista observable para actualizar los datos.
                    ListaClientes[index] = formulario.NuevoCliente;

                    // Fuerza de manera visual al DataGrid a refrescar los cambios de la tabla.
                    dgClientes.Items.Refresh();
                }
            }
            else
            {
                // Muestra un aviso flotante si el usuario hizo clic en editar sin seleccionar ninguna fila.
                MessageBox.Show("Selecciona un cliente de la tabla para editar.", "Aviso");
            }
        }

        // Evento que se dispara al hacer clic en el botón de eliminar de una fila.
        private void BtnEliminar_Click(object sender, RoutedEventArgs e)
        {
            // Obtiene el cliente seleccionado de la fila de la tabla.
            Cliente clienteSeleccionado = (Cliente)dgClientes.SelectedItem;

            // Valida que exista un cliente seleccionado.
            if (clienteSeleccionado != null)
            {
                // Muestra una ventana de confirmación antes de proceder a borrar el registro.
                MessageBoxResult resultado = MessageBox.Show($"¿Desea eliminar a {clienteSeleccionado.NombreCompleto}?", "Confirmar eliminación", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                // Si el usuario confirma haciendo clic en "Sí"...
                if (resultado == MessageBoxResult.Yes)
                {
                    // Remueve el cliente directamente de la colección observable, eliminándolo de la tabla al instante.
                    ListaClientes.Remove(clienteSeleccionado);
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