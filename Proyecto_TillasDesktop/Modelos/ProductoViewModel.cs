// Importa las entidades del dominio de inventario (en este caso, la clase base Producto)
using TillasDesktop.Entities.Inventario;

// Define el espacio de nombres (namespace) correspondiente a los modelos de vista de la interfaz gráfica del sistema
namespace TillasDesktop.UI.Modelos
{
    // Declara la clase pública ProductoViewModel que hereda de ViewModelBase, encargada de envolver a la entidad pura Producto para mantener la pantalla sincronizada mediante notificaciones de cambios (Patrón MVVM).
    public class ProductoViewModel : ViewModelBase
    {
        // La entidad "pura" (modelo de datos original sin lógica de interfaz ni dependencias de WPF) que luego se enviará de forma limpia a las capas BLL y DAL.
        private readonly Producto _productoPuro;

        // Propiedades auxiliares y campos privados que NO existen nativamente en la tabla física de la base de datos de Productos, 
        // pero que la interfaz de usuario (UI) necesita mostrar en la grilla para que el operador o cajero comprenda los datos descriptivos.
        private string _nombreMarca;
        private string _nombreCategoria;
        private int _stockTotal;

        // Constructor principal: recibe un producto existente o instancia uno nuevo en blanco si llega como nulo (utilizado por ejemplo para operaciones de alta o creación).
        public ProductoViewModel(Producto producto)
        {
            _productoPuro = producto ?? new Producto();
        }

        // Propiedad envuelta para el ID numérico único del producto; actualiza de inmediato la entidad interna y dispara OnPropertyChanged para notificar a la pantalla si cambia.
        public int ID
        {
            get => _productoPuro.ID;
            set { _productoPuro.ID = value; OnPropertyChanged(); }
        }

        // Código SKU único del modelo de zapatilla (ej: "NK-AF1-01"), enlazado de forma bidireccional con la entidad pura.
        public string Codigo_Modelo
        {
            get => _productoPuro.Codigo_Modelo;
            set { _productoPuro.Codigo_Modelo = value; OnPropertyChanged(); }
        }

        // Nombre comercial descriptivo del producto (ej: "Air Force 1"), con notificación de cambios.
        public string Nombre
        {
            get => _productoPuro.Nombre;
            set { _productoPuro.Nombre = value; OnPropertyChanged(); }
        }

        // En la base de datos solo guardamos la clave foránea numérica (Ej: ID "1"). 
        // En la UI, este ID es inyectado y actualizado por los ComboBox al seleccionar una marca específica.
        public int Marca_ID
        {
            get => _productoPuro.Marca_ID;
            set { _productoPuro.Marca_ID = value; OnPropertyChanged(); }
        }

        // Identificador numérico de la categoría asociada al producto en la base de datos relacional.
        public int Categoria_ID
        {
            get => _productoPuro.Categoria_ID;
            set { _productoPuro.Categoria_ID = value; OnPropertyChanged(); }
        }

        // Precio de venta al público por par expresado en formato decimal.
        public decimal Precio_Venta
        {
            get => _productoPuro.Precio_Venta;
            set { _productoPuro.Precio_Venta = value; OnPropertyChanged(); }
        }

        // Bandera lógica booleana para saber si el producto está activo (visible en catálogo) o dado de baja lógica en el sistema.
        public bool Activo
        {
            get => _productoPuro.Activo;
            set { _productoPuro.Activo = value; OnPropertyChanged(); }
        }

        // El texto legible y descriptivo que se mostrará directamente en la grilla visual (Ej: "Nike" en lugar de mostrar únicamente el ID numérico "1").
        public string NombreMarca
        {
            get => _nombreMarca;
            set { _nombreMarca = value; OnPropertyChanged(); }
        }

        // El texto legible y descriptivo de la categoría en la interfaz (Ej: "Urbano" en lugar de mostrar el Categoria_ID numérico "2").
        public string NombreCategoria
        {
            get => _nombreCategoria;
            set { _nombreCategoria = value; OnPropertyChanged(); }
        }

        // La sumatoria acumulada de las existencias de todos los talles de esta zapatilla. Se calcula y actualiza dinámicamente.
        public int StockTotal
        {
            get => _stockTotal;
            set { _stockTotal = value; OnPropertyChanged(); }
        }

        // Método vital y de diseño clave que "desempaqueta" y retorna la entidad pura subyacente para poder enviarla hacia las capas inferiores 
        // de persistencia (SQL / BLL) sin "ensuciarla" ni acoplarla con propiedades visuales exclusivas de la interfaz (como nombres de marcas o stocks calculados).
        public Producto ObtenerEntidadPura()
        {
            return _productoPuro;
        }
    }
}