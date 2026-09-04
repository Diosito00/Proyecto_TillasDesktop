using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using TillasDesktop.Entities.Inventario;

namespace TillasDesktop.UI.Modelos
{
    public class NuevoModeloViewModel : ViewModelBase
    {
        private string _codigoModelo;
        private string _nombre;
        private decimal _precioVenta;
        private Marca _marcaSeleccionada;
        private Categoria _categoriaSeleccionada;

        public string CodigoModelo { get => _codigoModelo; set { _codigoModelo = value; OnPropertyChanged(); } }
        public string Nombre { get => _nombre; set { _nombre = value; OnPropertyChanged(); } }
        public decimal PrecioVenta { get => _precioVenta; set { _precioVenta = value; OnPropertyChanged(); } }
        public Marca MarcaSeleccionada { get => _marcaSeleccionada; set { _marcaSeleccionada = value; OnPropertyChanged(); } }
        public Categoria CategoriaSeleccionada { get => _categoriaSeleccionada; set { _categoriaSeleccionada = value; OnPropertyChanged(); } }

        public ObservableCollection<Marca> MarcasDisponibles { get; set; }
        public ObservableCollection<Categoria> CategoriasDisponibles { get; set; }

        public ICommand GuardarCommand { get; }
        public Action CerrarVentana { get; set; } // Acción para cerrar la ventana desde el ViewModel
        public Action<ProductoViewModel> OnModeloGuardado { get; set; } // Una acción que recibirá el nuevo producto para enviarlo al inventario

        public NuevoModeloViewModel()
        {
            // Datos simulados hasta conectar MariaDB
            MarcasDisponibles = new ObservableCollection<Marca>
            {
                new Marca { ID = 1, Nombre = "Nike" },
                new Marca { ID = 2, Nombre = "Adidas" }
            };
            CategoriasDisponibles = new ObservableCollection<Categoria>
            {
                new Categoria { ID = 1, Nombre = "Sneakers" },
                new Categoria { ID = 2, Nombre = "Deportivo" }
            };

            GuardarCommand = new RelayCommand(Guardar, PuedeGuardar);
        }

        private void Guardar(object parametro)
        {
            // 1. Ensamblamos el nuevo objeto con los datos del formulario
            var nuevoProducto = new ProductoViewModel
            {
                Codigo_Modelo = this.CodigoModelo,
                Nombre = this.Nombre,
                Marca = this.MarcaSeleccionada.Nombre,
                Categoria = this.CategoriaSeleccionada.Nombre,
                Precio_Venta = this.PrecioVenta,
                StockTotal = 0 // Empieza sin stock hasta que se haga un ingreso
            };

            // 2. Ejecutamos la acción para enviarlo de vuelta a la pantalla principal
            OnModeloGuardado?.Invoke(nuevoProducto);

            MessageBox.Show($"Modelo {Nombre} guardado con éxito.", "Éxito");
            CerrarVentana?.Invoke();
        }

        private bool PuedeGuardar(object parametro)
        {
            return !string.IsNullOrWhiteSpace(CodigoModelo) && !string.IsNullOrWhiteSpace(Nombre) && PrecioVenta > 0 && MarcaSeleccionada != null && CategoriaSeleccionada != null;
        }
    }
}
