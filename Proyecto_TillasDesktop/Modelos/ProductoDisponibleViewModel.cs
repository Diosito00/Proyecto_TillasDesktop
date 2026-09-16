namespace TillasDesktop.UI.Modelos
{
    public class ProductoDisponibleViewModel : ViewModelBase
    {
        // Propiedades fijas (no cambian mientras están en la pantalla)
        public int ProductoID { get; set; }
        public string Codigo_Modelo { get; set; }
        public string Nombre { get; set; }
        public string NombreMarca { get; set; }
        public decimal Precio_Venta { get; set; }
        public int Talle { get; set; }

        // Propiedad reactiva: Notifica a la interfaz cuando el stock baja al mandarlo al carrito
        private int _stockActual;
        public int Stock_Actual
        {
            get => _stockActual;
            set { _stockActual = value; OnPropertyChanged(); }
        }
    }
}
