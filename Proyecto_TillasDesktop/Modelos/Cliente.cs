using System.ComponentModel; // Importa las herramientas para la notificación de cambios en las propiedades (necesario para WPF).
using System.Runtime.CompilerServices; // Importa atributos para detectar automáticamente el nombre de la propiedad que cambió.

namespace TillasDesktop.UI.Modelos
{
    // La clase Cliente representa la estructura de los datos de un cliente. 
    // Hereda de INotifyPropertyChanged para que la interfaz gráfica (WPF) se entere cuando sus valores cambian.
    public class Cliente : INotifyPropertyChanged
    {
        // Campos privados que almacenan internamente los datos del cliente de forma segura.
        private int _id;
        private string _nombreCompleto = string.Empty;
        private string _documento = string.Empty;
        private string _telefono = string.Empty;
        private string _email = string.Empty;

        // Propiedad pública para el ID del cliente.
        public int Id
        {
            get => _id; // Retorna el valor actual almacenado en privado.
            set { _id = value; OnPropertyChanged(); } // Guarda el nuevo valor y avisa a la interfaz que cambió.
        }

        // Propiedad pública para el Nombre Completo del cliente.
        public string NombreCompleto
        {
            get => _nombreCompleto; // Retorna el nombre actual.
            set { _nombreCompleto = value; OnPropertyChanged(); } // Actualiza el nombre y avisa a la interfaz.
        }

        // Propiedad pública para el Documento (DNI / CUIT) del cliente.
        public string Documento
        {
            get => _documento; // Retorna el documento actual.
            set { _documento = value; OnPropertyChanged(); } // Actualiza el documento y avisa a la interfaz.
        }

        // Propiedad pública para el Teléfono del cliente.
        public string Telefono
        {
            get => _telefono; // Retorna el teléfono actual.
            set { _telefono = value; OnPropertyChanged(); } // Actualiza el teléfono y avisa a la interfaz.
        }

        // Propiedad pública para el Email del cliente.
        public string Email
        {
            get => _email; // Retorna el email actual.
            set { _email = value; OnPropertyChanged(); } // Actualiza el email y avisa a la interfaz.
        }

        // Evento requerido por la interfaz INotifyPropertyChanged para detectar los cambios de las propiedades.
        public event PropertyChangedEventHandler? PropertyChanged;

        // Método encargado de disparar la notificación hacia la interfaz gráfica.
        // [CallerMemberName] detecta automáticamente el nombre de la propiedad que acaba de modificarse.
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}