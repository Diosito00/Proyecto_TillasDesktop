using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using TillasDesktop.Entities.Inventario;

namespace TillasDesktop.UI.Modelos
{
    public class DetalleStockViewModel : ViewModelBase
    {
        private string _tituloVentana;
        private readonly Producto _productoPuro;

        public string TituloVentana
        {
            get => _tituloVentana;
            set { _tituloVentana = value; OnPropertyChanged(); }
        }

        // Aquí cargaremos los talles desde la base de datos (Ej: Talle 40 -> 5 unidades)
        public ObservableCollection<ProductoTalle> ListaTalles { get; set; }

        public ICommand IngresarNuevoStockCommand { get; }
        public Action CerrarVentana { get; set; }

        public DetalleStockViewModel(ProductoViewModel productoVM)
        {
            _productoPuro = productoVM.ObtenerEntidadPura();
            TituloVentana = $"Stock detallado: {_productoPuro.Codigo_Modelo} - {_productoPuro.Nombre}";

            ListaTalles = new ObservableCollection<ProductoTalle>();

            // FUTURO: Llamaremos a la BLL para llenar la lista
            // ListaTalles = new ObservableCollection<ProductoTalle>(_inventarioService.ObtenerTallesPorProducto(_productoPuro.ID));

            // Desde aquí mismo podemos abrir la ventanita pequeña de ingreso de stock que programamos en el turno anterior
            IngresarNuevoStockCommand = new RelayCommand(AbrirVentanaIngreso);
        }

        private void AbrirVentanaIngreso(object parametro)
        {
            // Lógica para abrir IngresoStockWindow...
        }
    }
}