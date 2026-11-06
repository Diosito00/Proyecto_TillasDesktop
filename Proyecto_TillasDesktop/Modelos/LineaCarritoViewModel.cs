// Define el espacio de nombres (namespace) correspondiente a los modelos de vista de la interfaz gráfica del sistema
namespace TillasDesktop.UI.Modelos
{
    // Declara la clase pública LineaCarritoViewModel que hereda de ViewModelBase, encargada de administrar los ítems individuales y el cálculo dinámico de subtotales dentro del carrito de ventas.
    public class LineaCarritoViewModel : ViewModelBase
    {
        // Propiedades de configuración estática que no cambian una vez que el producto y su variante de talle se agregan al carrito de compras.
        public int ProductoID { get; set; }
        public string Nombre { get; set; }
        public int Talle { get; set; }
        public decimal PrecioUnitario { get; set; }

        // Campo privado que almacena la cantidad actual de pares seleccionados para esta línea del carrito.
        private int _cantidad;

        // Propiedad pública que se enlaza de forma bidireccional con la interfaz para ver o modificar la cantidad de pares mediante los botones de incremento/decremento.
        public int Cantidad
        {
            get => _cantidad;
            set
            {
                _cantidad = value;

                // Avisa a la vista (XAML) que el número de cantidad cambió y debe redibujarlo en pantalla.
                OnPropertyChanged();

                // Como el importe del Subtotal depende estrictamente de la cantidad, también disparamos un aviso explícito
                // indicando que la propiedad calculada 'Subtotal' debe refrescarse visualmente.
                OnPropertyChanged(nameof(Subtotal));
            }
        }

        // Propiedad calculada de solo lectura. No requiere variable privada de respaldo porque siempre se obtiene 
        // mediante el resultado matemático dinámico de multiplicar la cantidad actual por el precio unitario del producto.
        public decimal Subtotal => Cantidad * PrecioUnitario;
    }
}