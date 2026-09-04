using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using TillasDesktop.Entities.Facturacion;

namespace TillasDesktop.UI.Modelos
{
    public class PuntoVentaViewModel : ViewModelBase
    {
        private decimal _totalCobrar;
        private string _codigoBusqueda;

        public ObservableCollection<DetalleVentaViewModel> Carrito { get; set; }

        // Catálogo simulado hasta conectar MariaDB
        private ObservableCollection<ProductoViewModel> _catalogoPrueba;

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
            Carrito = new ObservableCollection<DetalleVentaViewModel>();
            Carrito.CollectionChanged += (s, e) => RecalcularTotal();

            // Llenamos el catálogo de prueba
            _catalogoPrueba = new ObservableCollection<ProductoViewModel>
            {
                new ProductoViewModel { Codigo_Modelo = "779001", Nombre = "Air Force 1 - Talle 42", Precio_Venta = 125000 },
                new ProductoViewModel { Codigo_Modelo = "779002", Nombre = "Samba OG - Talle 39", Precio_Venta = 110000 }
            };

            CobrarCommand = new RelayCommand(EjecutarCobro, PuedeCobrar);
            BuscarProductoCommand = new RelayCommand(BuscarYAgregarProducto);
            EliminarItemCommand = new RelayCommand(EliminarItem);
        }

        private void BuscarYAgregarProducto(object parametro)
        {
            if (string.IsNullOrWhiteSpace(CodigoBusqueda)) return;

            var productoEncontrado = _catalogoPrueba.FirstOrDefault(p => p.Codigo_Modelo == CodigoBusqueda);

            if (productoEncontrado != null)
            {
                // Verificamos si ya está en el carrito para sumar la cantidad
                var itemExistente = Carrito.FirstOrDefault(c => c.NombreProducto == productoEncontrado.Nombre);

                if (itemExistente != null)
                {
                    itemExistente.Cantidad += 1;
                    RecalcularTotal(); // Agrega esta línea para actualizar el total general de la caja
                }
                else
                {
                    var nuevoDetalle = new DetalleVenta
                    {
                        Precio_Unitario = productoEncontrado.Precio_Venta,
                        Cantidad = 1
                    };
                    Carrito.Add(new DetalleVentaViewModel(nuevoDetalle, productoEncontrado.Nombre));
                }

                CodigoBusqueda = string.Empty; // Limpiamos el buscador
            }
            else
            {
                MessageBox.Show("Producto no encontrado. Verifique el código.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void EliminarItem(object parametro)
        {
            if (parametro is DetalleVentaViewModel itemAEliminar)
            {
                Carrito.Remove(itemAEliminar);
            }
        }

        private void EjecutarCobro(object parametro)
        {
            MessageBox.Show($"¡Venta procesada con éxito por un total de {TotalCobrar:C}!", "Cobro exitoso");
            Carrito.Clear();
        }

        private bool PuedeCobrar(object parametro) => Carrito.Count > 0;

        public void RecalcularTotal()
        {
            TotalCobrar = Carrito.Sum(item => item.Subtotal);
        }
    }
}