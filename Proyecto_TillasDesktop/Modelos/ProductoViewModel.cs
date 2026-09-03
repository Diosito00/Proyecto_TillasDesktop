using System;
using System.Collections.Generic;
using System.Text;

namespace TillasDesktop.UI.Modelos
{
    public class ProductoViewModel : ViewModelBase
    {
        private string _codigoModelo;
        private string _nombre;
        private string _marca;
        private string _categoria;
        private decimal _precioVenta;
        private int _stockTotal;

        public string Codigo_Modelo
        {
            get => _codigoModelo;
            set { _codigoModelo = value; OnPropertyChanged(); }
        }
        public string Nombre
        {
            get => _nombre;
            set { _nombre = value; OnPropertyChanged(); }
        }
        public string Marca
        {
            get => _marca;
            set { _marca = value; OnPropertyChanged(); }
        }
        public string Categoria
        {
            get => _categoria;
            set { _categoria = value; OnPropertyChanged(); }
        }
        public decimal Precio_Venta
        {
            get => _precioVenta;
            set { _precioVenta = value; OnPropertyChanged(); }
        }
        public int StockTotal
        {
            get => _stockTotal;
            set { _stockTotal = value; OnPropertyChanged(); }
        }
    }
}