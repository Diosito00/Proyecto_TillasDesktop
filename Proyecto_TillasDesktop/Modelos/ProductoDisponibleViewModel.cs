namespace TillasDesktop.UI.Modelos
{
    // Hereda de ViewModelBase para mantener la pantalla sincronizada mediante notificaciones automáticas.
    public class ProductoDisponibleViewModel : ViewModelBase
    {
        // Propiedades de identidad e información. Son fijas y no necesitan disparar eventos 
        // porque el nombre o precio de este ítem no van a cambiar mientras el cajero mira el catálogo.
        public int ProductoID { get; set; }
        public string Codigo_Modelo { get; set; }
        public string Nombre { get; set; }
        public string NombreMarca { get; set; }
        public decimal Precio_Venta { get; set; }

        // El talle específico de este artículo (ej: 42). En el catálogo de ventas, 
        // un "Air Force 1 talle 41" y un "Air Force 1 talle 42" son ítems separados.
        public int Talle { get; set; }

        // Propiedad reactiva: A medida que el vendedor hace clic en "Agregar al Carrito", 
        // el stock físico visual debe ir descontándose en tiempo real en la pantalla. 
        // Por eso requiere el bloque 'set' con 'OnPropertyChanged()'.
        private int _stockActual;
        public int Stock_Actual
        {
            get => _stockActual;
            set { _stockActual = value; OnPropertyChanged(); }
        }
    }
}