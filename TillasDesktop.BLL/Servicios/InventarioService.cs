using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Linq; // Necesario para usar .Where() en la lista en memoria
using TillasDesktop.Entities.Inventario;

namespace TillasDesktop.BLL.Services
{
    public class InventarioService
    {
        // Devuelve las marcas activas disponibles. 
        // Actualmente usa datos estáticos para simular la base de datos; más adelante se conectará mediante el repositorio.
        public List<Marca> ObtenerMarcasActivas()
        {
            return new List<Marca>
            {
                new Marca { ID = 1, Nombre = "Nike" },
                new Marca { ID = 2, Nombre = "Adidas" }
            };
        }

        // Devuelve las categorías habilitadas para clasificar las zapatillas 
        public List<Categoria> ObtenerCategoriasActivas()
        {
            return new List<Categoria>
            {
                new Categoria { ID = 1, Nombre = "Urbano" },
                new Categoria { ID = 2, Nombre = "Deportivo" }
            };
        }

        // Valida que el producto tenga un código de modelo válido antes de darlo de alta en el sistema.
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

        // Controla que al ingresar stock la cantidad y el talle sean mayores a cero.
        public bool RegistrarIngresoStock(ProductoTalle nuevoStock, out string mensajeRespuesta)
        {
            if (nuevoStock.Talle <= 0 || nuevoStock.Stock_Actual <= 0)
            {
                mensajeRespuesta = "El talle y la cantidad deben ser mayores a cero.";
                return false;
            }

            mensajeRespuesta = $"Se ingresaron {nuevoStock.Stock_Actual} unidades del talle {nuevoStock.Talle} exitosamente.";
            return true;
        }

        // Actualiza el stock de un talle existente asegurando que no tome valores negativos.
        public bool ActualizarStockTalle(ProductoTalle talleActualizado, out string mensajeRespuesta)
        {
            
            if (talleActualizado.Talle <= 0 || talleActualizado.Stock_Actual < 0)
            {
                mensajeRespuesta = "El talle debe ser válido y el stock no puede ser negativo.";
                return false;
            }

            mensajeRespuesta = $"Stock del talle {talleActualizado.Talle} actualizado correctamente a {talleActualizado.Stock_Actual} unidades.";
            return true;
        }

        // Modifica los datos generales de un producto validando que su código no quede vacío.
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


        // Elimina un producto del sistema. 
        // luego deberemos validar que no tenga ventas asociadas antes de borrarlo.
        public bool EliminarProducto(int productoId, out string mensajeRespuesta)
        {
            
            mensajeRespuesta = "Producto eliminado del sistema.";
            return true;
        }

        // Guardamos los talles en una lista estática interna para que no se pierdan al cambiar de ventana
        private static List<ProductoTalle> _tallesEnMemoria = new List<ProductoTalle>
        {
        // Talles iniciales para el Air Force 1 (ID = 1)
        new ProductoTalle { ID = 1, Producto_ID = 1, Talle = 39, Stock_Actual = 20 },
        new ProductoTalle { ID = 2, Producto_ID = 1, Talle = 42, Stock_Actual = 25 },
        
        // Talles iniciales para el Samba OG (ID = 2) 
        new ProductoTalle { ID = 3, Producto_ID = 2, Talle = 35, Stock_Actual = 10 },
        new ProductoTalle { ID = 4, Producto_ID = 2, Talle = 40, Stock_Actual = 15 }
         };


        // Filtra y devuelve los talles y stock que pertenecen a un producto específico usando su ID.
        public List<ProductoTalle> ObtenerTallesPorProducto(int productoId)
        {
            return _tallesEnMemoria.Where(t => t.Producto_ID == productoId).ToList();
        }
    }
}