using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TillasDesktop.UI.Vistas
{
    /// <summary>
    /// Lógica de interacción para EditarCantidadWindow.xaml
    /// </summary>
    public partial class EditarCantidadWindow : Window
    {
        public int NuevaCantidad { get; private set; }

        public EditarCantidadWindow(string nombreProducto, int cantidadActual)
        {
            InitializeComponent();

            txtMensaje.Text = $"Ingresa la nueva cantidad para:\n{nombreProducto}";

            txtCantidad.Text = cantidadActual.ToString();
            txtCantidad.Focus();
            txtCantidad.SelectAll();
        }

        private void BtnAceptar_Click(object sender, RoutedEventArgs e)
        {
            Confirmar();
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void TxtCantidad_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Confirmar();
            }
        }

        private void Confirmar()
        {
            if (int.TryParse(txtCantidad.Text, out int cantidad) && cantidad >= 0)
            {
                NuevaCantidad = cantidad;
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Por favor, ingresa un número entero válido (0 o mayor).", "Valor Inválido", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtCantidad.Focus();
                txtCantidad.SelectAll();
            }
        }

        private void SoloNumeros_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !e.Text.All(char.IsDigit);
        }
    }
}
