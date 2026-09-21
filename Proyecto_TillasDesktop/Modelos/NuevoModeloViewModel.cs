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
    public class NuevoModeloViewModel : ViewModelBase
    {
        // Inicialización en línea de la variable readonly.
        private readonly InventarioService _inventarioService = new InventarioService();

        // Título dinámico de la ventana ("NUEVO MODELO" o "EDITAR MODELO").
        public string TituloFormulario { get; set; }
        public bool EsModoEdicion { get; set; }

        // Envoltorio que contiene a la entidad pura del producto y avisa a la UI de los cambios.
        public ProductoViewModel ProductoActual { get; set; }

        // Propiedades seleccionadas desde los ComboBox (Listas desplegables) de la interfaz.
        private Marca _marcaSeleccionada;
        public Marca MarcaSeleccionada { get => _marcaSeleccionada; set { _marcaSeleccionada = value; OnPropertyChanged(); } }

        private Categoria _categoriaSeleccionada;
        public Categoria CategoriaSeleccionada { get => _categoriaSeleccionada; set { _categoriaSeleccionada = value; OnPropertyChanged(); } }

        // Colecciones observables que llenan las opciones de los ComboBox de Marcas y Categorías.
        public ObservableCollection<Marca> MarcasDisponibles { get; set; }
        public ObservableCollection<Categoria> CategoriasDisponibles { get; set; }

        public ICommand GuardarCommand { get; private set; }
        public Action CerrarVentana { get; set; }
        public Action OnModeloGuardado { get; set; }

        // ====================================================================
        // CONSTRUCTOR 1: NUEVO MODELO (Alta de registro).
        // ====================================================================
        public NuevoModeloViewModel()
        {
            EsModoEdicion = false;
            TituloFormulario = "NUEVO MODELO";

            // Se inicializa una entidad en blanco con estado activo por defecto.
            var entidadNueva = new Producto { Activo = true };
            ProductoActual = new ProductoViewModel(entidadNueva);

            InicializarComunes();
        }

        // ====================================================================
        // CONSTRUCTOR 2: EDITAR MODELO (Modificación de registro).
        // ====================================================================
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

        private void InicializarComunes()
        {
            // Llenamos las listas desplegables consultando las marcas y categorías guardados en InventarioService.
            MarcasDisponibles = new ObservableCollection<Marca>(_inventarioService.ObtenerMarcasActivas());
            CategoriasDisponibles = new ObservableCollection<Categoria>(_inventarioService.ObtenerCategoriasActivas());

            GuardarCommand = new RelayCommand(Guardar);
        }

        private void Guardar(object parametro)
        {
            // 1. Validaciones explícitas de campos requeridos y coherencia de datos (precio mayor a 0).
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

            // Extraemos la entidad e inyectamos los IDs de los ComboBox.
            Producto entidadParaGuardar = ProductoActual.ObtenerEntidadPura();
            entidadParaGuardar.Marca_ID = MarcaSeleccionada.ID;
            entidadParaGuardar.Categoria_ID = CategoriaSeleccionada.ID;

            bool exito;
            string mensaje;

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

            // Resolución de la operación.
            if (exito)
            {
                // Sincroniza los nombres en texto de los ComboBox hacia el ViewModel 
                // para que la grilla principal actualice los textos inmediatamente.
                ProductoActual.NombreMarca = MarcaSeleccionada.Nombre;
                ProductoActual.NombreCategoria = CategoriaSeleccionada.Nombre;

                MessageBox.Show(EsModoEdicion ? "Modelo actualizado correctamente." : "Modelo creado correctamente.", "Operación Exitosa", MessageBoxButton.OK, MessageBoxImage.Information);

                OnModeloGuardado?.Invoke();
                CerrarVentana?.Invoke();
            }
            else
            {
                MessageBox.Show(mensaje, "Error de Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}