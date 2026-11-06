// Importa el espacio de nombres fundamental de .NET para tipos base y utilidades generales
using System;
// Importa clases base de WPF para la gestión de la aplicación y cuadros de mensajes interactivos
using System.Windows;
// Importa las utilidades de comandos y eventos de entrada de la interfaz de usuario en WPF
using System.Windows.Input;
// Importa las entidades del dominio de inventario (como Producto y ProductoTalle)
using TillasDesktop.Entities.Inventario;
// Importa los servicios de la Capa de Negocios (BLL) encargados de gestionar las reglas e inventario de productos
using TillasDesktop.BLL.Services;

// Define el espacio de nombres (namespace) correspondiente a los modelos de vista (ViewModels) de la interfaz de usuario del sistema
namespace TillasDesktop.UI.Modelos
{
    // Declara la clase pública IngresoStockViewModel que hereda de ViewModelBase, encargada de manejar la ventana secundaria para ingresar stock nuevo o modificar un talle existente.
    public class IngresoStockViewModel : ViewModelBase
    {
        // Instancia de solo lectura del servicio que se comunica con la capa de negocios para impactar los cambios en la base de datos de inventario.
        private readonly InventarioService _inventarioService = new InventarioService();

        // Referencia privada al producto base (puro) sobre el cual estamos operando los talles y existencias.
        private readonly Producto _productoPuro;

        // Referencia al objeto que almacena los datos del talle y la cantidad actual. 
        // Se comparte y reutiliza tanto para dar de alta un talle nuevo como para editar uno existente.
        private readonly ProductoTalle _movimiento;

        // Propiedades de la interfaz que cambian dinámicamente (títulos y textos de botones) 
        // para permitir que la misma ventana visual sirva de manera polimórfica tanto para dar de alta como para editar.
        public string TituloVentana { get; set; }
        public string LabelCantidad { get; set; }
        public string TextoBoton { get; set; }

        // Bandera booleana para saber si estamos operando en modo edición (true) o creando un ingreso nuevo (false).
        public bool EsModoEdicion { get; set; }

        // Campo privado que almacena el resumen descriptivo del producto para el encabezado
        private string _productoResumen;
        // Propiedad pública bindeada al encabezado de la ventana (ej: "Producto: NK-AF1 - Nike Air Force") con notificación de cambios.
        public string ProductoResumen
        {
            get => _productoResumen;
            set { _productoResumen = value; OnPropertyChanged(); }
        }

        // Propiedad bindeada al campo de talle numérico en la pantalla con notificación de cambios.
        public int Talle
        {
            get => _movimiento.Talle;
            set { _movimiento.Talle = value; OnPropertyChanged(); }
        }

        // Propiedad bindeada al campo de cantidad/stock en la pantalla con notificación de cambios.
        public int Cantidad
        {
            get => _movimiento.Stock_Actual;
            set { _movimiento.Stock_Actual = value; OnPropertyChanged(); }
        }

        // Comando público que dispara la acción de guardar al hacer clic en el botón correspondiente del formulario.
        public ICommand GuardarCommand { get; private set; }
        // Acción (delegado) para ordenar a la ventana que se cierre de forma limpia sin acoplar código visual (respetando el patrón MVVM).
        public Action CerrarVentana { get; set; }

        // Acción (callback/delegado) que avisa al ViewModel padre (`DetalleStockViewModel`) 
        // que la operación fue un éxito y le devuelve el talle procesado para que la tabla se refresque al instante.
        public Action<ProductoTalle> OnStockIngresado { get; set; }

        // CONSTRUCTOR DE NUEVO INGRESO DE STOCK (Se utiliza al agregar un talle que no existía).
        public IngresoStockViewModel(ProductoViewModel productoVM)
        {
            // Extrae la entidad pura subyacente del producto envuelto
            _productoPuro = productoVM.ObtenerEntidadPura();

            // Inicializamos un objeto ProductoTalle nuevo apuntando al ID del producto actual.
            _movimiento = new ProductoTalle
            {
                Producto_ID = _productoPuro.ID
            };

            // Configuramos los textos y etiquetas de la interfaz para el modo "Creación/Alta".
            ConfigurarTextosUI("INGRESAR MERCADERÍA", "Cantidad a Ingresar", "CONFIRMAR INGRESO", false);
            // Inicializa los comandos de guardado y validación
            InicializarComandos();
        }

        // CONSTRUCTOR DE EDICIÓN DE STOCK (Se utiliza al modificar un talle que ya figuraba en la lista existente).
        public IngresoStockViewModel(ProductoViewModel productoVM, ProductoTalle talleExistente)
        {
            // Extrae la entidad pura del producto
            _productoPuro = productoVM.ObtenerEntidadPura();

            // Reutilizamos la entidad existente que ya tiene su ID de registro cargado desde la base de datos.
            _movimiento = talleExistente;

            // Configuramos los textos de la ventana para el modo "Edición".
            ConfigurarTextosUI("EDITAR STOCK", "Stock Actual", "GUARDAR CAMBIOS", true);
            // Inicializa los comandos de guardado y validación
            InicializarComandos();
        }

        // Método auxiliar privado para no repetir código al configurar los textos visuales y el resumen en los constructores.
        private void ConfigurarTextosUI(string titulo, string label, string boton, bool esEdicion)
        {
            TituloVentana = titulo;
            LabelCantidad = label;
            TextoBoton = boton;
            EsModoEdicion = esEdicion;
            // Construye el resumen informativo combinando el código de modelo y el nombre del producto
            ProductoResumen = $"Producto: {_productoPuro.Codigo_Modelo} - {_productoPuro.Nombre}";
        }

        // Prepara el comando de guardado asociándole la regla de validación delegada para habilitar o deshabilitar el botón en tiempo real.
        private void InicializarComandos()
        {
            // El método PuedeGuardar se ejecuta automáticamente cada vez que el usuario modifica un campo (tipea o cambia valores), 
            // bloqueando o desbloqueando de forma reactiva el botón de guardar.
            GuardarCommand = new RelayCommand(Guardar, PuedeGuardar);
        }

        // Lógica principal que se ejecuta al presionar el botón de confirmar en el formulario.
        private void Guardar(object parametro)
        {
            bool exito;
            string mensaje;

            // Se rutea la operación a un método u otro de la capa de negocios según el contexto operativo de la ventana (Edición vs Alta).
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
                // Dispara el evento/callback que envía el talle procesado hacia el ViewModel padre (`DetalleStockViewModel`).
                OnStockIngresado?.Invoke(_movimiento);
                // Muestra un mensaje emergente de éxito al operador
                MessageBox.Show(mensaje, "Operación Exitosa", MessageBoxButton.OK, MessageBoxImage.Information);
                // Ordena el cierre de la ventana secundaria
                CerrarVentana?.Invoke();
            }
            else
            {
                // Si la capa de negocios rechaza la operación, muestra una advertencia con el motivo
                MessageBox.Show(mensaje, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // Método de validación condicional que bloquea o habilita visualmente el botón de guardar si no se cumplen las reglas de negocio base.
        private bool PuedeGuardar(object parametro)
        {
            // Si estamos editando un talle existente, permitimos que la cantidad quede en 0 (por ejemplo, si se agotó el stock físico por completo).
            if (EsModoEdicion) return Talle > 0 && Cantidad >= 0;

            // Si es un ingreso nuevo de mercadería, no tiene sentido ingresar 0 o menos unidades; tanto el talle como la cantidad deben ser estrictamente positivos.
            return Talle > 0 && Cantidad > 0;
        }
    }
}