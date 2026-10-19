using System;
using System.Linq;
using System.Collections.Generic;
using TillasDesktop.DAL.Repositorios;
using TillasDesktop.Entities.Inventario;

// Definimos el espacio de nombres donde vive la capa de lógica de negocio (BLL).
namespace TillasDesktop.BLL.Services
{
    // Servicio de negocio para gestionar el inventario, productos, talles, marcas y categorías.
    public class InventarioService
    {
        // Variable privada que almacena la instancia del repositorio para comunicarse con la base de datos.
        private readonly InventarioRepository _inventarioRepo;

        // Constructor del servicio: inicializa el repositorio al crear el objeto.
        public InventarioService()
        {
            _inventarioRepo = new InventarioRepository();
        }

        // Método unificado de lectura de productos (idéntico a UsuariosService.ObtenerTodos)
        public List<Producto> ObtenerProductos(bool activos = true)
        {
            try
            {
                return _inventarioRepo.ObtenerProductos(activos);
            }
            catch (Exception)
            {
                return new List<Producto>();
            }
        }


        // Método para solicitar la lista de marcas que se encuentran activas en el sistema.
        public List<Marca> ObtenerMarcasActivas()
        {
            try
            {
                // Trae todas del repositorio y filtra en memoria las que están activas
                return _inventarioRepo.ObtenerMarcas().Where(m => m.Activo).ToList();
            }
            catch (Exception)
            {
                // Si ocurre un error de conexión o base de datos, devuelve una lista vacía para que la app no se rompa.
                return new List<Marca>();
            }
        }

        // Método para registrar una nueva marca validando primero que su nombre no esté vacío.
        public bool RegistrarMarca(Marca marca, out string mensaje)
        {
            // Validamos que el nombre de la marca no sea nulo, esté vacío o tenga solo espacios.
            if (string.IsNullOrWhiteSpace(marca.Nombre))
            {
                mensaje = "El nombre de la marca es obligatorio.";
                return false;
            }

            try
            {
                // Obtenemos todas las marcas guardadas en la BD
                List<Marca> todasLasMarcas = _inventarioRepo.ObtenerMarcas();

                // Buscamos coincidencia exacta por nombre (ignorando mayúsculas/minúsculas)
                Marca marcaExistente = todasLasMarcas.FirstOrDefault(m => m.Nombre.Trim().Equals(marca.Nombre.Trim(), StringComparison.OrdinalIgnoreCase));

                if (marcaExistente != null)
                {
                    if (marcaExistente.Activo)
                    {
                        mensaje = $"La marca '{marcaExistente.Nombre}' ya existe y está activa.";
                        return false;
                    }
                    else
                    {
                        // Si ya existía pero estaba inactiva, la reactivamos
                        marcaExistente.Activo = true;
                        bool reactivada = _inventarioRepo.ActualizarMarca(marcaExistente);

                        marca.ID = marcaExistente.ID;
                        marca.Activo = true;

                        mensaje = reactivada ? $"La marca '{marcaExistente.Nombre}' fue reactivada exitosamente." : "No se pudo reactivar la marca.";
                        return reactivada;
                    }
                }
                else
                {
                    // Si no existía, la insertamos como nueva
                    marca.Activo = true;
                    bool resultado = _inventarioRepo.InsertarMarca(marca);
                    mensaje = resultado ? "Marca registrada correctamente." : "No se pudo registrar la marca.";
                    return resultado;
                }
            }
            catch (Exception ex)
            {
                mensaje = "Error al registrar la marca: " + ex.Message;
                return false;
            }
        }

        // Método para dar de baja lógica una marca utilizando su ID.
        public bool EliminarMarca(int idMarca, out string mensaje)
        {
            try
            {
                // Llamamos al repositorio para hacer la baja lógica (pasar Activo a 0).
                bool resultado = _inventarioRepo.EliminarMarca(idMarca);
                mensaje = resultado ? "Marca eliminada correctamente." : "No se encontró la marca especificada.";
                return resultado;
            }
            catch (Exception ex)
            {
                mensaje = "Error al eliminar la marca: " + ex.Message;
                return false;
            }
        }

        // Método para obtener la lista de categorías activas desde la base de datos.
        public List<Categoria> ObtenerCategoriasActivas()
        {
            try
            {
                // Trae todas del repositorio y filtra en memoria las que están activas
                return _inventarioRepo.ObtenerCategorias().Where(c => c.Activo).ToList();
            }
            catch (Exception)
            {
                return new List<Categoria>();
            }
        }

        // Método para registrar una nueva categoría validando que el nombre no esté en blanco.
        public bool RegistrarCategoria(Categoria categoria, out string mensaje)
        {
            // Validamos que el nombre de la categoría sea válido.
            if (string.IsNullOrWhiteSpace(categoria.Nombre))
            {
                mensaje = "El nombre de la categoría es obligatorio.";
                return false;
            }

            try
            {
                // Obtenemos todas las categorías guardadas en la BD
                List<Categoria> todasLasCategorias = _inventarioRepo.ObtenerCategorias();

                // Buscamos coincidencia exacta por nombre
                Categoria categoriaExistente = todasLasCategorias.FirstOrDefault(c => c.Nombre.Trim().Equals(categoria.Nombre.Trim(), StringComparison.OrdinalIgnoreCase));

                if (categoriaExistente != null)
                {
                    if (categoriaExistente.Activo)
                    {
                        mensaje = $"La categoría '{categoriaExistente.Nombre}' ya existe y está activa.";
                        return false;
                    }
                    else
                    {
                        // Si ya existía pero estaba inactiva, la reactivamos
                        categoriaExistente.Activo = true;
                        bool reactivada = _inventarioRepo.ActualizarCategoria(categoriaExistente);

                        categoria.ID = categoriaExistente.ID;
                        categoria.Activo = true;

                        mensaje = reactivada ? $"La categoría '{categoriaExistente.Nombre}' fue reactivada exitosamente." : "No se pudo reactivar la categoría.";
                        return reactivada;
                    }
                }
                else
                {
                    // Si no existía, la insertamos nueva
                    categoria.Activo = true;
                    bool resultado = _inventarioRepo.InsertarCategoria(categoria);
                    mensaje = resultado ? "Categoría registrada correctamente." : "No se pudo registrar la categoría.";
                    return resultado;
                }
            }
            catch (Exception ex)
            {
                mensaje = "Error al registrar la categoría: " + ex.Message;
                return false;
            }
        }

