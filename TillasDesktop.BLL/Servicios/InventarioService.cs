using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using TillasDesktop.Entities.Inventario;

namespace TillasDesktop.BLL.Services
{
    public class InventarioService
    {
        // Estos métodos actualmente devuelven datos hardcodeados en memoria. 
        // En el futuro, aquí llamarás a _inventarioRepo.ObtenerMarcas() para traerlas de SQL Server.
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

        // ==========================================
        // LÓGICA DE VALIDACIÓN CENTRALIZADA
        // ==========================================
        // El uso del modificador 'out string mensajeRespuesta' es una excelente práctica. 
        // Permite devolver un 'bool' (para saber si falló o fue exitoso) y al mismo tiempo 
        // "enviar" un mensaje de texto explicativo sin necesidad de lanzar costosas Excepciones.

        public bool RegistrarNuevoProducto(Producto nuevoProducto, out string mensajeRespuesta)
        {
            // Defensa de la BLL: Aunque la UI ya validó esto, la capa de negocio no confía en nadie 
            // y vuelve a verificar las reglas críticas antes de tocar la base de datos.
            if (string.IsNullOrWhiteSpace(nuevoProducto.Codigo_Modelo))
            {
                mensajeRespuesta = "El código del modelo es obligatorio.";
                return false;
            }

            mensajeRespuesta = $"Modelo {nuevoProducto.Nombre} registrado correctamente en el sistema.";
            return true;
        }

        public bool RegistrarIngresoStock(ProductoTalle nuevoStock, out string mensajeRespuesta)
        {
            // Validamos que no intenten ingresar mercadería fantasma o talles negativos.
            if (nuevoStock.Talle <= 0 || nuevoStock.Stock_Actual <= 0)
            {
                mensajeRespuesta = "El talle y la cantidad deben ser mayores a cero.";
                return false;
            }

            mensajeRespuesta = $"Se ingresaron {nuevoStock.Stock_Actual} unidades del talle {nuevoStock.Talle} exitosamente.";
            return true;
        }

        public bool ActualizarStockTalle(ProductoTalle talleActualizado, out string mensajeRespuesta)
        {
            // A diferencia de un nuevo ingreso, en una actualización permitimos que el stock sea '0' 
            // (por si hubo un robo, merma o se vendió por otro canal xd).
            if (talleActualizado.Talle <= 0 || talleActualizado.Stock_Actual < 0)
            {
                mensajeRespuesta = "El talle debe ser válido y el stock no puede ser negativo.";
                return false;
            }

            mensajeRespuesta = $"Stock del talle {talleActualizado.Talle} actualizado correctamente a {talleActualizado.Stock_Actual} unidades.";
            return true;
        }

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
            // Aquí en el futuro verificarás que el producto no tenga ventas asociadas antes de borrarlo físicamente.
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

        public List<ProductoTalle> ObtenerTallesPorProducto(int productoId)
        {
            // Filtramos y devolvemos los talles que coincidan con el ID del producto
            return _tallesEnMemoria.Where(t => t.Producto_ID == productoId).ToList();
        }
    }
}