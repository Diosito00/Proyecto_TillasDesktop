using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Input;
using TillasDesktop.UI.Vistas;

namespace TillasDesktop.UI.Modelos
{
    public class InventarioViewModel : ViewModelBase
    {
        private string _textoBusqueda;

        public ObservableCollection<ProductoViewModel> ListaProductos { get; set; }

        public string TextoBusqueda
        {
            get => _textoBusqueda;
            set { _textoBusqueda = value; OnPropertyChanged(); /* Aquí iría la lógica para filtrar la lista */ }
        }

        public ICommand IngresarStockCommand { get; }
        public ICommand NuevoModeloCommand { get; }

        public InventarioViewModel()
        {
            ListaProductos = new ObservableCollection<ProductoViewModel>();

            IngresarStockCommand = new RelayCommand(AbrirVentanaStock);
            NuevoModeloCommand = new RelayCommand(AbrirVentanaNuevoModelo);

            // Datos de prueba temporales hasta conectar MariaDB
            ListaProductos.Add(new ProductoViewModel { Codigo_Modelo = "NK-AF1-01", Nombre = "Air Force 1", Marca = "Nike", Categoria = "Sneakers", Precio_Venta = 125000, StockTotal = 45 });
            ListaProductos.Add(new ProductoViewModel { Codigo_Modelo = "AD-SM-02", Nombre = "Samba OG", Marca = "Adidas", Categoria = "Sneakers", Precio_Venta = 110000, StockTotal = 12 });
        }

        private void AbrirVentanaStock(object parametro)
        {
            MessageBox.Show("Abriendo formulario de ingreso de mercadería...", "Ingreso de Stock");
        }

        private void AbrirVentanaNuevoModelo(object parametro)
        {
            var ventana = new Vistas.NuevoModeloWindow();
            ventana.ShowDialog(); // ShowDialog bloquea la ventana de atrás hasta que el usuario termine
        }
    }
}
