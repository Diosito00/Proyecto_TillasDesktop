using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using TillasDesktop.BLL.Services;
using TillasDesktop.Entities.Usuarios;

namespace TillasDesktop.UI.Modelos
{
    public class FormularioUsuarioViewModel : ViewModelBase
    {
        // === PROPIEDADES DE LA INTERFAZ ===
        private readonly UsuariosService _usuarioService;
        public string TituloFormulario { get; set; }
        public bool EsModoEdicion { get; set; }
        public string MensajePassword { get; set; }
        public List<string> ListaRoles { get; set; }

        // === EL ENVOLTORIO REACTIVO ===
        public UsuarioViewModel UsuarioActual { get; set; }

        private string _nuevaPassword;
        public string NuevaPassword
        {
            get => _nuevaPassword;
            set { _nuevaPassword = value; OnPropertyChanged(); }
        }

        // === COMANDOS ===
        public ICommand GuardarCommand { get; private set; }
        public Action CerrarVentana { get; set; }
        public Action OnUsuarioGuardado { get; set; }

        // ====================================================================
        // CONSTRUCTOR 1: NUEVO USUARIO
        // ====================================================================
        public FormularioUsuarioViewModel()
        {
            EsModoEdicion = false;
            TituloFormulario = "DATOS DEL USUARIO (NUEVO)";
            MensajePassword = "* Obligatorio para usuarios nuevos.";

            // Creamos una entidad en blanco con valores por defecto
            var entidadNueva = new Usuario
            {
                Fecha_Nacimiento = DateTime.Now.AddYears(-20),
                Activo = true,
                Rol = "Vendedor"
            };

            // La envolvemos para la vista
            UsuarioActual = new UsuarioViewModel(entidadNueva);

            Inicializar();
        }

        // ====================================================================
        // CONSTRUCTOR 2: EDITAR USUARIO
        // ====================================================================
        public FormularioUsuarioViewModel(Usuario usuarioExistente)
        {
            _usuarioService = new UsuariosService();
            EsModoEdicion = true;
            TituloFormulario = "DATOS DEL USUARIO (EDICIÓN)";
            MensajePassword = "* Dejar en blanco para mantener la contraseña actual.";

            // Envolvemos la entidad que vino de la base de datos
            UsuarioActual = new UsuarioViewModel(usuarioExistente);

            Inicializar();
        }

        private void Inicializar()
        {
            ListaRoles = new List<string> { "Admin", "Gerente", "Vendedor" };
            NuevaPassword = string.Empty; // Siempre en blanco al abrir la ventana
            GuardarCommand = new RelayCommand(Guardar);
        }

        // === LÓGICA DE GUARDADO ===
        private void Guardar(object parametro)
        {
            // Extraemos la entidad pura del envoltorio
            Usuario entidadParaGuardar = UsuarioActual.ObtenerEntidadPura();

            // 1. Validar que los campos de texto normales no estén vacíos
            if (string.IsNullOrWhiteSpace(UsuarioActual.Nombre) ||
                string.IsNullOrWhiteSpace(UsuarioActual.Apellido) ||
                string.IsNullOrWhiteSpace(UsuarioActual.Nombre_Usuario) ||
                string.IsNullOrWhiteSpace(UsuarioActual.Rol)
                )
            {
                MessageBox.Show("Todos los campos son obligatorios. Por favor, complete la información faltante.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 2. Validar DNI: Exactamente 8 caracteres numéricos
            if (string.IsNullOrWhiteSpace(UsuarioActual.Dni) ||
                UsuarioActual.Dni.Length != 8 ||
                !Regex.IsMatch(UsuarioActual.Dni, @"^\d{8}$"))
            {
                MessageBox.Show("El DNI ingresado no es válido (debe contener al menos 7 números).", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 3. Validar Email: Formato estándar (texto @ texto . texto)
            if (string.IsNullOrWhiteSpace(UsuarioActual.Email) ||
                !Regex.IsMatch(UsuarioActual.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                MessageBox.Show("El formato del correo electrónico no es válido (ejemplo: usuario@dominio.com).", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 4. Validar Fecha de Nacimiento (No futura y mayor de 18 años)
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

            // 5. Validar Contraseña: Obligatoria en creación, opcional en edición
            if (!EsModoEdicion && string.IsNullOrWhiteSpace(NuevaPassword))
            {
                MessageBox.Show("La contraseña es obligatoria para nuevos usuarios.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Solo tocamos la contraseña de la entidad si escribieron una nueva
            if (!string.IsNullOrWhiteSpace(NuevaPassword))
            {
                entidadParaGuardar.Password = NuevaPassword;
            }

            bool exito;

            if (EsModoEdicion)
            {
                exito = _usuarioService.ActualizarUsuario(entidadParaGuardar);
            }
            else
            {
                exito = _usuarioService.CrearUsuario(entidadParaGuardar);

                // Forzamos la actualización en la UI para que desaparezca el "0" y muestre el ID real
                if (exito) UsuarioActual.Id_Usuario = entidadParaGuardar.Id_Usuario;
            }

            if (exito)
            {
                MessageBox.Show(EsModoEdicion ? "Usuario actualizado correctamente." : "Usuario creado correctamente.",
                                "Operación Exitosa", MessageBoxButton.OK, MessageBoxImage.Information);

                OnUsuarioGuardado?.Invoke();
                CerrarVentana?.Invoke();
            }
        }
    }
}