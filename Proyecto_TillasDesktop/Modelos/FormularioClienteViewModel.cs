// Importa espacios de nombres fundamentales para tipos base, manejo de excepciones y utilidades
using System;
// Importa colecciones genéricas para manipulación de datos en memoria
using System.Collections.Generic;
// Importa las extensiones de LINQ para consultas, filtrado y validación de colecciones y caracteres
using System.Linq;
// Importa las expresiones regulares (Regular Expressions) necesarias para validar formatos complejos como correos electrónicos
using System.Text.RegularExpressions;
// Importa clases base de WPF para la gestión de la aplicación y cuadros de mensaje interactivos
using System.Windows;
// Importa las utilidades de comandos y eventos de entrada de la interfaz de usuario en WPF
using System.Windows.Input;
// Importa las clases de la Capa de Negocios (BLL)
using TillasDesktop.BLL;
// Importa los servicios de la capa de negocios dedicados a la gestión de clientes
using TillasDesktop.BLL.Services;
// Importa las entidades del dominio de clientes
using TillasDesktop.Entities.Clientes;

// Define el espacio de nombres (namespace) correspondiente a los modelos de vista (ViewModels) de la interfaz de usuario del sistema
namespace TillasDesktop.UI.Modelos
{
    // Declara la clase pública FormularioClienteViewModel que hereda de ViewModelBase, encargada de administrar la lógica de alta y edición de clientes
    public class FormularioClienteViewModel : ViewModelBase
    {
        // Instanciamos el servicio directamente para manejar la persistencia y reglas de negocio de los clientes.
        private readonly ClienteService _clienteService = new ClienteService();

        // Propiedades de control para la interfaz que cambian dinámicamente según la operación (Alta o Edición).
        public string TituloFormulario { get; set; }
        public bool EsModoEdicion { get; set; }

        // Contiene a la entidad Cliente real envuelta, pero expone sus propiedades con notificación (OnPropertyChanged) para que la vista reaccione.
        public ClienteViewModel ClienteActual { get; set; }

        // Comando vinculado al botón de guardar del formulario de cliente.
        public ICommand GuardarCommand { get; private set; }

        // Acciones (Actions) que permiten al ViewModel dar órdenes a la Vista (cerrar la ventana o 
        // recargar la tabla principal) respetando el patrón MVVM sin acoplar código visual.
        public Action CerrarVentana { get; set; }
        public Action OnClienteGuardado { get; set; }

        // Constructor que se ejecuta al crear un cliente nuevo desde la grilla principal.
        public FormularioClienteViewModel()
        {
            // Configura las banderas de estado para el modo de alta/creación
            EsModoEdicion = false;
            TituloFormulario = "DATOS DEL CLIENTE (NUEVO)";

            // Creamos una entidad en blanco con valores por defecto para un nuevo cliente.
            var entidadNueva = new Cliente();

            // Envolvemos la entidad pura para que la interfaz pueda enlazarla mediante DataBinding.
            ClienteActual = new ClienteViewModel(entidadNueva);

            // Llama al método común de inicialización
            Inicializar();
        }

        // Constructor que se ejecuta al hacer clic en editar un cliente existente.
        public FormularioClienteViewModel(Cliente clienteExistente)
        {
            // Configura las banderas de estado para el modo de edición
            EsModoEdicion = true;
            TituloFormulario = "DATOS DEL CLIENTE (EDICIÓN)";

            // Envolvemos la entidad original que vino de la base de datos para su modificación.
            ClienteActual = new ClienteViewModel(clienteExistente);

            // Llama al método común de inicialización
            Inicializar();
        }

        // Método auxiliar privado para no repetir código de configuración de comandos en ambos constructores.
        private void Inicializar()
        {
            // Inicializa el comando de guardado asociándolo con el método delegado correspondiente
            GuardarCommand = new RelayCommand(Guardar);
        }

