// Importa las expresiones regulares (Regular Expressions) para validar formatos de texto como DNI y correos electrónicos
using System.Text.RegularExpressions;
// Importa clases base de WPF para la gestión de la aplicación y cuadros de mensaje interactivos
using System.Windows;
// Importa las utilidades de comandos y eventos de entrada de la interfaz de usuario en WPF
using System.Windows.Input;
// Importa los servicios de la Capa de Negocios (BLL) dedicados a la gestión de usuarios
using TillasDesktop.BLL.Services;
// Importa colecciones genéricas para manejar listas en memoria (como la lista de roles)
using System.Collections.Generic;
// Importa las entidades del dominio de usuarios
using TillasDesktop.Entities.Usuarios;

// Define el espacio de nombres (namespace) correspondiente al modelo de vistas (ViewModels) de la interfaz gráfica del sistema
namespace TillasDesktop.UI.Modelos
{
    // Declara la clase pública FormularioUsuarioViewModel que hereda de ViewModelBase, encargada de administrar la lógica de alta, edición y validación de usuarios y operadores
    public class FormularioUsuarioViewModel : ViewModelBase
    {
        // Instancia el servicio directamente para manejar la persistencia y reglas de negocio de los usuarios.
        private readonly UsuariosService _usuarioService = new UsuariosService();

        // Propiedades de control para la interfaz que cambian dinámicamente según la operación (Alta o Edición).
        public string TituloFormulario { get; set; }
        public bool EsModoEdicion { get; set; }
        public string MensajePassword { get; set; }
        // Lista desplegable con los roles disponibles que puede tener un operador en el sistema
        public List<string> ListaRoles { get; set; }

        // Contiene a la entidad Usuario envuelta, exponiendo sus propiedades con notificación (OnPropertyChanged) para que la vista reaccione.
        public UsuarioViewModel UsuarioActual { get; set; }

        // Campo privado para almacenar temporalmente la nueva contraseña ingresada en el formulario
        private string _nuevaPassword;
        // Propiedad pública vinculada a la caja de texto de contraseña con notificación de cambios
        public string NuevaPassword
        {
            get => _nuevaPassword;
            set { _nuevaPassword = value; OnPropertyChanged(); }
        }

        // Comando vinculado al botón de guardar del formulario de usuario.
        public ICommand GuardarCommand { get; private set; }

        // Acciones (Actions) que permiten al ViewModel dar órdenes a la Vista (cerrar la ventana o 
        // recargar la tabla principal) respetando el patrón MVVM sin acoplar código visual.
        public Action CerrarVentana { get; set; }
        public Action OnUsuarioGuardado { get; set; }

        // Constructor que se ejecuta al crear un usuario nuevo desde la grilla principal.
        public FormularioUsuarioViewModel()
        {
            EsModoEdicion = false;
            TituloFormulario = "DATOS DEL USUARIO (NUEVO)";
            MensajePassword = "* Obligatorio para usuarios nuevos.";

            // Creamos una entidad en blanco con valores lógicos predeterminados por conveniencia operativa
            var entidadNueva = new Usuario
            {
                // Sugerimos una fecha de nacimiento válida (20 años atrás) para facilitar la carga inicial
                Fecha_Nacimiento = DateTime.Now.AddYears(-20),
                Activo = true,
                Rol = "Vendedor"
            };

            // Envolvemos la entidad pura para que la interfaz pueda enlazarla (DataBinding).
            UsuarioActual = new UsuarioViewModel(entidadNueva);

            // Llama al método común de inicialización
            Inicializar();
        }

        // Constructor que se ejecuta al hacer clic en editar un usuario existente.
        public FormularioUsuarioViewModel(Usuario usuarioExistente)
        {
            EsModoEdicion = true;
            TituloFormulario = "DATOS DEL USUARIO (EDICIÓN)";
            MensajePassword = "* Dejar en blanco para mantener la contraseña actual.";

            // Envolvemos la entidad original (previamente clonada en GestionUsuariosViewModel) que vino de la base de datos.
            UsuarioActual = new UsuarioViewModel(usuarioExistente);

            // Llama al método común de inicialización
            Inicializar();
        }

        // Método auxiliar privado para no repetir código de configuración en ambos constructores.
        private void Inicializar()
        {
            // Define los roles permitidos dentro del sistema
            ListaRoles = new List<string> { "Admin", "Gerente", "Vendedor" };
            NuevaPassword = string.Empty;
            // Asocia el comando de guardado al método delegado correspondiente
            GuardarCommand = new RelayCommand(Guardar);
        }

