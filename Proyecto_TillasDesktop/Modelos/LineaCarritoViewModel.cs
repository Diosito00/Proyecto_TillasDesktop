namespace TillasDesktop.UI.Modelos
{
    public class LineaCarritoViewModel : ViewModelBase
    {
        // Datos fijos de la línea de venta
        public int ProductoID { get; set; }
        public string Nombre { get; set; }
        public int Talle { get; set; }
        public decimal PrecioUnitario { get; set; }

        // Propiedad reactiva: Cuando la cantidad cambia (sumamos o restamos del carrito), 
        // también avisamos que el Subtotal debe recalcularse visualmente.
        private int _cantidad;
        public int Cantidad
        {
            get => _cantidad;
            set
            {
                _cantidad = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Subtotal));
            }
        }

        // Propiedad calculada (Solo lectura)
        public decimal Subtotal => Cantidad * PrecioUnitario;
    }
}