        // Método para dar de baja lógica una categoría mediante su ID.
        public bool EliminarCategoria(int idCategoria, out string mensaje)
        {
            try
            {
                bool resultado = _inventarioRepo.EliminarCategoria(idCategoria);
                mensaje = resultado ? "Categoría eliminada correctamente." : "No se encontró la categoría especificada.";
                return resultado;
            }
            catch (Exception ex)
            {
                mensaje = "Error al eliminar la categoría: " + ex.Message;
                return false;
            }
        }



        // Método de negocio para validar y registrar un nuevo producto en la base de datos.
        public bool RegistrarNuevoProducto(Producto nuevoProducto, out string mensajeRespuesta)
        {
            // Validamos que el código del modelo (SKU) sea obligatorio.
            if (string.IsNullOrWhiteSpace(nuevoProducto.Codigo_Modelo))
            {
                mensajeRespuesta = "El código del modelo (SKU) es obligatorio.";
                return false;
            }

            try
            {
                bool resultado = _inventarioRepo.Insertar(nuevoProducto);
                if (resultado)
                {
                    mensajeRespuesta = $"Modelo {nuevoProducto.Nombre} registrado correctamente.";
                    return true;
                }
                mensajeRespuesta = "No se pudo registrar el producto.";
                return false;
            }
            catch (Exception ex)
            {
                // Capturamos cualquier excepción personalizada (como SKU duplicado) lanzada por el repositorio.
                mensajeRespuesta = ex.Message;
                return false;
            }
        }

        // Método para validar y actualizar los datos de un producto ya existente.
        public bool ActualizarProducto(Producto productoActualizado, out string mensajeRespuesta)
        {
            if (string.IsNullOrWhiteSpace(productoActualizado.Codigo_Modelo))
            {
                mensajeRespuesta = "El código del modelo es obligatorio.";
                return false;
            }

            try
            {
                bool resultado = _inventarioRepo.Actualizar(productoActualizado);
                if (resultado)
                {
                    mensajeRespuesta = "Producto actualizado correctamente.";
                    return true;
                }
                mensajeRespuesta = "No se encontró el producto a modificar.";
                return false;
            }
            catch (Exception ex)
            {
                mensajeRespuesta = ex.Message;
                return false;
            }
        }

        // Método para realizar la baja lógica de un producto en el sistema.
        public bool EliminarProducto(int productoId, out string mensajeRespuesta)
        {
            try
            {
                bool resultado = _inventarioRepo.Eliminar(productoId);
                if (resultado)
                {
                    mensajeRespuesta = "Producto eliminado del sistema.";
                    return true;
                }
                mensajeRespuesta = "No se pudo encontrar el producto.";
                return false;
            }
            catch (Exception ex)
            {
                mensajeRespuesta = ex.Message;
                return false;
            }
        }

        // Método para obtener la lista de talles y stock asociados a un ID de producto determinado.
        public List<ProductoTalle> ObtenerTallesPorProducto(int productoId)
        {
            try
            {
                return _inventarioRepo.ObtenerTallesPorProducto(productoId);
            }
            catch (Exception)
            {
                return new List<ProductoTalle>();
            }
        }

        // Método para registrar un nuevo ingreso de stock por talle validando que los valores sean mayores a cero.
        public bool RegistrarIngresoStock(ProductoTalle nuevoStock, out string mensajeRespuesta)
        {
            if (nuevoStock.Talle <= 0 || nuevoStock.Stock_Actual <= 0)
            {
                mensajeRespuesta = "El talle y la cantidad deben ser mayores a cero.";
                return false;
            }

            try
            {
                bool resultado = _inventarioRepo.InsertarTalleStock(nuevoStock);
                if (resultado)
                {
                    mensajeRespuesta = $"Se ingresaron {nuevoStock.Stock_Actual} unidades del talle {nuevoStock.Talle}.";
                    return true;
                }
                mensajeRespuesta = "No se pudo registrar el ingreso de stock.";
                return false;
            }
            catch (Exception ex)
            {
                mensajeRespuesta = ex.Message;
                return false;
            }
        }

        // Método para actualizar la cantidad o número de un talle existente validando la coherencia de los datos.
        public bool ActualizarStockTalle(ProductoTalle talleActualizado, out string mensajeRespuesta)
        {
            if (talleActualizado.Talle <= 0 || talleActualizado.Stock_Actual < 0)
            {
                mensajeRespuesta = "El talle debe ser válido y el stock no puede ser negativo.";
                return false;
            }

            try
            {
                bool resultado = _inventarioRepo.ActualizarTalleStock(talleActualizado);
                if (resultado)
                {
                    mensajeRespuesta = "Stock del talle actualizado correctamente.";
                    return true;
                }
                mensajeRespuesta = "No se encontró el talle a actualizar.";
                return false;
            }
            catch (Exception ex)
            {
                mensajeRespuesta = ex.Message;
                return false;
            }
        }
    }
}