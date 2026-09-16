using System.Collections.Generic;
using TillasDesktop.Entities.Inventario;

namespace TillasDesktop.BLL.Services
{
    public class InventarioService
    {
        public List<Marca> ObtenerMarcasActivas()
        {
            return new List<Marca>
            {
                new Marca { ID = 1, Nombre = "Nike" },
                new Marca { ID = 2, Nombre = "Adidas" }
            };
        }

        public List<Categoria> ObtenerCategoriasActivas()
        {
            return new List<Categoria>
            {
                new Categoria { ID = 1, Nombre = "Urbano" },
                new Categoria { ID = 2, Nombre = "Deportivo" }
            };
        }

        // Lógica de validación centralizada
        public bool RegistrarNuevoProducto(Producto nuevoProducto, out string mensajeRespuesta)
        {
            if (string.IsNullOrWhiteSpace(nuevoProducto.Codigo_Modelo))
            {
                mensajeRespuesta = "El código del modelo es obligatorio.";
                return false;
            }

            mensajeRespuesta = $"Modelo {nuevoProducto.Nombre} registrado correctamente en el sistema.";
            return true;
        }

        // Agrega este método dentro de tu clase InventarioService existente
        public bool RegistrarIngresoStock(ProductoTalle nuevoStock, out string mensajeRespuesta)
        {
            // Validamos directamente las propiedades de la entidad
            if (nuevoStock.Talle <= 0 || nuevoStock.Stock_Actual <= 0)
            {
                mensajeRespuesta = "El talle y la cantidad deben ser mayores a cero.";
                return false;
            }

            mensajeRespuesta = $"Se ingresaron {nuevoStock.Stock_Actual} unidades del talle {nuevoStock.Talle} exitosamente.";
            return true;
        }

        // Agrega estos dos métodos a tu InventarioService en la BLL
        public bool ActualizarProducto(Producto productoActualizado, out string mensajeRespuesta)
        {
            if (string.IsNullOrWhiteSpace(productoActualizado.Codigo_Modelo))
            {
                mensajeRespuesta = "El código no puede estar vacío.";
                return false;
            }

            mensajeRespuesta = "Producto actualizado correctamente.";
            return true;
        }

        public bool EliminarProducto(int productoId, out string mensajeRespuesta)
        {
            mensajeRespuesta = "Producto eliminado del sistema.";
            return true;
        }
    }
}