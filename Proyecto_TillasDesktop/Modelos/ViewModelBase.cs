using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TillasDesktop.UI.Modelos
{
    // Es una clase abstracta porque nunca la instanciarás directamente (nunca harás 'new ViewModelBase()'). 
    // Solo sirve para que otras clases la hereden.
    // Implementa INotifyPropertyChanged, la interfaz nativa de WPF para detectar cambios.
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        // Evento principal al que se suscribe el motor gráfico de WPF (XAML).
        public event PropertyChangedEventHandler? PropertyChanged;

        // Método protegido que invocarán los hijos (como UsuarioViewModel o ReportesViewModel) cuando una propiedad cambie.
        // [CallerMemberName] es un atributo de C# que detecta automáticamente el nombre de la propiedad 
        // que llamó al método, evitando que tengas que escribir textos quemados como OnPropertyChanged("Nombre").
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            // Dispara el evento indicando exactamente qué propiedad cambió 
            // para que WPF solo redibuje ese pedacito de la pantalla y no toda la ventana completa.
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}