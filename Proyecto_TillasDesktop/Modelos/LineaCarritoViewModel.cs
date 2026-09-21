namespace TillasDesktop.UI.Modelos
{
    // Hereda de ViewModelBase para poder avisar a la interfaz gráfica cuando cambia algún valor.
    public class LineaCarritoViewModel : ViewModelBase
    {
        // Propiedades de configuración estática que no cambian una vez que el producto se agrega al carrito.
        public int ProductoID { get; set; }
        public string Nombre { get; set; }
        public int Talle { get; set; }
        public decimal PrecioUnitario { get; set; }

        // Propiedad reactiva: La cantidad puede aumentar o disminuir mediante los botones de "+" o "-" del carrito.
        private int _cantidad;
        public int Cantidad
        {
            get => _cantidad;
            set
            {
                _cantidad = value;

                // Avisa a la vista que el número de cantidad cambió y debe redibujarlo.
                OnPropertyChanged();

                // Como el Subtotal depende de la cantidad, también disparamos un aviso
                // indicando que la propiedad 'Subtotal' debe volver a calcularse en pantalla.
                OnPropertyChanged(nameof(Subtotal));
            }
        }

        // Propiedad calculada de solo lectura. No necesita variable privada porque siempre es el resultado 
        // matemático de multiplicar la cantidad actual por el precio del producto.
        public decimal Subtotal => Cantidad * PrecioUnitario;
    }
}