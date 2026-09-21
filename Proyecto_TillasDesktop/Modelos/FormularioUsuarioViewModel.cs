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

        // Inicializamos el servicio directamente aquí.
        private readonly UsuariosService _usuarioService = new UsuariosService();

        // Textos dinámicos que cambian dependiendo de si estamos creando o editando.
        public string TituloFormulario { get; set; }
        public bool EsModoEdicion { get; set; }
        public string MensajePassword { get; set; }
        public List<string> ListaRoles { get; set; }

        // === EL ENVOLTORIO REACTIVO ===
        // Contiene a la entidad Usuario real, pero expone sus propiedades con OnPropertyChanged para que la vista reaccione.
        public UsuarioViewModel UsuarioActual { get; set; }

        private string _nuevaPassword;
        public string NuevaPassword
        {
            get => _nuevaPassword;
            set { _nuevaPassword = value; OnPropertyChanged(); }
        }

        // === COMANDOS Y DELEGADOS ===
        public ICommand GuardarCommand { get; private set; }

        // Actions: Permiten que el ViewModel cierre la ventana o avise a la tabla principal que recargue los datos 
        // sin romper la arquitectura MVVM (el ViewModel no conoce a la Vista).
        public Action CerrarVentana { get; set; }
        public Action OnUsuarioGuardado { get; set; }

        // ====================================================================
        // CONSTRUCTOR 1: NUEVO USUARIO (Se llama al hacer clic en "Nuevo").
        // ====================================================================
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

            // La envolvemos para que el XAML pueda "bindearla" (Binding)
            UsuarioActual = new UsuarioViewModel(entidadNueva);

            Inicializar();
        }

        // ====================================================================
        // CONSTRUCTOR 2: EDITAR USUARIO (Se llama desde el botón de la grilla).
        // ====================================================================
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

        // === LÓGICA DE GUARDADO ===
        private void Guardar(object parametro)
        {
            // Extraemos la entidad pura (sin código de UI) para mandarla a la Capa de Negocios (BLL).
            Usuario entidadParaGuardar = UsuarioActual.ObtenerEntidadPura();

            // 1. Validaciones básicas de campos vacíos.
            if (string.IsNullOrWhiteSpace(UsuarioActual.Nombre) ||
                string.IsNullOrWhiteSpace(UsuarioActual.Apellido) ||
                string.IsNullOrWhiteSpace(UsuarioActual.Nombre_Usuario) ||
                string.IsNullOrWhiteSpace(UsuarioActual.Rol))
            {
                MessageBox.Show("Todos los campos son obligatorios. Por favor, complete la información faltante.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return; // Corta la ejecución aquí mismo.
            }

            // 2. Validar DNI mediante Expresión Regular (Regex). Obliga a que sean exactamente 8 números enteros.
            if (string.IsNullOrWhiteSpace(UsuarioActual.Dni) ||
                UsuarioActual.Dni.Length != 8 ||
                !Regex.IsMatch(UsuarioActual.Dni, @"^\d{8}$"))
            {
                MessageBox.Show("El DNI ingresado no es válido (debe contener al menos 7 números).", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 3. Validar Email mediante Regex: Asegura el formato usuario@dominio.extensión.
            if (string.IsNullOrWhiteSpace(UsuarioActual.Email) ||
                !Regex.IsMatch(UsuarioActual.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                MessageBox.Show("El formato del correo electrónico no es válido (ejemplo: usuario@dominio.com).", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 4. Validar la Fecha de Nacimiento (Mayor de 18 años y que la fecha ingresada no sea superior a la actual).
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

            // 5. Validar Contraseña: Solo es obligatoria si estamos creando un usuario nuevo.
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
            bool exito;

            // Ejecutamos el método correspondiente en la capa de servicios.
            if (EsModoEdicion)
            {
                exito = _usuarioService.ActualizarUsuario(entidadParaGuardar);
            }
            else
            {
                exito = _usuarioService.CrearUsuario(entidadParaGuardar);

                // Si se insertó con éxito, SQL Server generó un ID. Se lo inyectamos al envoltorio 
                // para que la tabla principal no muestre un "0".
                if (exito) UsuarioActual.Id_Usuario = entidadParaGuardar.Id_Usuario;
            }

            // Solo cerramos la ventana y avisamos si NO hubo errores de base de datos (Evita falsos positivos).
            if (exito)
            {
                MessageBox.Show(EsModoEdicion ? "Usuario actualizado correctamente." : "Usuario creado correctamente.",
                                "Operación Exitosa", MessageBoxButton.OK, MessageBoxImage.Information);

                OnUsuarioGuardado?.Invoke(); // Avisa a la grilla principal que recargue
                CerrarVentana?.Invoke();     // Cierra este formulario
            }
        }
    }
}