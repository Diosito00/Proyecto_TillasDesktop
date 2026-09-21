using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace TillasDesktop.UI.Modelos
{
    // Implementa ICommand, la interfaz nativa que pide WPF para poder hacer Binding a un botón.
    public class RelayCommand : ICommand
    {
        // El método principal que se ejecutará al hacer clic (Ej: Guardar, Cobrar).
        private readonly Action<object> _execute;

        // Método opcional (devuelve true o false). Si devuelve false, WPF desactiva el botón automáticamente.
        private readonly Predicate<object> _canExecute;

        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            // El Action es obligatorio, de lo contrario el botón no haría nada.
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        // Método invocado constantemente por el motor gráfico de WPF para saber si el botón debe habilitarse.
        public bool CanExecute(object parameter)
        {
            // Si no envian condiciones de bloqueo, el botón siempre es CLICKEABLE (true).
            return _canExecute == null || _canExecute(parameter);
        }

        // Método que dispara el motor gráfico de WPF cuando el usuario presiona el botón.
        public void Execute(object parameter)
        {
            _execute(parameter);
        }

        // Este evento se "engancha" al administrador general de comandos de WPF (CommandManager).
        // Obliga a que todos los botones de la pantalla re-evalúen su estado (CanExecute) si el usuario hace clic o teclea en otro lado.
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}