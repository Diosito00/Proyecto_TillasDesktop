using System;
using System.Windows;
using System.Windows.Input;
using TillasDesktop.Entities.Inventario;
using TillasDesktop.BLL.Services;

namespace TillasDesktop.UI.Modelos
{
    public class IngresoStockViewModel : ViewModelBase
    {
        private readonly InventarioService _inventarioService = new InventarioService();
        private readonly Producto _productoPuro;

        // Entidad DTO que sirve de transporte tanto para crear como para editar el stock del talle.
        private readonly ProductoTalle _movimiento;

        // === PROPIEDADES VISUALES DINÁMICAS ===
        // Permiten que el formulario de XAML se adapte cambiando sus títulos y botones sin tener que crear dos ventanas distintas.
        public string TituloVentana { get; set; }
        public string LabelCantidad { get; set; }
        public string TextoBoton { get; set; }

        // Bandera (Flag) que le indica a la UI y a las validaciones cómo comportarse.
        public bool EsModoEdicion { get; set; }

        // Muestra un resumen del producto en el encabezado de la ventana (ej: "Producto: NK-AF1 - Nike Air Force").
        private string _productoResumen;
        public string ProductoResumen
        {
            get => _productoResumen;
            set { _productoResumen = value; OnPropertyChanged(); }
        }

        // Enlace directo a las propiedades del DTO del movimiento.
        public decimal Talle
        {
            get => _movimiento.Talle;
            set { _movimiento.Talle = value; OnPropertyChanged(); }
        }

        public int Cantidad
        {
            get => _movimiento.Stock_Actual;
            set { _movimiento.Stock_Actual = value; OnPropertyChanged(); }
        }

        // === COMANDOS Y DELEGADOS ===
        public ICommand GuardarCommand { get; private set; }
        public Action CerrarVentana { get; set; }

        // Delegado que permite pasarle la entidad actualizada de vuelta a DetalleStockViewModel para que la UI principal se entere del cambio.
        public Action<ProductoTalle> OnStockIngresado { get; set; }

        // ====================================================================
        // CONSTRUCTOR 1: MODO NUEVO INGRESO (Cuando presionan "+ INGRESAR STOCK").
        // ====================================================================
        public IngresoStockViewModel(ProductoViewModel productoVM)
        {
            _productoPuro = productoVM.ObtenerEntidadPura();

            // Prepara un objeto de movimiento vacío, atado únicamente al ID del producto padre.
            _movimiento = new ProductoTalle
            {
                Producto_ID = _productoPuro.ID
            };

            // Configura la UI en modo "Creación".
            ConfigurarTextosUI("INGRESAR MERCADERÍA", "Cantidad a Ingresar", "CONFIRMAR INGRESO", false);
            InicializarComandos();
        }

        // ====================================================================
        // CONSTRUCTOR 2: MODO EDICIÓN (Cuando presionan el lápiz en un talle que ya existe).
        // ====================================================================
        public IngresoStockViewModel(ProductoViewModel productoVM, ProductoTalle talleExistente)
        {
            _productoPuro = productoVM.ObtenerEntidadPura();

            // Usamos la entidad existente que ya tiene un ID de base de datos cargado.
            _movimiento = talleExistente;

            // Configura la UI en modo "Edición".
            ConfigurarTextosUI("EDITAR STOCK", "Stock Actual", "GUARDAR CAMBIOS", true);
            InicializarComandos();
        }

        private void ConfigurarTextosUI(string titulo, string label, string boton, bool esEdicion)
        {
            TituloVentana = titulo;
            LabelCantidad = label;
            TextoBoton = boton;
            EsModoEdicion = esEdicion;
            ProductoResumen = $"Producto: {_productoPuro.Codigo_Modelo} - {_productoPuro.Nombre}";
        }

        private void InicializarComandos()
        {
            // El comando Evalúa 'PuedeGuardar' automáticamente en tiempo real mientras el usuario escribe.
            GuardarCommand = new RelayCommand(Guardar, PuedeGuardar);
        }

        private void Guardar(object parametro)
        {
            bool exito;
            string mensaje;

            // Se rutea la operación a un método u otro de la capa de negocios según el contexto de la ventana.
            if (EsModoEdicion)
            {
                exito = _inventarioService.ActualizarStockTalle(_movimiento, out mensaje);
            }
            else
            {
                exito = _inventarioService.RegistrarIngresoStock(_movimiento, out mensaje);
            }

            if (exito)
            {
                // Dispara el evento que manda el talle procesado hacia el ViewModel padre.
                OnStockIngresado?.Invoke(_movimiento);
                MessageBox.Show(mensaje, "Operación Exitosa", MessageBoxButton.OK, MessageBoxImage.Information);
                CerrarVentana?.Invoke();
            }
            else
            {
                MessageBox.Show(mensaje, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // Método que bloquea visualmente el botón de guardar si no se cumplen las condiciones base.
        private bool PuedeGuardar(object parametro)
        {
            // Si está editando, se permite que el stock quede en 0 (ej: si se vendió el último par físicamente o hubo merma).
            if (EsModoEdicion) return Talle > 0 && Cantidad >= 0;

            // Si es un nuevo ingreso de mercadería, carece de lógica ingresar "0" unidades.
            return Talle > 0 && Cantidad > 0;
        }
    }
}