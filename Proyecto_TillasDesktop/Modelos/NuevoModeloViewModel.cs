// Importa el espacio de nombres fundamental de .NET para tipos base y utilidades generales
using System;
// Importa las colecciones observables de WPF para llenar dinámicamente las listas desplegables (ComboBox) de marcas y categorías
using System.Collections.ObjectModel;
// Importa las extensiones de LINQ para realizar búsquedas y filtrados en memoria
using System.Linq;
// Importa clases base de WPF para la gestión de la aplicación y cuadros de mensajes interactivos
using System.Windows;
// Importa las utilidades de comandos y eventos de entrada de la interfaz de usuario en WPF
using System.Windows.Input;
// Importa las expresiones regulares (Regular Expressions) necesarias para validar formatos estrictos como el SKU o código de modelo
using System.Text.RegularExpressions;
// Importa las entidades del dominio de inventario (como Producto, Marca y Categoria)
using TillasDesktop.Entities.Inventario;
// Importa los servicios de la Capa de Negocios encargados de gestionar las operaciones y reglas del inventario
using TillasDesktop.BLL.Services;

// Define el espacio de nombres (namespace) correspondiente a los modelos de vista de la interfaz gráfica del sistema
namespace TillasDesktop.UI.Modelos
{
    // Declara la clase pública NuevoModeloViewModel que hereda de ViewModelBase, encargada de administrar la lógica de alta y edición de modelos de calzado.
    public class NuevoModeloViewModel : ViewModelBase
    {
        // Instancia de solo lectura del servicio que maneja la lógica de negocio y se comunica con la base de datos de inventario
        private readonly InventarioService _inventarioService = new InventarioService();

        // Propiedades de control para el título dinámico de la ventana ("NUEVO MODELO" o "EDITAR MODELO") y el modo de operación
        public string TituloFormulario { get; set; }
        public bool EsModoEdicion { get; set; }

        // Objeto envuelto (ProductoViewModel) que gestiona la entidad del producto y avisa a la pantalla cuando alguna de sus propiedades se modifica (Data Binding).
        public ProductoViewModel ProductoActual { get; set; }

        // Campo privado para almacenar el texto libre ingresado en el campo de precio (permite flexibilidad con puntos y comas)
        private string _precioVentaTexto;
        // Propiedad pública vinculada al cuadro de texto del precio de venta con notificación de cambios
        public string PrecioVentaTexto
        {
            get => _precioVentaTexto;
            set
            {
                _precioVentaTexto = value;
                OnPropertyChanged();
            }
        }

        // Campo privado y propiedad pública para la marca seleccionada en el ComboBox de la interfaz
        private Marca _marcaSeleccionada;
        public Marca MarcaSeleccionada { get => _marcaSeleccionada; set { _marcaSeleccionada = value; OnPropertyChanged(); } }

        // Campo privado y propiedad pública para la categoría seleccionada en el ComboBox de la interfaz
        private Categoria _categoriaSeleccionada;
        public Categoria CategoriaSeleccionada { get => _categoriaSeleccionada; set { _categoriaSeleccionada = value; OnPropertyChanged(); } }

        // Colecciones observables públicas para poblar las listas desplegables (ComboBox) de marcas y categorías activas en la vista
        public ObservableCollection<Marca> MarcasDisponibles { get; set; }
        public ObservableCollection<Categoria> CategoriasDisponibles { get; set; }

        // Comando público vinculado al botón de guardar del formulario de producto
        public ICommand GuardarCommand { get; private set; }

        // Acciones (delegados Actions) para ordenar de forma desacoplada que la ventana se cierre y para avisarle al listado principal que guardamos un producto con éxito.
        public Action CerrarVentana { get; set; }
        public Action OnModeloGuardado { get; set; }

        // CONSTRUCTOR 1: Se utiliza cuando abrimos el formulario limpio para dar de alta un producto nuevo.
        public NuevoModeloViewModel()
        {
            EsModoEdicion = false;
            TituloFormulario = "NUEVO MODELO";

            // Se inicializa una entidad en blanco con estado activo por defecto.
            var entidadNueva = new Producto { Activo = true };
            ProductoActual = new ProductoViewModel(entidadNueva);

            // Ejecuta el método común de carga de listas y comandos
            InicializarComunes();
        }

        // CONSTRUCTOR 2: Se utiliza cuando seleccionamos un producto existente en la tabla principal de inventario para modificar sus datos.
        public NuevoModeloViewModel(Producto productoExistente)
        {
            EsModoEdicion = true;
            TituloFormulario = "EDITAR MODELO";

            // Envuelve la entidad clonada que se envió desde la tabla principal de Inventario para aislar los cambios temporales.
            ProductoActual = new ProductoViewModel(productoExistente);

            // Ejecuta el método común de carga de listas y comandos
            InicializarComunes();

            // Cuando editamos, los ComboBox deben mostrar por defecto la marca y categoría actual asignada al producto.
            // Buscamos en nuestras listas disponibles cuál coincide exactamente con los IDs guardados en la entidad.
            MarcaSeleccionada = MarcasDisponibles.FirstOrDefault(m => m.ID == ProductoActual.Marca_ID);
            CategoriaSeleccionada = CategoriasDisponibles.FirstOrDefault(c => c.ID == ProductoActual.Categoria_ID);
        }

