using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Text.RegularExpressions;
using TillasDesktop.Entities.Inventario;
using TillasDesktop.BLL.Services;

namespace TillasDesktop.UI.Modelos
{
    // Hereda de ViewModelBase para mantener la interfaz actualizada automáticamente cuando cambian los datos.
    public class NuevoModeloViewModel : ViewModelBase
    {
        // Instancia del servicio que maneja la lógica de negocio  
        private readonly InventarioService _inventarioService = new InventarioService();

        // Título dinámico de la ventana ("NUEVO MODELO" o "EDITAR MODELO").
        public string TituloFormulario { get; set; }
        public bool EsModoEdicion { get; set; }

        // Objeto que envuelve la entidad del producto y avisa a la pantalla cuando alguna de sus propiedades se modifica.
        public ProductoViewModel ProductoActual { get; set; }

        // Campo privado y propiedad pública para la marca que el usuario selecciona en el ComboBox de la interfaz.
        private Marca _marcaSeleccionada;
        public Marca MarcaSeleccionada { get => _marcaSeleccionada; set { _marcaSeleccionada = value; OnPropertyChanged(); } }

        // Campo privado y propiedad pública para la categoría que el usuario selecciona en el ComboBox.
        private Categoria _categoriaSeleccionada;
        public Categoria CategoriaSeleccionada { get => _categoriaSeleccionada; set { _categoriaSeleccionada = value; OnPropertyChanged(); } }

        // Colecciones observables para llenar las listas desplegables (ComboBox) de marcas y categorías en la vista.
        public ObservableCollection<Marca> MarcasDisponibles { get; set; }
        public ObservableCollection<Categoria> CategoriasDisponibles { get; set; }

        // Comando vinculado al botón de guardar del formulario.
        public ICommand GuardarCommand { get; private set; }

        // Acciones (delegados) para ordenar que la ventana se cierre y para avisarle al listado principal que guardamos un producto.
        public Action CerrarVentana { get; set; }
        public Action OnModeloGuardado { get; set; }

        // CONSTRUCTOR 1: Se usa cuando abrimos el formulario limpio para dar de alta un producto nuevo.
        public NuevoModeloViewModel()
        {
            EsModoEdicion = false;
            TituloFormulario = "NUEVO MODELO";

            // Se inicializa una entidad en blanco con estado activo por defecto.
            var entidadNueva = new Producto { Activo = true };
            ProductoActual = new ProductoViewModel(entidadNueva);

            InicializarComunes();
        }

        // CONSTRUCTOR 2: Se usa cuando seleccionamos un producto existente en la tabla para modificar sus datos.
        public NuevoModeloViewModel(Producto productoExistente)
        {
            EsModoEdicion = true;
            TituloFormulario = "EDITAR MODELO";

            // Envuelve la entidad clonada que se envió desde la tabla principal de Inventario.
            ProductoActual = new ProductoViewModel(productoExistente);

            InicializarComunes();

            // Cuando editamos, los ComboBox deben mostrar la marca y categoría actual del producto.
            // Buscamos en nuestras listas disponibles cuál coincide con los IDs guardados en InventarioService.
            MarcaSeleccionada = MarcasDisponibles.FirstOrDefault(m => m.ID == ProductoActual.Marca_ID);
            CategoriaSeleccionada = CategoriasDisponibles.FirstOrDefault(c => c.ID == ProductoActual.Categoria_ID);
        }

        // Método auxiliar para evitar repetir código: carga las listas desde la base de datos y arma el comando de guardar.
        private void InicializarComunes()
        {
            // Llenamos las listas desplegables consultando las marcas y categorías guardados en InventarioService.
            MarcasDisponibles = new ObservableCollection<Marca>(_inventarioService.ObtenerMarcasActivas());
            CategoriasDisponibles = new ObservableCollection<Categoria>(_inventarioService.ObtenerCategoriasActivas());

            GuardarCommand = new RelayCommand(Guardar);
        }

        // Lógica que se ejecuta al hacer clic en el botón de guardar.
        private void Guardar(object parametro)
        {
            //  Validaciones explícitas de campos requeridos y coherencia de datos (precio mayor a 0).
            if (string.IsNullOrWhiteSpace(ProductoActual.Codigo_Modelo) ||
                string.IsNullOrWhiteSpace(ProductoActual.Nombre) ||
                ProductoActual.Precio_Venta <= 0 ||
                MarcaSeleccionada == null ||
                CategoriaSeleccionada == null)
            {
                MessageBox.Show("Todos los campos son obligatorios y el precio debe ser mayor a 0.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 2. VALIDACIÓN DE FORMATO SKU MEDIANTE EXPRESIÓN REGULAR (Regex).
            // ^[A-Z]{2} -> Exige exactamente 2 letras mayúsculas al inicio.
            // -         -> Exige un guion.
            // [A-Z0-9]{2,3} -> Exige entre 2 y 3 letras o números.
            // -         -> Exige otro guion.
            // \d{2}$    -> Exige exactamente 2 dígitos al final.
            if (!Regex.IsMatch(ProductoActual.Codigo_Modelo, @"^[A-Z]{2}-[A-Z0-9]{2,3}-\d{2}$"))
            {
                MessageBox.Show("El formato del Código (SKU) es incorrecto.\nDebe ser similar a 'NK-AF1-01' o 'AD-SM-02' (Mayúsculas y guiones requeridos).",
                                "Validación de Formato", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Extraemos la entidad pura y le asignamos los IDs correspondientes a la marca y categoría elegidas en los ComboBox.
            Producto entidadParaGuardar = ProductoActual.ObtenerEntidadPura();
            entidadParaGuardar.Marca_ID = MarcaSeleccionada.ID;
            entidadParaGuardar.Categoria_ID = CategoriaSeleccionada.ID;

            bool exito;
            string mensaje;

            // Dependiendo del modo, llamamos al método del servicio para actualizar un producto existente o registrar uno nuevo.
            if (EsModoEdicion)
            {
                exito = _inventarioService.ActualizarProducto(entidadParaGuardar, out mensaje);
            }
            else
            {
                exito = _inventarioService.RegistrarNuevoProducto(entidadParaGuardar, out mensaje);
                // Asigna el ID autogenerado por SQL Server a nuestra entidad en memoria (cuando se implemente ProductosRepository, por ahora es 0 pero no se muestra en la tabla).
                if (exito) ProductoActual.ID = entidadParaGuardar.ID;
            }

            // Si la base de datos confirmó la operación con éxito...
            if (exito)
            {
                // Actualizamos los nombres en texto de la marca y categoría en el ViewModel 
                // para que la tabla principal los refleje al instante sin necesidad de recargar todo.
                ProductoActual.NombreMarca = MarcaSeleccionada.Nombre;
                ProductoActual.NombreCategoria = CategoriaSeleccionada.Nombre;

                MessageBox.Show(EsModoEdicion ? "Modelo actualizado correctamente." : "Modelo creado correctamente.", "Operación Exitosa", MessageBoxButton.OK, MessageBoxImage.Information);

                // Disparamos los avisos para actualizar la tabla de la ventana principal y cerrar este formulario.
                OnModeloGuardado?.Invoke();
                CerrarVentana?.Invoke();
            }
            else
            {
               // Si hubo algún rechazo en las reglas de negocio, mostramos el aviso.
                MessageBox.Show(mensaje, "Error de Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}