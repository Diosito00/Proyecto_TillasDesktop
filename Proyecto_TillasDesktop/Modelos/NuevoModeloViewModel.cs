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
        private readonly InventarioService _inventarioService = new InventarioService();

        public string TituloFormulario { get; set; }
        public bool EsModoEdicion { get; set; }
        public ProductoViewModel ProductoActual { get; set; }

        private Marca _marcaSeleccionada;
        public Marca MarcaSeleccionada { get => _marcaSeleccionada; set { _marcaSeleccionada = value; OnPropertyChanged(); } }

        private Categoria _categoriaSeleccionada;
        public Categoria CategoriaSeleccionada { get => _categoriaSeleccionada; set { _categoriaSeleccionada = value; OnPropertyChanged(); } }

        public ObservableCollection<Marca> MarcasDisponibles { get; set; }
        public ObservableCollection<Categoria> CategoriasDisponibles { get; set; }

        public ICommand GuardarCommand { get; private set; }
        public Action CerrarVentana { get; set; }
        public Action OnModeloGuardado { get; set; }

        // CONSTRUCTOR NUEVO
        public NuevoModeloViewModel()
        {
            EsModoEdicion = false;
            TituloFormulario = "NUEVO MODELO";

            var entidadNueva = new Producto { Activo = true };
            ProductoActual = new ProductoViewModel(entidadNueva);

            InicializarComunes();
        }

        // CONSTRUCTOR EDICIÓN (Recibe la entidad pura clonada)
        public NuevoModeloViewModel(Producto productoExistente)
        {
            EsModoEdicion = true;
            TituloFormulario = "EDITAR MODELO";

            ProductoActual = new ProductoViewModel(productoExistente);

            InicializarComunes();

            // Sincronizar los ComboBox con los IDs de la base de datos
            MarcaSeleccionada = MarcasDisponibles.FirstOrDefault(m => m.ID == ProductoActual.Marca_ID);
            CategoriaSeleccionada = CategoriasDisponibles.FirstOrDefault(c => c.ID == ProductoActual.Categoria_ID);
        }

        private void InicializarComunes()
        {            
            MarcasDisponibles = new ObservableCollection<Marca>(_inventarioService.ObtenerMarcasActivas());
            CategoriasDisponibles = new ObservableCollection<Categoria>(_inventarioService.ObtenerCategoriasActivas());

            // Ya no requiere PuedeGuardar
            GuardarCommand = new RelayCommand(Guardar);
        }

        private void Guardar(object parametro)
        {
            // Validaciones explícitas
            if (string.IsNullOrWhiteSpace(ProductoActual.Codigo_Modelo) ||
                string.IsNullOrWhiteSpace(ProductoActual.Nombre) ||
                ProductoActual.Precio_Venta <= 0 ||
                MarcaSeleccionada == null ||
                CategoriaSeleccionada == null)
            {
                MessageBox.Show("Todos los campos son obligatorios y el precio debe ser mayor a 0.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // VALIDACIÓN DE FORMATO SKU
            // ^[A-Z]{2}-[A-Z0-9]{2,3}-\d{2}$
            if (!Regex.IsMatch(ProductoActual.Codigo_Modelo, @"^[A-Z]{2}-[A-Z0-9]{2,3}-\d{2}$"))
            {
                MessageBox.Show("El formato del Código (SKU) es incorrecto.\nDebe ser similar a 'NK-AF1-01' o 'AD-SM-02' (Mayúsculas y guiones requeridos).",
                                "Validación de Formato", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Extraemos la entidad e inyectamos los IDs de los ComboBox seleccionados
            Producto entidadParaGuardar = ProductoActual.ObtenerEntidadPura();
            entidadParaGuardar.Marca_ID = MarcaSeleccionada.ID;
            entidadParaGuardar.Categoria_ID = CategoriaSeleccionada.ID;

            bool exito;
            string mensaje;

            // Operación de base de datos
            if (EsModoEdicion)
            {
                exito = _inventarioService.ActualizarProducto(entidadParaGuardar, out mensaje);
            }
            else
            {
                exito = _inventarioService.RegistrarNuevoProducto(entidadParaGuardar, out mensaje);
                if (exito) ProductoActual.ID = entidadParaGuardar.ID; // Asignar ID autogenerado
            }

            // Resolución
            if (exito)
            {
                // Sincronizar los nombres en texto para la tabla principal
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