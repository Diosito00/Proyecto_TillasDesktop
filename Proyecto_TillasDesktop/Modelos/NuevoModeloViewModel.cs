using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using TillasDesktop.Entities.Inventario;
using TillasDesktop.BLL.Services;

namespace TillasDesktop.UI.Modelos
{
    public class NuevoModeloViewModel : ViewModelBase
    {
        private string _codigoModelo;
        private string _nombre;
        private decimal _precioVenta;
        private Marca _marcaSeleccionada;
        private Categoria _categoriaSeleccionada;
        private bool _esEdicion;
        private Producto _productoAEditar;

        private readonly InventarioService _inventarioService;

        public string CodigoModelo { get => _codigoModelo; set { _codigoModelo = value; OnPropertyChanged(); } }
        public string Nombre { get => _nombre; set { _nombre = value; OnPropertyChanged(); } }
        public decimal PrecioVenta { get => _precioVenta; set { _precioVenta = value; OnPropertyChanged(); } }
        public Marca MarcaSeleccionada { get => _marcaSeleccionada; set { _marcaSeleccionada = value; OnPropertyChanged(); } }
        public Categoria CategoriaSeleccionada { get => _categoriaSeleccionada; set { _categoriaSeleccionada = value; OnPropertyChanged(); } }

        public ObservableCollection<Marca> MarcasDisponibles { get; set; }
        public ObservableCollection<Categoria> CategoriasDisponibles { get; set; }

        public ICommand GuardarCommand { get; }
        public Action CerrarVentana { get; set; }
        public Action<ProductoViewModel> OnModeloGuardado { get; set; }

        public NuevoModeloViewModel()
        {
            // Instanciamos el servicio de negocio
            _inventarioService = new InventarioService();

            // Llenamos las colecciones visuales delegando la consulta a la BLL
            MarcasDisponibles = new ObservableCollection<Marca>(_inventarioService.ObtenerMarcasActivas());
            CategoriasDisponibles = new ObservableCollection<Categoria>(_inventarioService.ObtenerCategoriasActivas());

            GuardarCommand = new RelayCommand(Guardar, PuedeGuardar);
        }

        public NuevoModeloViewModel(ProductoViewModel productoExistente) : this()
        {
            _esEdicion = true;
            _productoAEditar = productoExistente.ObtenerEntidadPura();

            // Precargamos los datos en la pantalla
            CodigoModelo = _productoAEditar.Codigo_Modelo;
            Nombre = _productoAEditar.Nombre;
            PrecioVenta = _productoAEditar.Precio_Venta;

            // Aquí usamos LINQ para seleccionar la marca y categoría correctas en los ComboBox
            MarcaSeleccionada = MarcasDisponibles.FirstOrDefault(m => m.ID == _productoAEditar.Marca_ID);
            CategoriaSeleccionada = CategoriasDisponibles.FirstOrDefault(c => c.ID == _productoAEditar.Categoria_ID);
        }

        private void Guardar(object parametro)
        {
            if (_esEdicion)
            {
                // Actualizamos la entidad existente
                _productoAEditar.Codigo_Modelo = this.CodigoModelo;
                _productoAEditar.Nombre = this.Nombre;
                _productoAEditar.Precio_Venta = this.PrecioVenta;
                _productoAEditar.Marca_ID = this.MarcaSeleccionada.ID;
                _productoAEditar.Categoria_ID = this.CategoriaSeleccionada.ID;

                bool exito = _inventarioService.ActualizarProducto(_productoAEditar, out string mensaje);
                if (exito)
                {
                    MessageBox.Show(mensaje, "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    CerrarVentana?.Invoke();
                }
            }
            else
            {
                var nuevaEntidad = new Producto
                {
                    Codigo_Modelo = this.CodigoModelo,
                    Nombre = this.Nombre,
                    Marca_ID = this.MarcaSeleccionada.ID,
                    Categoria_ID = this.CategoriaSeleccionada.ID,
                    Precio_Venta = this.PrecioVenta,
                    Activo = true
                };

                // La BLL toma la decisión final
                bool exito = _inventarioService.RegistrarNuevoProducto(nuevaEntidad, out string mensajeRespuesta);

                if (exito)
                {
                    var nuevoProductoVM = new ProductoViewModel(nuevaEntidad)
                    {
                        NombreMarca = this.MarcaSeleccionada.Nombre,
                        NombreCategoria = this.CategoriaSeleccionada.Nombre,
                        StockTotal = 0
                    };

                    OnModeloGuardado?.Invoke(nuevoProductoVM);
                    MessageBox.Show(mensajeRespuesta, "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    CerrarVentana?.Invoke();
                }
                else
                {
                    MessageBox.Show(mensajeRespuesta, "Error de Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private bool PuedeGuardar(object parametro)
        {
            return !string.IsNullOrWhiteSpace(CodigoModelo) &&
                   !string.IsNullOrWhiteSpace(Nombre) &&
                   PrecioVenta > 0 &&
                   MarcaSeleccionada != null &&
                   CategoriaSeleccionada != null;
        }
    }
}