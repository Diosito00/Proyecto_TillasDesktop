using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace TillasDesktop.UI.Modelos
{

    public class RecuperarPasswordViewModel : ViewModelBase
    {
        // Almacena el correo ingresado por el usuario con su respectiva notificación a la vista.
        private string _emailRecuperacion = string.Empty;
        public string EmailRecuperacion
        {
            get => _emailRecuperacion;
            set { _emailRecuperacion = value; OnPropertyChanged(); }
        }

        // Comando que se ejecuta al presionar el botón de enviar/verificar.
        public ICommand EnviarCommand { get; }

        // Comando que se ejecuta al presionar el botón de volver.
        public ICommand VolverCommand { get; }

        // Acción (delegado) para ordenar a la ventana física que se cierre desde el ViewModel.
        public Action CerrarVentanaAccion { get; set; }

        public RecuperarPasswordViewModel()
        {
            // Inicializamos los comandos enlazándolos a sus métodos correspondientes.
            EnviarCommand = new RelayCommand(EjecutarEnviar);
            VolverCommand = new RelayCommand(EjecutarVolver);
        }

        // Lógica de negocio y validación para el envío de correo de recuperación.
        private void EjecutarEnviar(object obj)
        {
            // Limpia espacios en los extremos y previene valores nulos asignando una cadena vacía.
            string email = EmailRecuperacion?.Trim() ?? string.Empty;

            // Validación 1: Verificar que el campo no esté vacío.
            if (string.IsNullOrWhiteSpace(email))
            {
                MessageBox.Show("Por favor, ingrese su correo electrónico.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Validación 2: Verificar el formato correcto del correo mediante una expresión regular.
            string patronEmail = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            if (!Regex.IsMatch(email, patronEmail))
            {
                MessageBox.Show("El formato del correo electrónico no es válido.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Simulación de éxito y cierre de ventana.
            MessageBox.Show("Se han enviado las instrucciones de recuperación a su correo electrónico.", "Recuperación Exitosa", MessageBoxButton.OK, MessageBoxImage.Information);

            // Invocamos la acción para cerrar la vista de forma controlada.
            CerrarVentanaAccion?.Invoke();
        }

        // Lógica para cancelar y volver atrás.
        private void EjecutarVolver(object obj)
        {
            CerrarVentanaAccion?.Invoke();
        }
    }
}