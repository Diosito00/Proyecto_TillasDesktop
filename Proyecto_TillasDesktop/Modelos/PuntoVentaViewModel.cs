using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows;

namespace TillasDesktop.UI.Modelos
{
    public class PuntoVentaViewModel : ViewModelBase
    {
        private decimal _totalCobrar;

        // ObservableCollection actualiza el DataGrid del XAML automáticamente
        public ObservableCollection<DetalleVentaViewModel> Carrito { get; set; }
        // Declaramos el comando
        public ICommand CobrarCommand { get; }

        public decimal TotalCobrar
        {
            get { return _totalCobrar; }
            set { _totalCobrar = value; OnPropertyChanged(); }
        }

        public PuntoVentaViewModel()
        {
            Carrito = new ObservableCollection<DetalleVentaViewModel>();

            // Sobrescribimos un evento para recalcular el total cada vez que la lista cambie
            Carrito.CollectionChanged += (s, e) => RecalcularTotal();

            // Inicializamos el comando en el constructor
            // Parámetro 1: Qué hacer al hacer clic (EjecutarCobro)
            // Parámetro 2: Cuándo está habilitado el botón (PuedeCobrar)
            CobrarCommand = new RelayCommand(EjecutarCobro, PuedeCobrar);
        }

        public void AgregarProductoAlCarrito(DetalleVentaViewModel nuevoItem)
        {
            Carrito.Add(nuevoItem);
        }

        // El método que se ejecuta al presionar "CONFIRMAR COBRO"
        private void EjecutarCobro(object parametro)
        {
            MessageBox.Show($"¡Venta procesada con éxito por un total de {TotalCobrar:C}!", "Cobro exitoso");
            Carrito.Clear(); // Vaciamos el carrito después de cobrar
        }

        // Lógica que decide si el botón se puede presionar (retorna true o false)
        private bool PuedeCobrar(object parametro)
        {
            // El botón solo estará activo si el carrito tiene al menos 1 producto
            return Carrito.Count > 0;
        }

        public void RecalcularTotal()
        {
            TotalCobrar = Carrito.Sum(item => item.Subtotal);
        }
    }
}
