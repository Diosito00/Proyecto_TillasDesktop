using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using TillasDesktop.BLL.Services;
using System.Collections.Generic;
using TillasDesktop.Entities.Usuarios;

namespace TillasDesktop.UI.Modelos
{
    public class FormularioUsuarioViewModel : ViewModelBase
    {

        // Instanciamos el servicio directamente para manejar la persistencia.
        private readonly UsuariosService _usuarioService = new UsuariosService();

        // Propiedades de control para la interfaz que cambian dinámicamente según la operación.
        public string TituloFormulario { get; set; }
        public bool EsModoEdicion { get; set; }
        public string MensajePassword { get; set; }
        public List<string> ListaRoles { get; set; }

        
        // Contiene a la entidad Usuario real, pero expone sus propiedades con OnPropertyChanged para que la vista reaccione.
        public UsuarioViewModel UsuarioActual { get; set; }

        private string _nuevaPassword;
        public string NuevaPassword
        {
            get => _nuevaPassword;
            set { _nuevaPassword = value; OnPropertyChanged(); }
        }

        // Comando vinculado al botón de guardar del formulario.
        public ICommand GuardarCommand { get; private set; }

        // Acciones (Actions) que permiten al ViewModel dar órdenes a la Vista (cerrar la ventana o 
        // recargar la tabla principal) respetando el patrón MVVM sin acoplar código visual.
        public Action CerrarVentana { get; set; }
        public Action OnUsuarioGuardado { get; set; }

        // Constructor que Se ejecuta al crear un usuario nuevo desde la grilla principal.
        public FormularioUsuarioViewModel()
        {
            EsModoEdicion = false;
            TituloFormulario = "DATOS DEL USUARIO (NUEVO)";
            MensajePassword = "* Obligatorio para usuarios nuevos.";

            // Creamos una entidad en blanco con valores lógicos por defecto
            var entidadNueva = new Usuario
            {
                // Sugerimos una fecha válida (20 años atrás) para facilitar la carga
                Fecha_Nacimiento = DateTime.Now.AddYears(-20),
                Activo = true,
                Rol = "Vendedor"
            };

            // Envolvemos la entidad pura para que la interfaz pueda enlazarla (DataBinding).
            UsuarioActual = new UsuarioViewModel(entidadNueva);

            Inicializar();
        }

        // Constructor que Se ejecuta al hacer clic en editar un usuario existente.
        public FormularioUsuarioViewModel(Usuario usuarioExistente)
        {
            EsModoEdicion = true;
            TituloFormulario = "DATOS DEL USUARIO (EDICIÓN)";
            MensajePassword = "* Dejar en blanco para mantener la contraseña actual.";

            // Envolvemos la entidad original (previamente clonada en GestionUsuariosViewModel) que vino de la base de datos.
            UsuarioActual = new UsuarioViewModel(usuarioExistente);

            Inicializar();
        }

        // Método auxiliar para no repetir código en ambos constructores.
        private void Inicializar()
        {
            ListaRoles = new List<string> { "Admin", "Gerente", "Vendedor" };
            NuevaPassword = string.Empty;
            GuardarCommand = new RelayCommand(Guardar);
        }

        // Lógica principal de validación y guardado contra la base de datos.
        private void Guardar(object parametro)
        {
            // Extraemos la entidad pura (sin código de UI) para mandarla a la Capa de Negocios.
            Usuario entidadParaGuardar = UsuarioActual.ObtenerEntidadPura();

            // Validaciones básicas de campos vacíos.
            if (string.IsNullOrWhiteSpace(UsuarioActual.Nombre) ||
                string.IsNullOrWhiteSpace(UsuarioActual.Apellido) ||
                string.IsNullOrWhiteSpace(UsuarioActual.Nombre_Usuario) ||
                string.IsNullOrWhiteSpace(UsuarioActual.Rol))
            {
                MessageBox.Show("Todos los campos son obligatorios. Por favor, complete la información faltante.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return; // Corta la ejecución aquí mismo.
            }

            //  Validar DNI mediante Expresión Regular (Regex). Obliga a que sean exactamente 8 números enteros.
            if (string.IsNullOrWhiteSpace(UsuarioActual.Dni) ||
                UsuarioActual.Dni.Length != 8 ||
                !Regex.IsMatch(UsuarioActual.Dni, @"^\d{8}$"))
            {
                MessageBox.Show("El DNI ingresado no es válido (debe contener exactamente 8 números).", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            //  Validar Email mediante Regex: Asegura el formato usuario@dominio.extensión.
            if (string.IsNullOrWhiteSpace(UsuarioActual.Email) ||
                !Regex.IsMatch(UsuarioActual.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                MessageBox.Show("El formato del correo electrónico no es válido (ejemplo: usuario@dominio.com).", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Validar la Fecha de Nacimiento (Mayor de 18 años y que la fecha ingresada no sea superior a la actual).
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

            // Validar Contraseña: Solo es obligatoria si estamos creando un usuario nuevo.
            if (!EsModoEdicion && string.IsNullOrWhiteSpace(NuevaPassword))
            {
                MessageBox.Show("La contraseña es obligatoria para nuevos usuarios.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Si el usuario escribió algo en el campo de contraseña, se la asignamos a la entidad.
            // (Si lo dejó en blanco en modo edición, la entidad mantiene la contraseña vieja).
            if (!string.IsNullOrWhiteSpace(NuevaPassword))
            {
                entidadParaGuardar.Password = NuevaPassword;
            }

            // Variable de control para saber si la base de datos confirmó la transacción.
            bool exito = false;

            try
            {
                // Decidimos si llamamos al servicio de actualización o al de creación según el modo.
                if (EsModoEdicion)
                {
                    exito = _usuarioService.ActualizarUsuario(entidadParaGuardar);
                }
                else
                {
                    exito = _usuarioService.CrearUsuario(entidadParaGuardar);

                    // Si se creó con éxito, la BD asigna un ID autoincremental. Se lo pasamos al envoltorio 
                    // para que la grilla principal refleje el ID real en lugar de un "0".
                    if (exito) UsuarioActual.Id_Usuario = entidadParaGuardar.Id_Usuario;
                }

                // Si todo salió bien, informamos al usuario, actualizamos la tabla principal y cerramos el formulario.
                if (exito)
                {
                    MessageBox.Show(EsModoEdicion ? "Usuario actualizado correctamente." : "Usuario creado correctamente.",
                                    "Operación Exitosa", MessageBoxButton.OK, MessageBoxImage.Information);

                    OnUsuarioGuardado?.Invoke(); // Avisa a la grilla principal que recargue
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
                MessageBox.Show(ex.Message, "Error Crítico", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}