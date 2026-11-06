// Importa clases fundamentales del sistema (como tipos de datos básicos)
using System;
// Importa colecciones genéricas de .NET, necesarias para manejar listas (List<T>)
using System.Collections.Generic;
// Importa las herramientas de la interfaz gráfica de WPF, específicamente para mostrar mensajes emergentes (MessageBox)
using System.Windows;
// Importa el espacio de nombres principal de la capa de acceso a datos (DAL)
using TillasDesktop.DAL;
// Importa las entidades o modelos de datos correspondientes a los clientes (la estructura del objeto Cliente)
using TillasDesktop.Entities.Clientes;
// Importa los repositorios específicos de la capa de datos para clientes
using TillasDesktop.DAL.Repositorios;


// Define el espacio de nombres (namespace) de la capa de lógica de negocio (BLL) para los servicios de clientes
namespace TillasDesktop.BLL
{
    // Declaración de la clase pública 'ClienteService', encargada de aplicar las reglas de negocio antes de guardar o modificar clientes
    public class ClienteService
    {
        // Instancia del repositorio encargada de las consultas y operaciones en la base de datos de clientes.
        private readonly ClienteRepository _repositorio;

        // Constructor de la clase que se ejecuta al instanciar el servicio
        public ClienteService()
        {
            // Inicializa la variable creando un nuevo objeto de la clase 'ClienteRepository'
            _repositorio = new ClienteRepository();
        }

        // Obtiene la lista completa de clientes registrados. 
        // Funciona como una pasarela simple que comunica el ViewModel con la capa de datos (DAL).
        public List<Cliente> ObtenerClientes()
        {
            return _repositorio.ObtenerTodos();
        }

        // Valida los datos obligatorios y solicita al repositorio la creación de un nuevo cliente.
        public bool CrearCliente(Cliente nuevoCliente)
        {
            // Verifica que el nombre o el apellido no estén vacíos, nulos o compuestos solo por espacios en blanco
            if (string.IsNullOrWhiteSpace(nuevoCliente.Nombre) || string.IsNullOrWhiteSpace(nuevoCliente.Apellido))
            {
                MessageBox.Show("El nombre y el apellido del cliente son obligatorios.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // Verifica que el CUIT del cliente no esté vacío
            if (string.IsNullOrWhiteSpace(nuevoCliente.CUIT))
            {
                MessageBox.Show("El CUIT del cliente es obligatorio.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // Si superó todas las validaciones, delega la inserción física de los datos a la capa de datos (DAL)
            _repositorio.Insertar(nuevoCliente);
            // Retorna 'true' indicando que la operación se completó correctamente
            return true;
        }

        // Valida que el ID sea correcto y procede a actualizar los datos del cliente en la base de datos.
        public bool ActualizarCliente(Cliente clienteActualizado)
        {
            //  Asegura que el ID del cliente sea válido 
            if (clienteActualizado.ID <= 0)
            {
                MessageBox.Show("ID de cliente inválido.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // Verifica que los campos obligatorios de texto no estén vacíos
            if (string.IsNullOrWhiteSpace(clienteActualizado.Nombre) || string.IsNullOrWhiteSpace(clienteActualizado.Apellido))
            {
                MessageBox.Show("El nombre y el apellido del cliente no pueden estar vacíos.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // Si todo está correcto, llama al repositorio para ejecutar la actualización en la base de datos
            _repositorio.Actualizar(clienteActualizado);
            return true;
        }


        // Método público para eliminar (o dar de baja) un cliente a partir de su ID numérico
        public bool Eliminar(int idCliente)
        {
            // Comprueba que el ID recibido sea un número válido y positivo
            if (idCliente <= 0)
                if (idCliente <= 0)
            {
                MessageBox.Show("ID de cliente inválido para eliminación.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // Si el ID es correcto, solicita al repositorio que ejecute la eliminación del registro
            _repositorio.Eliminar(idCliente);
            return true;
        }

    }
}