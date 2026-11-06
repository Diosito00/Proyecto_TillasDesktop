// Importa las colecciones observables de WPF para actualizar automáticamente las grillas (DataGrid) al cargar los ítems de la venta
using System.Collections.ObjectModel;
// Importa los servicios de la Capa de Negocios (BLL) encargados de gestionar la lógica de ventas y consultas de comprobantes
using TillasDesktop.BLL.Services;
// Importa las entidades y objetos de transferencia de datos (DTOs) de reportes y detalles de ventas
using TillasDesktop.Entities.Reportes;

// Define el espacio de nombres (namespace) correspondiente a los modelos de vista (ViewModels) de la interfaz de usuario del sistema
namespace TillasDesktop.UI.Modelos
{
    // Declara la clase pública DetalleVentaViewModel que hereda de ViewModelBase, encargada de administrar la información y los ítems de un comprobante de venta específico
    public class DetalleVentaViewModel : ViewModelBase
    {
        // Instancia de solo lectura del servicio de negocio para consultar los detalles de la venta en la base de datos
        private readonly VentasService _ventasService;

        // Propiedad pública que almacena el identificador único de la venta que se está consultando
        public int VentaID { get; set; }
        // Propiedad de tipo string que almacena la fecha y hora formateada de la transacción
        public string Fecha { get; set; }
        // Propiedad de tipo string que almacena el nombre del cliente asociado a la venta
        public string Cliente { get; set; }
        // Propiedad de tipo string que almacena el nombre del vendedor que registró la operación
        public string Vendedor { get; set; }
        // Propiedad de tipo decimal que guarda el monto económico total final del comprobante
        public decimal Total { get; set; }

        // Colección observable tipada con el DTO (LineaDetalleVentaDTO) para mostrar dinámicamente los productos y renglones de esta venta en la interfaz gráfica
        public ObservableCollection<LineaDetalleVentaDTO> ProductosVendidos { get; set; }

        // Constructor principal que recibe un objeto dinámico (proveniente de la selección en la grilla de la ventana anterior)
        public DetalleVentaViewModel(dynamic ventaSeleccionada)
        {
            // Instancia el servicio de ventas de la capa de negocios
            _ventasService = new VentasService();

            // Asignamos los datos principales del encabezado extrayéndolos directamente de las propiedades del objeto seleccionado en la grilla
            VentaID = ventaSeleccionada.ID;
            Fecha = ventaSeleccionada.Fecha_Hora.ToString("dd/MM/yyyy HH:mm");
            Cliente = ventaSeleccionada.NombreCliente;
            Vendedor = ventaSeleccionada.NombreVendedor;
            Total = ventaSeleccionada.Total;

            // Inicializa la colección observable que contendrá los ítems del detalle
            ProductosVendidos = new ObservableCollection<LineaDetalleVentaDTO>();

            // Ejecuta el método para consultar y poblar los productos que componen la venta
            CargarDetallesVenta();
        }

        // Método privado encargado de consultar los ítems asociados a la venta actual y actualizar la colección de la vista
        private void CargarDetallesVenta()
        {
            // Llamamos al servicio de negocios pasando el ID de la venta actual para obtener la lista de renglones desde la base de datos
            var detallesDb = _ventasService.ObtenerDetallesVenta(VentaID);

            // Limpia la colección existente por seguridad ante posibles recargas
            ProductosVendidos.Clear();

            // Recorremos los resultados obtenidos de la base de datos y los poblamos en la ObservableCollection para que la vista gráfica (DataGrid) se actualice al instante
            foreach (var detalle in detallesDb)
            {
                ProductosVendidos.Add(detalle);
            }
        }
    }
}