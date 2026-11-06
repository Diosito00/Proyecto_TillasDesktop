// Importa clases base del sistema y tipos genéricos
using System;
using System.Collections.Generic;
using System.Text;
// Importa las utilidades de comandos y el administrador de eventos de WPF
using System.Windows.Input;

// Define el espacio de nombres (namespace) correspondiente a los modelos de vista de la interfaz gráfica del sistema
namespace TillasDesktop.UI.Modelos
{
    // Declara la clase pública RelayCommand que implementa la interfaz nativa 'ICommand' exigida por WPF para poder enlazar (Binding) acciones de botones.
    public class RelayCommand : ICommand
    {
        // El delegado (Action) que contiene la lógica principal que se ejecutará al hacer clic en el control (Ej: Guardar, Cobrar, Eliminar).
        private readonly Action<object> _execute;

        // El delegado opcional (Predicate) que evalúa una condición booleana (devuelve true o false). Si retorna false, WPF deshabilita el botón automáticamente.
        private readonly Predicate<object> _canExecute;

        // Constructor principal: recibe la acción de ejecución obligatoria y una condición de habilitación opcional.
        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            // El delegado de ejecución es estrictamente obligatorio; de lo contrario, se lanza una excepción de argumento nulo.
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        // Método invocado constantemente por el motor gráfico de WPF para evaluar si el botón vinculado debe mantenerse habilitado o desactivado.
        public bool CanExecute(object parameter)
        {
            // Si no se configuraron condiciones de bloqueo, el botón siempre estará disponible para hacer clic (retorna true).
            return _canExecute == null || _canExecute(parameter);
        }

        // Método principal que dispara el motor gráfico de WPF cuando el usuario presiona físicamente el botón en la interfaz.
        public void Execute(object parameter)
        {
            // Ejecuta el delegado de acción pasando el parámetro opcional recibido desde la vista.
            _execute(parameter);
        }

        // Evento obligatorio de la interfaz ICommand que se "engancha" al administrador global de comandos de WPF (CommandManager.RequerySuggested).
        // Obliga a que todos los controles y botones de la pantalla reevalúen de manera reactiva su estado (CanExecute) 
        // cada vez que el usuario hace clic o interactúa con otros elementos de la interfaz.
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}