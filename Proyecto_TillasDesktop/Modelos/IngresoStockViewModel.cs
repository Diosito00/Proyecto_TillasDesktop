using System;
using System.Windows;
using System.Windows.Input;
using TillasDesktop.Entities.Inventario;
using TillasDesktop.BLL.Services;

namespace TillasDesktop.UI.Modelos
{
    // ViewModel encargado de manejar la ventana secundaria para ingresar stock nuevo o modificar un talle existente.
    public class IngresoStockViewModel : ViewModelBase
    {
        // Servicio que se comunica con la capa de negocios para impactar los cambios en la base de datos.
        private readonly InventarioService _inventarioService = new InventarioService();

        // El producto base sobre el cual estamos operando los talles.
        private readonly Producto _productoPuro;

        // El objeto que almacena los datos del talle y la cantidad. 
        // Se comparte tanto para crear uno nuevo como para editar uno existente.
        private readonly ProductoTalle _movimiento;


        // Propiedades de la interfaz que cambian dinámicamente (títulos y textos de botones) 
        // para que la misma ventana sirva tanto para dar de alta como para editar.
        public string TituloVentana { get; set; }
        public string LabelCantidad { get; set; }
        public string TextoBoton { get; set; }

        // Bandera booleana para saber si estamos editando (true) o creando un ingreso nuevo (false).
        public bool EsModoEdicion { get; set; }


        // Muestra un resumen del producto en el encabezado de la ventana (ej: "Producto: NK-AF1 - Nike Air Force").
        private string _productoResumen;
        public string ProductoResumen
        {
            get => _productoResumen;
            set { _productoResumen = value; OnPropertyChanged(); }
        }

        // Propiedad bindeada al campo de talle en la pantalla.
        public decimal Talle
        {
            get => _movimiento.Talle;
            set { _movimiento.Talle = value; OnPropertyChanged(); }
        }

        // Propiedad bindeada al campo de cantidad/stock en la pantalla.
        public int Cantidad
        {
            get => _movimiento.Stock_Actual;
            set { _movimiento.Stock_Actual = value; OnPropertyChanged(); }
        }

        // Comando que dispara la acción de guardar al hacer clic en el botón correspondiente.
        public ICommand GuardarCommand { get; private set; }
        // Acción (delegado) para ordenar a la ventana que se cierre sola sin romper el MVVM.
        public Action CerrarVentana { get; set; }

        // Acción que avisa al ViewModel  (DetalleStockViewModel) 
        // que la operación fue un éxito y le devuelve el talle actualizado para que la tabla se refresque 
        public Action<ProductoTalle> OnStockIngresado { get; set; }

        // CONSTRUCTOR de  NUEVO INGRESO DE STOCK (Se usa al agregar un talle).
        public IngresoStockViewModel(ProductoViewModel productoVM)
        {
            _productoPuro = productoVM.ObtenerEntidadPura();

            // Inicializamos un objeto nuevo apuntando al ID del producto actual.
            _movimiento = new ProductoTalle
            {
                Producto_ID = _productoPuro.ID
            };

            // Configuramos los textos de la ventana para el modo "Creación".
            ConfigurarTextosUI("INGRESAR MERCADERÍA", "Cantidad a Ingresar", "CONFIRMAR INGRESO", false);
            InicializarComandos();
        }

        // CONSTRUCTOR de  EDICIÓN DE STOCK (Se usa al modificar un talle que ya figuraba en la lista).
        public IngresoStockViewModel(ProductoViewModel productoVM, ProductoTalle talleExistente)
        {
            _productoPuro = productoVM.ObtenerEntidadPura();

            // Reutilizamos la entidad existente que ya tiene su ID cargado desde la base de datos.
            _movimiento = talleExistente;

            // Configuramos los textos de la ventana para el modo "Edición".
            ConfigurarTextosUI("EDITAR STOCK", "Stock Actual", "GUARDAR CAMBIOS", true);
            InicializarComandos();
        }

        // Método auxiliar para no repetir código al configurar los textos visuales en los constructores.
        private void ConfigurarTextosUI(string titulo, string label, string boton, bool esEdicion)
        {
            TituloVentana = titulo;
            LabelCantidad = label;
            TextoBoton = boton;
            EsModoEdicion = esEdicion;
            ProductoResumen = $"Producto: {_productoPuro.Codigo_Modelo} - {_productoPuro.Nombre}";
        }

        // Prepara el comando de guardado atándole la regla de validación para habilitar/deshabilitar el botón.
        private void InicializarComandos()
        {
            /// El método PuedeGuardar se ejecuta automáticamente cada vez que el usuario tipea algo, 
            // bloqueando o desbloqueando el botón de guardar en tiempo real.
            GuardarCommand = new RelayCommand(Guardar, PuedeGuardar);
        }

        // Lógica que se ejecuta al presionar el botón de confirmar.
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
            // Si estamos editando, permitimos que la cantidad quede en 0 (por ejemplo, si se agotó el stock físico).
            if (EsModoEdicion) return Talle > 0 && Cantidad >= 0;

            // Si es un ingreso nuevo, no tiene sentido ingresar 0 o menos unidades; el talle y la cantidad deben ser positivos.
            return Talle > 0 && Cantidad > 0;
        }
    }
}