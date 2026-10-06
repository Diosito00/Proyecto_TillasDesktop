using System;
using System.Collections.Generic;
using System.Windows;
using TillasDesktop.DAL;
using TillasDesktop.Entities.Clientes;
using TillasDesktop.DAL.Repositorios;

namespace TillasDesktop.BLL
{
    public class ClienteService
    {
        // Instancia del repositorio encargada de las consultas y operaciones en la base de datos de clientes.
        private readonly ClienteRepository _repositorio;

        public ClienteService()
        {
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
            if (string.IsNullOrWhiteSpace(nuevoCliente.Nombre) || string.IsNullOrWhiteSpace(nuevoCliente.Apellido))
            {
                MessageBox.Show("El nombre y el apellido del cliente son obligatorios.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(nuevoCliente.CUIT))
            {
                MessageBox.Show("El CUIT del cliente es obligatorio.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // Delegar la inserción a la capa de datos (DAL).
            _repositorio.Insertar(nuevoCliente);
            return true;
        }

        // Valida que el ID sea correcto y procede a actualizar los datos del cliente en la base de datos.
        public bool ActualizarCliente(Cliente clienteActualizado)
        {
            if (clienteActualizado.ID <= 0)
            {
                MessageBox.Show("ID de cliente inválido.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(clienteActualizado.Nombre) || string.IsNullOrWhiteSpace(clienteActualizado.Apellido))
            {
                MessageBox.Show("El nombre y el apellido del cliente no pueden estar vacíos.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            _repositorio.Actualizar(clienteActualizado);
            return true;
        }

       
        // Valida que el ID sea válido antes de gestionar la baja de un cliente.
        public bool Eliminar(int idCliente)
        {
            if (idCliente <= 0)
            {
                MessageBox.Show("ID de cliente inválido para eliminación.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            _repositorio.Eliminar(idCliente);
            return true;
        }

    }
}