        // Método auxiliar privado para evitar duplicar código: carga las listas de la base de datos y configura el comando de guardado.
        private void InicializarComunes()
        {
            // Llenamos las colecciones consultando las marcas y categorías activas guardadas mediante el servicio de inventario.
            MarcasDisponibles = new ObservableCollection<Marca>(_inventarioService.ObtenerMarcasActivas());
            CategoriasDisponibles = new ObservableCollection<Categoria>(_inventarioService.ObtenerCategoriasActivas());

            // Inicializamos el texto libre del precio utilizando el valor numérico decimal actual del producto convertido a cadena.
            PrecioVentaTexto = ProductoActual.Precio_Venta.ToString();

            // Asigna el comando de guardado con su método delegado asociado
            GuardarCommand = new RelayCommand(Guardar);
        }

        // Lógica principal de validación estricta y persistencia que se ejecuta al hacer clic en el botón de guardar.
        private void Guardar(object parametro)
        {
            // Normalizamos y convertimos el texto libre del precio ingresado a decimal (reemplazando comas por puntos y usando cultura invariante)
            string precioLimpio = PrecioVentaTexto?.Replace(',', '.') ?? string.Empty;
            if (decimal.TryParse(precioLimpio, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal precioParsed))
            {
                ProductoActual.Precio_Venta = precioParsed;
            }
            else
            {
                ProductoActual.Precio_Venta = -1; // Asignamos un valor numérico inválido intencionalmente para que la validación posterior lo capture
            }

            // Validaciones explícitas de campos requeridos obligatorios y coherencia de datos (ej. precio estrictamente mayor a 0 y selecciones en ComboBox).
            if (string.IsNullOrWhiteSpace(ProductoActual.Codigo_Modelo) ||
                string.IsNullOrWhiteSpace(ProductoActual.Nombre) ||
                ProductoActual.Precio_Venta <= 0 ||
                MarcaSeleccionada == null ||
                CategoriaSeleccionada == null)
            {
                MessageBox.Show("Todos los campos son obligatorios y el precio debe ser mayor a 0.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return; // Corta la ejecución si hay errores
            }

            // VALIDACIÓN DE FORMATO SKU MEDIANTE EXPRESIÓN REGULAR (Regex):
            // ^[A-Z]{2}    -> Exige exactamente 2 letras mayúsculas al inicio.
            // -            -> Exige un guion separador.
            // [A-Z0-9]{2,3}-> Exige entre 2 y 3 letras o números.
            // -            -> Exige otro guion separador.
            // \d{2}$       -> Exige exactamente 2 dígitos numéricos al final.
            if (!Regex.IsMatch(ProductoActual.Codigo_Modelo, @"^[A-Z]{2}-[A-Z0-9]{2,3}-\d{2}$"))
            {
                MessageBox.Show("El formato del Código (SKU) es incorrecto.\nDebe ser similar a 'NK-AF1-01' o 'AD-SM-02' (Mayúsculas y guiones requeridos).",
                                "Validación de Formato", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Extraemos la entidad pura subyacente y le asignamos los IDs correspondientes a la marca y categoría elegidas en los ComboBox.
            Producto entidadParaGuardar = ProductoActual.ObtenerEntidadPura();
            entidadParaGuardar.Marca_ID = MarcaSeleccionada.ID;
            entidadParaGuardar.Categoria_ID = CategoriaSeleccionada.ID;

            bool exito;
            string mensaje;

            // Dependiendo del modo operativo (Edición vs Alta), llamamos al método correspondiente del servicio de inventario.
            if (EsModoEdicion)
            {
                exito = _inventarioService.ActualizarProducto(entidadParaGuardar, out mensaje);
            }
            else
            {
                exito = _inventarioService.RegistrarNuevoProducto(entidadParaGuardar, out mensaje);
                // Si se creó con éxito, asignamos el ID autogenerado por la base de datos a nuestra entidad en memoria.
                if (exito) ProductoActual.ID = entidadParaGuardar.ID;
            }

            // Si la base de datos confirmó la operación con éxito...
            if (exito)
            {
                // Actualizamos los nombres en texto de la marca y categoría directamente en el ViewModel 
                // para que la tabla principal los refleje al instante sin necesidad de recargar todo el listado desde la BD.
                ProductoActual.NombreMarca = MarcaSeleccionada.Nombre;
                ProductoActual.NombreCategoria = CategoriaSeleccionada.Nombre;

                // Muestra un mensaje emergente de éxito al operador
                MessageBox.Show(EsModoEdicion ? "Modelo actualizado correctamente." : "Modelo creado correctamente.", "Operación Exitosa", MessageBoxButton.OK, MessageBoxImage.Information);

                // Disparamos los delegados (acciones) para actualizar la tabla de la ventana principal y cerrar este formulario de manera limpia.
                OnModeloGuardado?.Invoke();
                CerrarVentana?.Invoke();
            }
            else
            {
                // Si la capa de negocios o el servicio rechazaron la operación, mostramos el aviso con el motivo.
                MessageBox.Show(mensaje, "Error de Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}