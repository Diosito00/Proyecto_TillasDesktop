using System.Windows;
using System.Windows.Input;
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
            // Validamos directamente desde el envoltorio
            bool camposCompletos = !string.IsNullOrWhiteSpace(UsuarioActual.Nombre) &&
                                   !string.IsNullOrWhiteSpace(UsuarioActual.Apellido) &&
                                   !string.IsNullOrWhiteSpace(UsuarioActual.Nombre_Usuario) &&
                                   !string.IsNullOrWhiteSpace(UsuarioActual.Rol);

            // La contraseña debe estar validada según el modo
            bool passwordValida = EsModoEdicion ? true : !string.IsNullOrWhiteSpace(NuevaPassword);

            return camposCompletos && passwordValida;
        }
    }
}