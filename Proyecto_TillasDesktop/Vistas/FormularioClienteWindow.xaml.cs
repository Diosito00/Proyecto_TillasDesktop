using System.Windows; // Importa los componentes esenciales de la interfaz y ventanas de WPF.
using System.Windows.Input;
using TillasDesktop.Entities.Usuarios;
using TillasDesktop.UI.Modelos;// Importa los modelos del proyecto (como la clase Cliente).
using System.Text.RegularExpressions;


namespace TillasDesktop.UI.Vistas
{
    // Clase parcial que maneja la lógica de la ventana emergente para registrar o editar un cliente.
    public partial class FormularioClienteWindow : Window
    {
        // Instancia del ViewModel que contiene las reglas de negocio, validaciones y datos del formulario.
        private FormularioClienteViewModel _viewModel;

        /// Propiedad pública que expone el cliente resultante (creado o modificado) para ser recuperado
        /// por la ventana padre una vez que se confirma la operación de guardado.
        public ClienteViewModel? NuevoCliente => _viewModel?.ClienteResultado;

        // Constructor vacío: se utiliza cuando se quiere dar de alta/crear un nuevo cliente desde cero.
        public FormularioClienteWindow()
        {
            InitializeComponent(); // Carga y dibuja los componentes visuales definidos en el archivo XAML.
            _viewModel = new FormularioClienteViewModel(); // Inicializa el ViewModel en blanco.
            ConfigurarViewModel(); // Enlaza el contexto de datos y las acciones de cierre de la ventana.
        }

        // Constructor con parámetros: se utiliza cuando se quiere editar un cliente existente, recibiendo sus datos actuales.
        public FormularioClienteWindow(ClienteViewModel clienteAEditar)
        {
            InitializeComponent(); // Inicializa los componentes de la interfaz.

            _viewModel = new FormularioClienteViewModel(clienteAEditar); // Inicializa el ViewModel inyectando los datos existentes.
            ConfigurarViewModel(); // Enlaza el contexto de datos y las acciones de cierre de la ventana.
        }

        /// Configura el enlace de datos (DataContext) de la ventana con el ViewModel y define la acción
        /// que cerrará la interfaz de manera controlada cuando la validación y el guardado sean exitosos.
        private void ConfigurarViewModel()
        {
            DataContext = _viewModel; // Conecta la pantalla visual con el "cerebro" (ViewModel).

            // Define la acción (delegado) que ejecutará el ViewModel para cerrar la ventana,
            // devolviendo el resultado booleano (DialogResult = true si se guardó con éxito).
            _viewModel.CerrarVentanaAccion = (resultado) =>
            {
                DialogResult = resultado;
                Close();
            };
        }

        
        private void SoloNumeros_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Expresión regular que solo permite dígitos numéricos
            e.Handled = !e.Text.All(char.IsDigit);
        }

        private void Telefono_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !e.Text.All(c => char.IsDigit(c) || c == '+' || c == '-' || c == ' ');
        }

        private void SoloLetras_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Permite solo letras (incluyendo vocales con tilde y espacios)
            e.Handled = !e.Text.All(c => char.IsLetter(c) || char.IsWhiteSpace(c));
        }
    }
}