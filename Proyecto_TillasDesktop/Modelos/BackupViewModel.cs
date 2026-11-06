// Importa el espacio de nombres fundamental del sistema para tipos base y manejo de excepciones
using System;
// Importa clases base de Windows Presentation Foundation (WPF) para la gestión de la aplicación y cuadros de mensaje
using System.Windows;
// Importa las utilidades de comandos y eventos de entrada de la interfaz de usuario en WPF
using System.Windows.Input;
// Importa el espacio de nombres necesario para invocar las ventanas nativas del explorador de archivos de Windows (Guardar/Abrir archivo)
using Microsoft.Win32;
// Importa los servicios de la Capa de Negocios (BLL) encargados de procesar la lógica de respaldos y restauraciones
using TillasDesktop.BLL.Services;

// Define el espacio de nombres (namespace) correspondiente a los modelos de vista (ViewModels) de la interfaz de usuario del sistema
namespace TillasDesktop.UI.Modelos
{
    // Declara la clase pública BackupViewModel que hereda de ViewModelBase, implementando el patrón MVVM para la gestión de copias de seguridad
    public class BackupViewModel : ViewModelBase
    {
        // Declara una instancia de solo lectura del servicio de negocio para manejar las operaciones de backup y restore
        private readonly BackupService _backupService;

        // Campo privado que almacena el texto informativo del estado del último respaldo realizado en la sesión
        private string _ultimoBackupInfo;
        // Propiedad pública enlazada a la interfaz para mostrar cuándo se hizo el último respaldo y notificar cambios visuales
        public string UltimoBackupInfo
        {
            get => _ultimoBackupInfo;
            set { _ultimoBackupInfo = value; OnPropertyChanged(); }
        }

        // Comando público expuesto a la vista para ejecutar la acción de generar una copia de seguridad
        public ICommand GenerarBackupCommand { get; }
        // Comando público expuesto a la vista para ejecutar la acción de restaurar el sistema desde un archivo .bak
        public ICommand RestaurarBackupCommand { get; }

        // Constructor que inicializa el ViewModel, sus servicios y los comandos asociados
        public BackupViewModel()
        {
            // Instancia el servicio de respaldos de la capa de negocios
            _backupService = new BackupService();
            // Establece el mensaje inicial indicando que todavía no hay respaldos registrados en esta sesión
            UltimoBackupInfo = "No se han registrados copias de seguridad en esta sesión.";

            // Asocia el comando de generación con su respectivo método delegado
            GenerarBackupCommand = new RelayCommand(GenerarBackup);
            // Asocia el comando de restauración con su respectivo método delegado
            RestaurarBackupCommand = new RelayCommand(RestaurarBackup);
        }

        // Método privado que maneja la lógica de creación y guardado de un archivo de respaldo
        private void GenerarBackup(object parametro)
        {
            // Preparamos la ventana nativa de Windows para guardar archivos (SaveFileDialog)
            SaveFileDialog dialogoGuardar = new SaveFileDialog
            {
                // Define el título que tendrá la ventana de diálogo del sistema
                Title = "Guardar Copia de Seguridad",
                // Establece el filtro para obligar a que el archivo tenga formato .bak
                Filter = "Archivo de Base de Datos SQL (*.bak)|*.bak",
                // Genera un nombre de archivo automático combinando un prefijo con la fecha y hora actuales (formato yyyyMMdd_HHmm)
                FileName = $"TillasDB_Backup_{DateTime.Now:yyyyMMdd_HHmm}.bak"
            };

            // Si el usuario selecciona una ruta válida y presiona "Guardar" (ShowDialog devuelve true)
            if (dialogoGuardar.ShowDialog() == true)
            {
                try
                {
                    // Delegamos la acción a la capa de negocios enviando la ruta elegida por el usuario
                    _backupService.CrearRespaldo(dialogoGuardar.FileName);

                    // Actualizamos la propiedad de texto en la interfaz para reflejar el éxito y la hora exacta de la copia
                    UltimoBackupInfo = $"Éxito: {DateTime.Now:dd/MM/yyyy HH:mm} hs";
                    // Muestra un mensaje emergente de éxito al operador
                    MessageBox.Show("La copia de seguridad se ha guardado correctamente.", "Respaldo Exitoso", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    // Atrapamos cualquier error imprevisto (ej. falta de permisos en el disco) y se lo mostramos al usuario en un cuadro de diálogo
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // Método privado que maneja la lógica de restauración de la base de datos a partir de un archivo existente
        private void RestaurarBackup(object parametro)
        {
            // Muestra una advertencia crítica inicial al usuario, ya que restaurar sobrescribe datos actuales
            var confirmacion = MessageBox.Show(
                "⚠️ ADVERTENCIA CRÍTICA ⚠️\n\nAl restaurar la base de datos, SE BORRARÁN todas las ventas, productos y cambios registrados después de la fecha de la copia de seguridad.\n\n¿Estás completamente seguro de que deseas sobreescribir el sistema actual?",
                "Confirmar Restauración", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            // Si el operador responde "No", cortamos la ejecución inmediatamente para evitar accidentes
            if (confirmacion != MessageBoxResult.Yes) return;

            // Instanciamos la ventana de autorización para requerir credenciales de un Gerente o Administrador
            var ventanaAutorizacion = new TillasDesktop.UI.Vistas.AutorizacionWindow();
            // Abre la ventana de manera modal y bloqueante, esperando el resultado booleano
            bool? resultadoAuth = ventanaAutorizacion.ShowDialog();

            // Verificamos si la autenticación se completó con éxito y si la propiedad de autorización está en true
            if (resultadoAuth != true || !ventanaAutorizacion.EstaAutorizado)
            {
                return; // Cortamos la ejecución si el usuario no fue autorizado correctamente
            }

            // Si fue autorizado, preparamos la ventana nativa de Windows para abrir y buscar el archivo .bak de respaldo
            OpenFileDialog dialogoAbrir = new OpenFileDialog
            {
                Title = "Seleccionar Archivo de Respaldo",
                Filter = "Archivo de Base de Datos SQL (*.bak)|*.bak"
            };

            // Si el usuario selecciona un archivo y presiona abrir
            if (dialogoAbrir.ShowDialog() == true)
            {
                try
                {
                    // Enviamos la ruta completa del archivo seleccionado a la capa de negocios para ejecutar la restauración del sistema
                    _backupService.RestaurarSistema(dialogoAbrir.FileName);

                    // Informamos al usuario que el proceso terminó con éxito
                    MessageBox.Show("El sistema se ha restaurado con éxito.\n\nPor motivos de seguridad y para refrescar los datos, el programa se cerrará. Por favor, vuelve a iniciar sesión.", "Restauración Completada", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Como cambiamos toda la base de datos, cerramos la aplicación por completo para limpiar la memoria caché y evitar conflictos con datos antiguos
                    Application.Current.Shutdown();
                }
                catch (Exception ex)
                {
                    // Capturamos y mostramos cualquier excepción surgida durante el proceso de restauración en SQL Server
                    MessageBox.Show(ex.Message, "Error de Restauración", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}