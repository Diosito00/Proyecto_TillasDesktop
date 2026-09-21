using TillasDesktop.Entities.Inventario;

namespace TillasDesktop.UI.Modelos
{
    // Hereda de ViewModelBase para habilitar el evento OnPropertyChanged().
    public class ProductoViewModel : ViewModelBase
    {
        // La entidad "pura" (sin lógica visual) que será enviada a la BLL y DAL.
        private readonly Producto _productoPuro;

        // Propiedades auxiliares que NO existen en la tabla de Productos, 
        // pero que la interfaz (UI) necesita mostrar en la grilla para que el cajero entienda los datos.
        private string _nombreMarca;
        private string _nombreCategoria;
        private int _stockTotal;

        // El constructor recibe la entidad de la BD, o crea una nueva en blanco si es null (modo creación).
        public ProductoViewModel(Producto producto)
        {
            _productoPuro = producto ?? new Producto();
        }

        // ==========================================
        // PROPIEDADES ENLAZADAS A LA ENTIDAD PURA
        // ==========================================
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

        // En la BD solo guardamos el ID (Ej: "1"). 
        // En la UI, este ID es inyectado por los ComboBox al seleccionar una opción.
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

        // ==========================================
        // PROPIEDADES EXCLUSIVAS DE LA INTERFAZ GRÁFICA
        // ==========================================

        // El texto legible que se mostrará en la grilla (Ej: "Nike" en lugar del Marca_ID "1").
        public string NombreMarca
        {
            get => _nombreMarca;
            set { _nombreMarca = value; OnPropertyChanged(); }
        }

        // El texto legible (Ej: "Urbano" en lugar de Categoria_ID "2").
        public string NombreCategoria
        {
            get => _nombreCategoria;
            set { _nombreCategoria = value; OnPropertyChanged(); }
        }

        // La sumatoria de todos los talles de esta zapatilla. Se calcula en tiempo real.
        public int StockTotal
        {
            get => _stockTotal;
            set { _stockTotal = value; OnPropertyChanged(); }
        }

        // Método vital que "desempaqueta" la entidad para enviarla a SQL sin "ensuciarla" con los nombres de marca o categoría.
        public Producto ObtenerEntidadPura()
        {
            return _productoPuro;
        }
    }
}