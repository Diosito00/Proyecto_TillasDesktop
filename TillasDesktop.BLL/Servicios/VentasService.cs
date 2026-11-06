// Importa clases fundamentales del sistema (como la clase base Exception)
using System;
// Importa colecciones genéricas para manejar listas de objetos (List<T>)
using System.Collections.Generic;
// Importa LINQ para realizar consultas, sumas y operaciones funcionales sobre colecciones (.Sum, .Any, etc.)
using System.Linq;
// Importa el repositorio de ventas de la capa de acceso a datos (DAL)
using TillasDesktop.DAL.Repositorios;
// Importa las entidades o DTOs correspondientes a la facturación y el carrito de compras
using TillasDesktop.Entities.Facturacion;
// Importa las entidades de reportes (si fuera necesario algún DTO auxiliar vinculado)
using TillasDesktop.Entities.Reportes;


// Define el espacio de nombres de los servicios dentro de la capa de lógica de negocio (BLL)
namespace TillasDesktop.BLL.Services
{
    // Servicio encargado de procesar y coordinar todas las operaciones del punto de venta, gestión de carritos y transacciones de cobro
    public class VentasService
    {
        // Instancia privada y de solo lectura del repositorio que ejecuta las consultas SQL del punto de venta y transacciones
        private readonly VentaRepository _ventaRepo;

        // Constructor del servicio: inicializa la instancia del repositorio de ventas
        public VentasService()
        {
            _ventaRepo = new VentaRepository();
        }

        // Método para obtener el catálogo completo de productos disponibles para la venta en el POS (Punto de Venta)
        public List<ProductoCatalogoDTO> ObtenerCatalogoPuntoVenta()
        {
            try
            {
                // Llama al repositorio para traer los productos y sus precios desde la base de datos
                return _ventaRepo.ObtenerCatalogoCaja();
            }
            catch (Exception ex)
            {
                // Si ocurre una falla en la BD, muestra una alerta visual al cajero
                System.Windows.MessageBox.Show("Falla en la BD: " + ex.Message);
                // Si hay error de conexión, devolvemos una lista vacía para que la caja no explote
                return new List<ProductoCatalogoDTO>();
            }
        }

        // Método para obtener los medios o tipos de pago aceptados por el sistema (ej. Efectivo, Tarjeta, Transferencia)
        public List<TipoPago> ObtenerTiposPago()
        {
            // Pasa la solicitud directamente al repositorio de ventas
            return _ventaRepo.ObtenerTiposPago();
        }

        // Método para recuperar un carrito de compras pendiente o abierto asociado a un usuario específico
        public List<ItemCarritoDTO> RecuperarCarritoAbierto(int idUsuario)
        {
            // Validación: Si el ID de usuario no es válido, retorna una lista vacía de ítems
            if (idUsuario <= 0) return new List<ItemCarritoDTO>();

            // Consulta al repositorio las reservas vigentes de este usuario
            return _ventaRepo.ObtenerReservasUsuario(idUsuario);
        }

        // === GESTIÓN DE RESERVAS (CARRITO) ===

        // Limpia cualquier reserva huérfana de sesiones anteriores o cortes de luz.
        public void LimpiarReservasPendientes()
        {
            // Ejecuta el procedimiento de limpieza en la base de datos a través del repositorio
            _ventaRepo.EjecutarLimpieza();
        }

        // Solicita a la base de datos bloquear y reservar temporalmente stock físico para este carrito.
        public bool AgregarAlCarrito(int idUsuario, int idProductoTalle, int cantidad)
        {

            // Valida que la cantidad y el ID del talle del producto sean valores positivos mayores a cero
            if (cantidad <= 0 || idProductoTalle <= 0) return false;

            // Invoca al repositorio para registrar la reserva con un tiempo de validez predeterminado de 10 minutos
            return _ventaRepo.ReservarProducto(idUsuario, idProductoTalle, cantidad, 10);
        }

        // Libera el stock reservado si el cliente se arrepiente y decide quitar el producto del carrito.
        public bool QuitarDelCarrito(int idUsuario, int idProductoTalle, int cantidad)
        {
            // Valida que los parámetros numéricos sean correctos
            if (cantidad <= 0 || idProductoTalle <= 0) return false;

            // Solicita al repositorio devolver esas unidades al inventario activo
            return _ventaRepo.LiberarReserva(idUsuario, idProductoTalle, cantidad);
        }


        // === GESTIÓN DE LA VENTA FINAL ===

        // Método utilitario que calcula el monto total acumulado de la venta utilizando programación funcional con LINQ (`IEnumerable`).
        public decimal CalcularTotalVenta(IEnumerable<DetalleVenta> detalles)
        {
            // Si la lista de detalles es nula, el total es 0
            if (detalles == null) return 0;

            // Multiplica el precio unitario por la cantidad de cada renglón y suma todos los subtotales
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

            // Regla de Negocio 2: Integridad financiera. (los montos deben ser mayores a cero).
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


        // Método para obtener el desglose detallado de los productos que formaron parte de una venta ya realizada
        public List<LineaDetalleVentaDTO> ObtenerDetallesVenta(int ventaId)
        {
            try
            {
                // Valida que el ID de la venta sea válido
                if (ventaId <= 0) return new List<LineaDetalleVentaDTO>();

                // Consulta al repositorio el detalle de líneas de esa venta
                return _ventaRepo.ObtenerDetallesPorVenta(ventaId);
            }
            catch (Exception ex)
            {
                // Si ocurre un error, muestra una alerta gráfica y retorna una lista vacía
                System.Windows.MessageBox.Show("Error al obtener los detalles de la venta: " + ex.Message, "Error BD", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return new List<LineaDetalleVentaDTO>();
            }
        }
    }
}