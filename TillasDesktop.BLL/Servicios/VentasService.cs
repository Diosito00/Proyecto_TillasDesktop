using System;
using System.Collections.Generic;
using System.Linq;
using TillasDesktop.DAL.Repositorios;
using TillasDesktop.Entities.Facturacion;

namespace TillasDesktop.BLL.Services
{
    public class VentasService
    {
        // Instancias de la capa de acceso a datos (DAL)
        private readonly VentaRepository _ventaRepo;

        public VentasService()
        {
            _ventaRepo = new VentaRepository();
        }

        public List<ProductoCatalogoDTO> ObtenerCatalogoPuntoVenta()
        {
            try
            {
                return _ventaRepo.ObtenerCatalogoCaja();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Falla en la BD: " + ex.Message);
                // Si hay error de conexión, devolvemos una lista vacía para que la caja no explote
                return new List<ProductoCatalogoDTO>();
            }
        }

        public List<TipoPago> ObtenerTiposPago()
        {
            return _ventaRepo.ObtenerTiposPago();
        }

        public List<ItemCarritoDTO> RecuperarCarritoAbierto(int idUsuario)
        {
            if (idUsuario <= 0) return new List<ItemCarritoDTO>();
            return _ventaRepo.ObtenerReservasUsuario(idUsuario);
        }

        // === GESTIÓN DE RESERVAS (CARRITO) ===

        // Limpia cualquier reserva huérfana de sesiones anteriores o cortes de luz.
        public void LimpiarReservasPendientes()
        {
            _ventaRepo.EjecutarLimpieza();
        }

        // Solicita a la base de datos bloquear el stock para este carrito.
        public bool AgregarAlCarrito(int idUsuario, int idProductoTalle, int cantidad)
        {
            if (cantidad <= 0 || idProductoTalle <= 0) return false;

            // Tiempo de validez de la reserva por defecto: 10 minutos
            return _ventaRepo.ReservarProducto(idUsuario, idProductoTalle, cantidad, 10);
        }

        // Libera la reserva si el cliente se arrepiente y quita el producto.
        public bool QuitarDelCarrito(int idUsuario, int idProductoTalle, int cantidad)
        {
            if (cantidad <= 0 || idProductoTalle <= 0) return false;

            return _ventaRepo.LiberarReserva(idUsuario, idProductoTalle, cantidad);
        }


        // === GESTIÓN DE LA VENTA FINAL ===

        // Método utilitario que mantiene su versatilidad al usar IEnumerable y LINQ.
        public decimal CalcularTotalVenta(IEnumerable<DetalleVenta> detalles)
        {
            if (detalles == null) return 0;
            return detalles.Sum(item => item.Precio_Unitario * item.Cantidad);
        }

        // Valida las reglas de negocio antes de enviar la orden de cobro a la base de datos.
        public bool RegistrarVenta(Venta nuevaVenta, Pago nuevoPago, List<DetalleVenta> carrito, out string mensajeRespuesta)
        {
            // Regla de Negocio 1: El carrito no puede estar vacío.
            if (carrito == null || !carrito.Any())
            {
                mensajeRespuesta = "No hay productos en el carrito para cobrar.";
                return false;
            }

            // Regla de Negocio 2: Integridad financiera.
            if (nuevaVenta.Total <= 0 || nuevoPago.Monto <= 0)
            {
                mensajeRespuesta = "El monto total de la venta debe ser mayor a cero.";
                return false;
            }

            // Regla de Negocio 3: Consistencia de datos del usuario y cliente.
            if (nuevaVenta.Usuario_ID <= 0 || nuevaVenta.Cliente_ID <= 0)
            {
                mensajeRespuesta = "Faltan datos obligatorios del cajero o del cliente.";
                return false;
            }

            // Si pasa las validaciones, delegamos la transacción a la capa de datos.
            return _ventaRepo.RegistrarVenta(nuevaVenta, nuevoPago, carrito, out mensajeRespuesta);
        }
    }
}