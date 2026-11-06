// Importa las interfaces para la notificación de metadatos y componentes
using System.ComponentModel;
// Importa los atributos para la captura automática de miembros en tiempo de compilación
using System.Runtime.CompilerServices;

// Define el espacio de nombres (namespace) correspondiente a los modelos de vista de la interfaz gráfica del sistema
namespace TillasDesktop.UI.Modelos
{
    // Declara la clase abstracta pública 'ViewModelBase' que implementa 'INotifyPropertyChanged', 
    // la interfaz nativa fundamental de WPF para habilitar la reactividad y la sincronización de datos mediante enlace (Binding).
    // Es abstracta porque nunca se instancia directamente (no se crea un "new ViewModelBase()"); solo sirve como clase base para heredar sus capacidades a todos los demás ViewModels.
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        // Evento principal al que se suscribe automáticamente el motor gráfico de WPF (XAML) para enterarse cuando un valor se modifica en memoria.
        public event PropertyChangedEventHandler? PropertyChanged;

        // Método protegido que invocarán las clases hijas (como UsuarioViewModel, InventarioViewModel, etc.) cada vez que una propiedad sufra cambios.
        // El atributo [CallerMemberName] es una característica avanzada de C# que detecta de forma automática e implícita 
        // el nombre exacto de la propiedad que invocó al método, evitando la necesidad de escribir cadenas de texto quemadas o propensas a errores (ej. OnPropertyChanged("Nombre")).
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            // Dispara el evento PropertyChanged indicando exactamente qué propiedad cambió, 
            // permitiendo que el motor de WPF redibuje de manera optimizada únicamente ese control específico en la pantalla en lugar de recargar toda la ventana.
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}