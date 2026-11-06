// Define el espacio de nombres (namespace) correspondiente a los modelos de vista de la interfaz gráfica del sistema
namespace TillasDesktop.UI.Modelos
{
    // Hereda de ViewModelBase para mantener la pantalla sincronizada mediante notificaciones automáticas (Patrón MVVM).
    public class ProductoDisponibleViewModel : ViewModelBase
    {
        // Propiedades de identidad e información general. Son fijas y no necesitan disparar eventos de cambio 
        // porque los atributos descriptivos de este artículo no se modifican mientras el cajero examina el catálogo.
        public int ProductoID { get; set; }
        public string Codigo_Modelo { get; set; }
        public string Nombre { get; set; }
        public string NombreMarca { get; set; }
        public string NombreCategoria { get; set; }
        public decimal Precio_Venta { get; set; }

        // El talle específico y diferenciado de este artículo (ej: 42). En el catálogo de ventas y facturación, 
        // un mismo modelo con distinto talle (ej. "Air Force 1 talle 41" vs "Air Force 1 talle 42") se gestiona como ítems independientes.
        public int Talle { get; set; }

        // Campo privado que almacena la existencia actual disponible en stock físico para este talle específico.
        private int _stockActual;

        // Propiedad reactiva: A medida que el vendedor selecciona y agrega unidades al carrito de compras, 
        // el stock físico visual disponible debe actualizarse y descontarse en tiempo real en la interfaz del catálogo. 
        // Por este motivo, requiere el uso explícito del bloque 'set' disparando el método 'OnPropertyChanged()'.
        public int Stock_Actual
        {
            get => _stockActual;
            set { _stockActual = value; OnPropertyChanged(); }
        }
    }
}