        // Lógica principal de validación estricta y guardado contra la base de datos.
        private void Guardar(object parametro)
        {
            // Validar que no queden campos vacíos, nulos o en blanco
            if (string.IsNullOrWhiteSpace(ClienteActual.Nombre) ||
                string.IsNullOrWhiteSpace(ClienteActual.Apellido) ||
                string.IsNullOrWhiteSpace(ClienteActual.CUIT) ||
                string.IsNullOrWhiteSpace(ClienteActual.Telefono) ||
                string.IsNullOrWhiteSpace(ClienteActual.Email))
            {
                MessageBox.Show("Todos los campos son obligatorios. Por favor, complete la información faltante.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return; // Corta la ejecución aquí mismo si hay campos vacíos.
            }

            // Extraemos la entidad pura (sin código de UI) para mandarla a la Capa de Negocios.
            Cliente entidadParaGuardar = ClienteActual.ObtenerEntidadPura();

            // Validar que el CUIT contenga exactamente 11 dígitos numéricos (ignorando guiones y espacios).
            string cuitLimpio = ClienteActual.CUIT.Replace("-", "").Trim();
            if (cuitLimpio.Length != 11 || !cuitLimpio.All(char.IsDigit))
            {
                MessageBox.Show("El CUIT ingresado no es válido (debe contener exactamente 11 números).", "Validación de CUIT", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Limpiamos el teléfono de símbolos comunes (+, -, espacios) para contar únicamente los dígitos reales.
            string telefonoLimpio = new string(ClienteActual.Telefono.Where(char.IsDigit).ToArray());

            // Validamos que la cantidad de dígitos limpios esté dentro de un rango lógico (entre 8 y 15)
            if (telefonoLimpio.Length < 8 || telefonoLimpio.Length > 15)
            {
                MessageBox.Show("El teléfono ingresado no es válido (debe contener entre 8 y 15 dígitos).", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Validación adicional de longitud mínima del texto ingresado en el teléfono
            if (ClienteActual.Telefono.Trim().Length < 7)
            {
                MessageBox.Show("El teléfono ingresado es demasiado corto.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Validar el formato sintáctico del correo electrónico mediante Expresiones Regulares (Regex).
            string patronEmail = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            if (!Regex.IsMatch(ClienteActual.Email.Trim(), patronEmail))
            {
                MessageBox.Show("El formato del correo electrónico no es válido (ejemplo: usuario@dominio.com).", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Validar que el nombre y el apellido no contengan números ni símbolos extraños, permitiendo solo letras y espacios.
            if (!ClienteActual.Nombre.All(c => char.IsLetter(c) || char.IsWhiteSpace(c)) ||
                !ClienteActual.Apellido.All(c => char.IsLetter(c) || char.IsWhiteSpace(c)))
            {
                MessageBox.Show("El nombre y el apellido no deben contener números ni símbolos.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Variable de control booleana para saber si la base de datos confirmó la transacción de persistencia.
            bool exito = false;

            try
            {
                // Decidimos si llamamos al servicio de actualización o al de creación según el modo en el que se encuentre el formulario.
                if (EsModoEdicion)
                {
                    exito = _clienteService.ActualizarCliente(entidadParaGuardar);
                }
                else
                {
                    exito = _clienteService.CrearCliente(entidadParaGuardar);

                    // Si se creó con éxito, la base de datos asigna un ID autoincremental. Se lo pasamos al envoltorio 
                    // para que la grilla principal refleje el ID real en lugar de un "0" inicial.
                    if (exito) ClienteActual.ID = entidadParaGuardar.ID;
                }

                // Si todo salió bien en la persistencia, informamos al usuario, actualizamos la tabla principal y cerramos el formulario.
                if (exito)
                {
                    MessageBox.Show(EsModoEdicion ? "Cliente actualizado correctamente." : "Cliente creado correctamente.",
                                    "Operación Exitosa", MessageBoxButton.OK, MessageBoxImage.Information);

                    OnClienteGuardado?.Invoke(); // Dispara el delegado para avisar a la grilla principal que debe recargar los datos
                    CerrarVentana?.Invoke();     // Dispara el delegado para cerrar este formulario de manera limpia
                }
                else
                {
                    // Advertencia de respaldo por si el servicio retorna false sin arrojar excepción
                    MessageBox.Show("No se pudo completar la operación en la base de datos.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                // Atrapamos cualquier error inesperado de red, restricciones únicas o de la capa de datos y lo mostramos claramente al operador.
                MessageBox.Show(ex.Message, "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}