using System.Windows;
using System.Windows.Input;
using System.Linq;
using TillasDesktop.Entities.Clientes; // Asegurado para usar la entidad Cliente
using TillasDesktop.UI.Modelos;


namespace TillasDesktop.UI.Vistas
{
    // Clase parcial que maneja la lógica de la ventana emergente para registrar o editar un cliente.
    public partial class FormularioClienteWindow : Window
    {


        // Constructor vacío: se utiliza cuando se quiere dar de alta/crear un nuevo cliente desde cero.
        public FormularioClienteWindow()
        {
            InitializeComponent(); // Carga y dibuja los componentes visuales definidos en el archivo XAML.
           
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