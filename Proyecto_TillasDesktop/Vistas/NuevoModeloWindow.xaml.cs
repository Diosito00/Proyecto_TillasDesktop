using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TillasDesktop.UI.Modelos;

namespace TillasDesktop.UI.Vistas
{
    /// <summary>
    /// Lógica de interacción para NuevoModeloWindow.xaml
    /// </summary>
    public partial class NuevoModeloWindow : Window
    {
        public NuevoModeloWindow()
        {
            InitializeComponent();
        }

        private void SoloDecimales_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox textBox = sender as TextBox;

            // Recorremos los caracteres que se intentan ingresar
            foreach (char c in e.Text)
            {
                // Si no es un número, ni un punto, ni una coma, bloqueamos la entrada
                if (!char.IsDigit(c) && c != '.' && c != ',')
                {
                    e.Handled = true;
                    return;
                }

                // Si ya existe un punto o una coma en el texto, evitamos que se escriba otro
                if ((c == '.' || c == ',') && (textBox.Text.Contains(".") || textBox.Text.Contains(",")))
                {
                    e.Handled = true;
                    return;
                }
            }
        }
    }

}
