using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace TillasDesktop.UI.Modelos
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;

        // Constructor que recibe el método a ejecutar y, opcionalmente, una condición para habilitar/deshabilitar el botón
        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        // Determina si el botón debe estar clickeable o grisado (deshabilitado)
        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        // Ejecuta la acción cuando se hace clic
        public void Execute(object parameter)
        {
            _execute(parameter);
        }

        // Le avisa a la interfaz que re-evalúe si el botón debe habilitarse o deshabilitarse
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}
