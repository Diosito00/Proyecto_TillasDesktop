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

        private void Guardar(object parametro)
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