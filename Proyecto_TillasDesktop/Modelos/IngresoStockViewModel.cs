using System;
using System.Windows;
using System.Windows.Input;
using TillasDesktop.Entities.Inventario;
using TillasDesktop.BLL.Services;

namespace TillasDesktop.UI.Modelos
{
    public class IngresoStockViewModel : ViewModelBase
    {
        private readonly InventarioService _inventarioService;
        private readonly Producto _productoPuro;

        // Entidad que viaja a la base de datos (sirve tanto para crear como para editar)
        private readonly ProductoTalle _movimiento;

        // === PROPIEDADES VISUALES DINÁMICAS ===
        public string TituloVentana { get; set; }
        public string LabelCantidad { get; set; }
        public string TextoBoton { get; set; }

        // Controla si el campo "Talle" debe estar bloqueado
        public bool EsModoEdicion { get; set; }

        private string _productoResumen;
        public string ProductoResumen
        {
            get => _productoResumen;
            set { _productoResumen = value; OnPropertyChanged(); }
        }

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
        public Action<ProductoTalle> OnStockIngresado { get; set; }

        // ====================================================================
        // CONSTRUCTOR 1: MODO NUEVO INGRESO (Cuando presionan "+ INGRESAR STOCK")
        // ====================================================================
        public IngresoStockViewModel(ProductoViewModel productoVM)
        {
            _inventarioService = new InventarioService();
            _productoPuro = productoVM.ObtenerEntidadPura();

            _movimiento = new ProductoTalle
            {
                Producto_ID = _productoPuro.ID
            };

            ConfigurarTextosUI("INGRESAR MERCADERÍA", "Cantidad a Ingresar", "CONFIRMAR INGRESO", false);
            InicializarComandos();
        }

        // ====================================================================
        // CONSTRUCTOR 2: MODO EDICIÓN (Cuando presionan el lápiz ✏️)
        // ====================================================================
        public IngresoStockViewModel(ProductoViewModel productoVM, ProductoTalle talleExistente)
        {
            _inventarioService = new InventarioService();
            _productoPuro = productoVM.ObtenerEntidadPura();

            // Usamos la entidad existente que ya tiene un ID cargado
            _movimiento = talleExistente;

            ConfigurarTextosUI("EDITAR STOCK", "Stock Actual", "GUARDAR CAMBIOS", true);
            InicializarComandos();
        }

        // === MÉTODOS AUXILIARES ===
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
            GuardarCommand = new RelayCommand(Guardar, PuedeGuardar);
        }

        // === LÓGICA DE GUARDADO ===
        private void Guardar(object parametro)
        {
            bool exito;
            string mensaje;

            // Derivamos a la BLL dependiendo de si es una creación o una actualización
            if (EsModoEdicion)
            {
                // NOTA: Asegúrate de tener este método en tu InventarioService
                exito = _inventarioService.ActualizarStockTalle(_movimiento, out mensaje);
            }
            else
            {
                exito = _inventarioService.RegistrarIngresoStock(_movimiento, out mensaje);
            }

            if (exito)
            {
                OnStockIngresado?.Invoke(_movimiento);
                MessageBox.Show(mensaje, "Operación Exitosa", MessageBoxButton.OK, MessageBoxImage.Information);
                CerrarVentana?.Invoke();
            }
            else
            {
                MessageBox.Show(mensaje, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private bool PuedeGuardar(object parametro)
        {
            // Si es edición, permitimos guardar stock en 0 (por si se quedaron sin stock), 
            // pero si es nuevo ingreso, forzamos a que sea mayor a 0.
            if (EsModoEdicion) return Talle > 0 && Cantidad >= 0;
            return Talle > 0 && Cantidad > 0;
        }
    }
}