        // Lógica principal de validación estricta y guardado contra la base de datos.
        private void Guardar(object parametro)
        {
            // Extraemos la entidad pura (sin código de UI) para mandarla a la Capa de Negocios.
            Usuario entidadParaGuardar = UsuarioActual.ObtenerEntidadPura();

            // Validaciones básicas de campos vacíos obligatorios.
            if (string.IsNullOrWhiteSpace(UsuarioActual.Nombre) ||
                string.IsNullOrWhiteSpace(UsuarioActual.Apellido) ||
                string.IsNullOrWhiteSpace(UsuarioActual.Nombre_Usuario) ||
                string.IsNullOrWhiteSpace(UsuarioActual.Rol))
            {
                MessageBox.Show("Todos los campos son obligatorios. Por favor, complete la información faltante.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return; // Corta la ejecución aquí mismo si falta información.
            }

            // Validar DNI mediante Expresión Regular (Regex): Obliga a que sean exactamente 8 números enteros.
            if (string.IsNullOrWhiteSpace(UsuarioActual.Dni) ||
                UsuarioActual.Dni.Length != 8 ||
                !Regex.IsMatch(UsuarioActual.Dni, @"^\d{8}$"))
            {
                MessageBox.Show("El DNI ingresado no es válido (debe contener exactamente 8 números).", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Validar Email mediante Regex: Asegura el correcto formato sintáctico usuario@dominio.extensión.
            if (string.IsNullOrWhiteSpace(UsuarioActual.Email) ||
                !Regex.IsMatch(UsuarioActual.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                MessageBox.Show("El formato del correo electrónico no es válido (ejemplo: usuario@dominio.com).", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Validar la Fecha de Nacimiento (Verifica obligatoriedad, coherencia temporal y mayoría de edad).
            if (UsuarioActual.Fecha_Nacimiento.Date == DateTime.MinValue.Date)
            {
                MessageBox.Show("La fecha de nacimiento es obligatoria.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (UsuarioActual.Fecha_Nacimiento.Date > DateTime.Today)
            {
                MessageBox.Show("La fecha de nacimiento no puede ser mayor a la fecha actual.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (UsuarioActual.Fecha_Nacimiento.Date > DateTime.Today.AddYears(-18))
            {
                MessageBox.Show("El usuario debe tener al menos 18 años.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Validar Contraseña: Solo es estrictamente obligatoria si estamos creando un usuario nuevo.
            if (!EsModoEdicion && string.IsNullOrWhiteSpace(NuevaPassword))
            {
                MessageBox.Show("La contraseña es obligatoria para nuevos usuarios.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Si el operador escribió algo en el campo de contraseña, se la asignamos a la entidad.
            // (Si lo dejó en blanco en modo edición, la entidad mantiene la contraseña anterior intacta).
            if (!string.IsNullOrWhiteSpace(NuevaPassword))
            {
                entidadParaGuardar.Password = NuevaPassword;
            }

            // Variable de control booleana para saber si la base de datos confirmó la transacción de persistencia.
            bool exito = false;

            try
            {
                // Decidimos si llamamos al servicio de actualización o al de creación según el modo en el que se encuentre el formulario.
                if (EsModoEdicion)
                {
                    exito = _usuarioService.ActualizarUsuario(entidadParaGuardar);
                }
                else
                {
                    exito = _usuarioService.CrearUsuario(entidadParaGuardar);

                    // Si se creó con éxito, la base de datos asigna un ID autoincremental. Se lo pasamos al envoltorio 
                    // para que la grilla principal refleje el ID real en lugar de un "0" inicial.
                    if (exito) UsuarioActual.Id_Usuario = entidadParaGuardar.Id_Usuario;
                }

                // Si todo salió bien en la persistencia, informamos al usuario, actualizamos la tabla principal y cerramos el formulario.
                if (exito)
                {
                    MessageBox.Show(EsModoEdicion ? "Usuario actualizado correctamente." : "Usuario creado correctamente.",
                                    "Operación Exitosa", MessageBoxButton.OK, MessageBoxImage.Information);

                    OnUsuarioGuardado?.Invoke(); // Dispara el delegado para avisar a la grilla principal que debe recargar los datos
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