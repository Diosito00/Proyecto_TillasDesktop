using TillasDesktop.Entities.Inventario;

namespace TillasDesktop.UI.Modelos
{
    // Hereda de ViewModelBase para mantener la pantalla sincronizada mediante notificaciones de cambios.
    public class ProductoViewModel : ViewModelBase
    {
        // La entidad "pura" (modelo de datos original sin lógica de interfaz) que luego se enviará a las capas BLL y DAL.
        private readonly Producto _productoPuro;

        // Propiedades auxiliares que NO existen en la tabla de Productos, 
        // pero que la interfaz (UI) necesita mostrar en la grilla para que el cajero entienda los datos.
        private string _nombreMarca;
        private string _nombreCategoria;
        private int _stockTotal;

        // Constructor principal: recibe un producto existente o crea uno nuevo en blanco si llega como null (para dar de alta).
        public ProductoViewModel(Producto producto)
        {
            _productoPuro = producto ?? new Producto();
        }

        // Propiedad envuelta para el ID del producto; actualiza la entidad interna y avisa a la pantalla si cambia.
        public int ID
        {
            get => _productoPuro.ID;
            set { _productoPuro.ID = value; OnPropertyChanged(); }
        }

        // Código SKU del modelo de zapatilla (ej: "NK-AF1-01").
        public string Codigo_Modelo
        {
            get => _productoPuro.Codigo_Modelo;
            set { _productoPuro.Codigo_Modelo = value; OnPropertyChanged(); }
        }

        // Nombre comercial del producto (ej: "Air Force 1").
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

        // ID numérico de la categoría asociada al producto.
        public int Categoria_ID
        {
            get => _productoPuro.Categoria_ID;
            set { _productoPuro.Categoria_ID = value; OnPropertyChanged(); }
        }

        // Precio de venta al público por par.
        public decimal Precio_Venta
        {
            get => _productoPuro.Precio_Venta;
            set { _productoPuro.Precio_Venta = value; OnPropertyChanged(); }
        }

        // Bandera lógica para saber si el producto está activo (visible) o dado de baja en el sistema.
        public bool Activo
        {
            get => _productoPuro.Activo;
            set { _productoPuro.Activo = value; OnPropertyChanged(); }
        }

        

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