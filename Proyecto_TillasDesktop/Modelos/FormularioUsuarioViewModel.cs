using System.Windows;
using System.Windows.Input;
using System.Text.RegularExpressions;
using TillasDesktop.Entities.Usuarios;

namespace TillasDesktop.UI.Modelos
{
    public class FormularioUsuarioViewModel : ViewModelBase
    {
        // === PROPIEDADES DE LA INTERFAZ ===
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
            GuardarCommand = new RelayCommand(Guardar, PuedeGuardar);
        }

        // === LÓGICA DE GUARDADO ===
        private void Guardar(object parametro)
        {
            // Extraemos la entidad pura del envoltorio
            Usuario entidadParaGuardar = UsuarioActual.ObtenerEntidadPura();

            // Solo tocamos la contraseña de la entidad si escribieron una nueva
            if (!string.IsNullOrWhiteSpace(NuevaPassword))
            {
                // NOTA: Aquí tu BLL debería hashear la contraseña antes del UPDATE/INSERT
                entidadParaGuardar.Password = NuevaPassword;
            }

            // Aquí llamarías a: _usuariosService.GuardarUsuario(entidadParaGuardar);

            MessageBox.Show(EsModoEdicion ? "Usuario actualizado correctamente." : "Usuario creado correctamente.",
                            "Operación Exitosa", MessageBoxButton.OK, MessageBoxImage.Information);

            OnUsuarioGuardado?.Invoke();
            CerrarVentana?.Invoke();
        }

        private bool PuedeGuardar(object parametro)
        {
            // 1. Validar que los campos de texto normales no estén vacíos
            bool camposCompletos = !string.IsNullOrWhiteSpace(UsuarioActual.Nombre) &&
                                   !string.IsNullOrWhiteSpace(UsuarioActual.Apellido) &&
                                   !string.IsNullOrWhiteSpace(UsuarioActual.Nombre_Usuario) &&
                                   !string.IsNullOrWhiteSpace(UsuarioActual.Rol);

            // 2. Validar DNI: Exactamente 8 caracteres numéricos
            bool dniValido = !string.IsNullOrWhiteSpace(UsuarioActual.Dni) &&
                             UsuarioActual.Dni.Length == 8 &&
                             Regex.IsMatch(UsuarioActual.Dni, @"^\d{8}$");

            // 3. Validar Email: Formato estándar (texto @ texto . texto)
            bool emailValido = !string.IsNullOrWhiteSpace(UsuarioActual.Email) &&
                               Regex.IsMatch(UsuarioActual.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");

            // 4. Validar Contraseña: Obligatoria en creación, opcional en edición
            bool passwordValida = EsModoEdicion || !string.IsNullOrWhiteSpace(NuevaPassword);

            // El botón GUARDAR se habilitará SOLO si las 4 condiciones son verdaderas
            return camposCompletos && dniValido && emailValido && passwordValida;
        }
    }
}