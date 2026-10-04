using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using TillasDesktop.BLL;
using TillasDesktop.BLL.Services;
using TillasDesktop.Entities.Clientes;

namespace TillasDesktop.UI.Modelos
{
    public class FormularioClienteViewModel : ViewModelBase
    {
        // Instanciamos el servicio directamente para manejar la persistencia de clientes.
        private readonly ClienteService _clienteService = new ClienteService();

        // Propiedades de control para la interfaz que cambian dinámicamente según la operación.
        public string TituloFormulario { get; set; }
        public bool EsModoEdicion { get; set; }

        // Contiene a la entidad Cliente real, pero expone sus propiedades con OnPropertyChanged para que la vista reaccione.
        public ClienteViewModel ClienteActual { get; set; }

        // Comando vinculado al botón de guardar del formulario.
        public ICommand GuardarCommand { get; private set; }

        // Acciones (Actions) que permiten al ViewModel dar órdenes a la Vista (cerrar la ventana o 
        // recargar la tabla principal) respetando el patrón MVVM sin acoplar código visual.
        public Action CerrarVentana { get; set; }
        public Action OnClienteGuardado { get; set; }

        // Constructor que se ejecuta al crear un cliente nuevo desde la grilla principal.
        public FormularioClienteViewModel()
        {
            EsModoEdicion = false;
            TituloFormulario = "DATOS DEL CLIENTE (NUEVO)";

            // Creamos una entidad en blanco con valores lógicos por defecto si es necesario.
            var entidadNueva = new Cliente();

            // Envolvemos la entidad pura para que la interfaz pueda enlazarla (DataBinding).
            ClienteActual = new ClienteViewModel(entidadNueva);

            Inicializar();
        }

        // Constructor que se ejecuta al hacer clic en editar un cliente existente.
        public FormularioClienteViewModel(Cliente clienteExistente)
        {
            EsModoEdicion = true;
            TituloFormulario = "DATOS DEL CLIENTE (EDICIÓN)";

            // Envolvemos la entidad original que vino de la base de datos.
            ClienteActual = new ClienteViewModel(clienteExistente);

            Inicializar();
        }

        // Método auxiliar para no repetir código en ambos constructores.
        private void Inicializar()
        {
            GuardarCommand = new RelayCommand(Guardar);
        }

        // Lógica principal de validación y guardado contra la base de datos.
        private void Guardar(object parametro)
        {
            // Validar que no queden campos vacíos o en blanco
            if (string.IsNullOrWhiteSpace(ClienteActual.Nombre) ||
                string.IsNullOrWhiteSpace(ClienteActual.Apellido) ||
                string.IsNullOrWhiteSpace(ClienteActual.CUIT) ||
                string.IsNullOrWhiteSpace(ClienteActual.Telefono) ||
                string.IsNullOrWhiteSpace(ClienteActual.Email))
            {
                MessageBox.Show("Todos los campos son obligatorios. Por favor, complete la información faltante.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return; // Corta la ejecución aquí mismo.
            }

            // Extraemos la entidad pura (sin código de UI) para mandarla a la Capa de Negocios.
            Cliente entidadParaGuardar = ClienteActual.ObtenerEntidadPura();

            // Validar que el CUIT contenga exactamente 11 dígitos numéricos (ignorando guiones).
            string cuitLimpio = ClienteActual.CUIT.Replace("-", "").Trim();
            if (cuitLimpio.Length != 11 || !cuitLimpio.All(char.IsDigit))
            {
                MessageBox.Show("El CUIT ingresado no es válido (debe contener exactamente 11 números).", "Validación de CUIT", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Limpiamos el teléfono de símbolos comunes (+, -, espacios) para contar únicamente los dígitos reales.
            string telefonoLimpio = new string(ClienteActual.Telefono.Where(char.IsDigit).ToArray());

            if (telefonoLimpio.Length < 8 || telefonoLimpio.Length > 15)
            {
                MessageBox.Show("El teléfono ingresado no es válido (debe contener entre 8 y 15 dígitos).", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (ClienteActual.Telefono.Trim().Length < 7)
            {
                MessageBox.Show("El teléfono ingresado es demasiado corto.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Validar el formato del correo electrónico mediante Expresiones Regulares (Regex).
            string patronEmail = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            if (!Regex.IsMatch(ClienteActual.Email.Trim(), patronEmail))
            {
                MessageBox.Show("El formato del correo electrónico no es válido (ejemplo: usuario@dominio.com).", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Validar que el nombre y el apellido no tengan números ni símbolos raros.
            if (!ClienteActual.Nombre.All(c => char.IsLetter(c) || char.IsWhiteSpace(c)) ||
                !ClienteActual.Apellido.All(c => char.IsLetter(c) || char.IsWhiteSpace(c)))
            {
                MessageBox.Show("El nombre y el apellido no deben contener números ni símbolos.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Variable de control para saber si la base de datos confirmó la transacción.
            bool exito = false;

            try
            {
                // Decidimos si llamamos al servicio de actualización o al de creación según el modo.
                if (EsModoEdicion)
                {
                    exito = _clienteService.ActualizarCliente(entidadParaGuardar);
                }
                else
                {
                    exito = _clienteService.CrearCliente(entidadParaGuardar);

                    // Si se creó con éxito, la BD asigna un ID autoincremental. Se lo pasamos al envoltorio 
                    // para que la grilla principal refleje el ID real en lugar de un "0".
                    if (exito) ClienteActual.ID = entidadParaGuardar.ID;
                }

                // Si todo salió bien, informamos al usuario, actualizamos la tabla principal y cerramos el formulario.
                if (exito)
                {
                    MessageBox.Show(EsModoEdicion ? "Cliente actualizado correctamente." : "Cliente creado correctamente.",
                                    "Operación Exitosa", MessageBoxButton.OK, MessageBoxImage.Information);

                    OnClienteGuardado?.Invoke(); // Avisa a la grilla principal que recargue
                    CerrarVentana?.Invoke();     // Cierra este formulario
                }
                else
                {
                    // Solo por precaución, si llegara a dar false pero no se lanza excepción
                    MessageBox.Show("No se pudo completar la operación en la base de datos.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                // Atrapamos cualquier error inesperado de red o de la capa de datos y lo mostramos claramente.
                MessageBox.Show(ex.Message, "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}