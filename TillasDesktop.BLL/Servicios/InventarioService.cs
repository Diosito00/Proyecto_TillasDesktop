using System.Collections.Generic;
using TillasDesktop.Entities.Inventario;

namespace TillasDesktop.BLL.Services
{
    public class InventarioService
    {
        // Proveedor de datos aislados (Próximamente conectados a DAL)
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
                new Categoria { ID = 1, Nombre = "Sneakers" },
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

            // FUTURO: Llamaremos a la capa de datos
            // Ej: _stockRepository.InsertarOActualizarTalle(nuevoStock);

            mensajeRespuesta = $"Se ingresaron {nuevoStock.Stock_Actual} unidades del talle {nuevoStock.Talle} exitosamente.";
            return true;
        }
    }
}