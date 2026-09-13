using TillasDesktop.Entities.Inventario;

namespace TillasDesktop.UI.Modelos
{
    public class ProductoViewModel : ViewModelBase
    {
        // LA ENTIDAD PURA (Dueña de los datos reales de la BD)
        private readonly Producto _productoPuro;

        // VARIABLES VISUALES (Exclusivas de la UI, no van a la tabla Producto)
        private string _nombreMarca;
        private string _nombreCategoria;
        private int _stockTotal;

        public ProductoViewModel(Producto producto)
        {
            _productoPuro = producto;
        }

        // --- PROPIEDADES QUE MODIFICAN DIRECTO A LA ENTIDAD ---

        public int ID
        {
            get => _productoPuro.ID;
            set { _productoPuro.ID = value; OnPropertyChanged(); }
        }

        public string Codigo_Modelo
        {
            get => _productoPuro.Codigo_Modelo;
            set { _productoPuro.Codigo_Modelo = value; OnPropertyChanged(); }
        }

        public string Nombre
        {
            get => _productoPuro.Nombre;
            set { _productoPuro.Nombre = value; OnPropertyChanged(); }
        }

        public int Marca_ID
        {
            get => _productoPuro.Marca_ID;
            set { _productoPuro.Marca_ID = value; OnPropertyChanged(); }
        }

        public int Categoria_ID
        {
            get => _productoPuro.Categoria_ID;
            set { _productoPuro.Categoria_ID = value; OnPropertyChanged(); }
        }

        public decimal Precio_Venta
        {
            get => _productoPuro.Precio_Venta;
            set { _productoPuro.Precio_Venta = value; OnPropertyChanged(); }
        }

        public bool Activo
        {
            get => _productoPuro.Activo;
            set { _productoPuro.Activo = value; OnPropertyChanged(); }
        }

        // --- PROPIEDADES EXCLUSIVAS PARA LA INTERFAZ GRÁFICA (WPF) ---

        public string NombreMarca
        {
            get => _nombreMarca;
            set { _nombreMarca = value; OnPropertyChanged(); }
        }

        public string NombreCategoria
        {
            get => _nombreCategoria;
            set { _nombreCategoria = value; OnPropertyChanged(); }
        }

        public int StockTotal
        {
            get => _stockTotal;
            set { _stockTotal = value; OnPropertyChanged(); }
        }

        // --- PUENTE HACIA LA CAPA DE NEGOCIO ---
        public Producto ObtenerEntidadPura()
        {
            return _productoPuro;
        }
    }
}