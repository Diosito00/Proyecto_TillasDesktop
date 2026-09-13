using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using TillasDesktop.BLL.Services;
using TillasDesktop.Entities.Facturacion;
using TillasDesktop.Entities.Inventario;

namespace TillasDesktop.UI.Modelos
{
    public class PuntoVentaViewModel : ViewModelBase
    {
        private decimal _totalCobrar;
        private string _codigoBusqueda;
        private readonly VentasService _ventasService;

        public ObservableCollection<DetalleVentaViewModel> Carrito { get; set; }

        public string CodigoBusqueda
        {
            get => _codigoBusqueda;
            set { _codigoBusqueda = value; OnPropertyChanged(); }
        }

        public decimal TotalCobrar
        {
            get => _totalCobrar;
            set { _totalCobrar = value; OnPropertyChanged(); }
        }

        public ICommand CobrarCommand { get; }
        public ICommand BuscarProductoCommand { get; }
        public ICommand EliminarItemCommand { get; }

        public PuntoVentaViewModel()
        {
            _ventasService = new VentasService();
            Carrito = new ObservableCollection<DetalleVentaViewModel>();

            // Cada vez que la colección cambia, delegamos el cálculo matemático a la BLL, traducimos los ViewModels a Entidades y calculamos
            var listaEntidades = Carrito.Select(c => c.ObtenerEntidadPura()).ToList();

            CobrarCommand = new RelayCommand(EjecutarCobro, PuedeCobrar);
            BuscarProductoCommand = new RelayCommand(BuscarYAgregarProducto);
            EliminarItemCommand = new RelayCommand(EliminarItem);
        }

        private void BuscarYAgregarProducto(object parametro)
        {
            // 1. Recibimos una Entidad pura de la BLL
            Producto productoEntidad = _ventasService.ObtenerProductoPorCodigo(CodigoBusqueda);

            if (productoEntidad != null)
            {
                var itemExistente = Carrito.FirstOrDefault(c => c.NombreProducto == productoEntidad.Nombre);
                if (itemExistente != null)
                {
                    itemExistente.Cantidad += 1;
                }
                else
                {
                    // 2. Empaquetamos la entidad en un ViewModel para la UI
                    var nuevoDetalle = new DetalleVenta
                    {
                        Precio_Unitario = productoEntidad.Precio_Venta,
                        Cantidad = 1
                    };
                    Carrito.Add(new DetalleVentaViewModel(nuevoDetalle, productoEntidad.Nombre));
                }

                // 3. Para enviar datos a la BLL, desempaquetamos los ViewModels y enviamos Entidades
                var listaEntidades = Carrito.Select(c => new DetalleVenta
                {
                    Precio_Unitario = c.PrecioUnitario,
                    Cantidad = c.Cantidad
                }).ToList();

                TotalCobrar = _ventasService.CalcularTotalVenta(listaEntidades);
                CodigoBusqueda = string.Empty;
            }
            else
            {
                MessageBox.Show("Producto no encontrado.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void EliminarItem(object parametro)
        {
            if (parametro is DetalleVentaViewModel item)
            {
                Carrito.Remove(item);
            }
        }

        private void EjecutarCobro(object parametro)
        {
            // Convertimos el carrito visual en entidades puras para la capa de negocio
            var listaEntidades = Carrito.Select(c => new DetalleVenta
            {
                Precio_Unitario = c.PrecioUnitario,
                Cantidad = c.Cantidad
            }).ToList();

            bool exito = _ventasService.RegistrarVenta(listaEntidades, out string mensaje);

            if (exito)
            {
                MessageBox.Show($"{mensaje}\nTotal cobrado: {TotalCobrar:C}", "Éxito");
                Carrito.Clear();
                TotalCobrar = 0;
            }
        }

        private bool PuedeCobrar(object parametro) => Carrito.Count > 0;
    }
}