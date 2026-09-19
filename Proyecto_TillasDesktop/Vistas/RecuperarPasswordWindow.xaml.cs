using System.Text.RegularExpressions;
using System.Windows;
using TillasDesktop.UI.Modelos;

namespace TillasDesktop.UI.Vistas
{
    /// responsable únicamente de inicializar la interfaz y conectarla con su ViewModel.
    public partial class RecuperarPasswordWindow : Window
    {

        // Instancia del ViewModel que maneja la lógica de validación del correo.
        private RecuperarPasswordViewModel _viewModel;


        public RecuperarPasswordWindow()
        {
            InitializeComponent(); // Dibuja los elementos visuales definidos en el XAML.

            // Crea la instancia del ViewModel que manejará los datos y la lógica de la ventana.
            _viewModel = new RecuperarPasswordViewModel();

            // Conecta la pantalla visual con el "cerebro" (ViewModel). 
            DataContext = _viewModel;

            // Configura la acción para que el ViewModel pueda ordenar el cierre de la ventana de forma segura.
            _viewModel.CerrarVentanaAccion = () => this.Close();
        }
        
    }
}