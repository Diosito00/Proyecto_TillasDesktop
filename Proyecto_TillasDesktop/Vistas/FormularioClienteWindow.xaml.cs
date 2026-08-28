using System.Windows; // Importa los componentes esenciales de la interfaz y ventanas de WPF.
using TillasDesktop.UI.Modelos; // Importa los modelos del proyecto (como la clase Cliente).

namespace TillasDesktop.UI.Vistas
{
    // Clase parcial que maneja la lógica de la ventana emergente para registrar o editar un cliente.
    public partial class FormularioClienteWindow : Window
    {
        // Propiedad pública que almacena el cliente creado o editado para ser devuelto a la ventana principal.
        public Cliente? NuevoCliente { get; set; }

        // Constructor vacío: se utiliza cuando se quiere dar de alta/crear un nuevo cliente desde cero.
        public FormularioClienteWindow()
        {
            InitializeComponent(); // Carga y dibuja los componentes visuales definidos en el archivo XAML.
        }

        // Constructor con parámetros: se utiliza cuando se quiere editar un cliente existente, recibiendo sus datos actuales.
        public FormularioClienteWindow(Cliente clienteAEditar)
        {
            InitializeComponent(); // Inicializa los componentes de la interfaz.

            // Rellena los cuadros de texto del formulario con la información del cliente que se va a editar.
            txtNombre.Text = clienteAEditar.NombreCompleto;
            txtDocumento.Text = clienteAEditar.Documento;
            txtTelefono.Text = clienteAEditar.Telefono;
            txtEmail.Text = clienteAEditar.Email;

            // Asigna la referencia del cliente existente para modificarlo directamente al guardar.
            NuevoCliente = clienteAEditar;
        }

        // Evento que se ejecuta al hacer clic en el botón "GUARDAR CLIENTE".
        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            // Valida si la propiedad es nula (significa que es un alta nueva y no una edición).
            if (NuevoCliente == null)
            {
                NuevoCliente = new Cliente(); // Instancia un nuevo objeto Cliente.
                NuevoCliente.Id = 104; // Asigna un ID temporal para el registro nuevo.
            }

            // Captura y asigna al objeto los valores actualizados que el usuario escribió en los TextBox.
            NuevoCliente.NombreCompleto = txtNombre.Text;
            NuevoCliente.Documento = txtDocumento.Text;
            NuevoCliente.Telefono = txtTelefono.Text;
            NuevoCliente.Email = txtEmail.Text;

            // Establece DialogResult en true para cerrar la ventana emergente e indicar a la vista principal que la acción fue exitosa.
            DialogResult = true;
        }
    }
}