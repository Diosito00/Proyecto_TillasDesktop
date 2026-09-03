using System;
using System.Collections.Generic;
using System.Text;
using TillasDesktop.Entities;
using TillasDesktop.Entities.Facturacion;

namespace TillasDesktop.UI.Modelos
{
    public class DetalleVentaViewModel : ViewModelBase
    {
        private DetalleVenta _detallePuro;
        private string _nombreProducto;

        public DetalleVentaViewModel(DetalleVenta detalle, string nombreProducto)
        {
            _detallePuro = detalle;
            _nombreProducto = nombreProducto;
        }

        // Propiedad exclusiva para la vista (no está en la tabla de la BD)
        public string NombreProducto
        {
            get { return _nombreProducto; }
            set { _nombreProducto = value; OnPropertyChanged(); }
        }

        public int Cantidad
        {
            get { return _detallePuro.Cantidad; }
            set
            {
                _detallePuro.Cantidad = value;
                OnPropertyChanged();
                // Si cambia la cantidad, avisamos a la vista que el Subtotal también cambió
                OnPropertyChanged(nameof(Subtotal));
            }
        }

        public decimal PrecioUnitario
        {
            get { return _detallePuro.Precio_Unitario; }
            set { _detallePuro.Precio_Unitario = value; OnPropertyChanged(); }
        }

        // El subtotal se calcula automáticamente sin necesidad de una variable extra
        public decimal Subtotal => Cantidad * PrecioUnitario;
    }
}