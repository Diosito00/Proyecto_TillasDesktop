using System;
using System.IO;
using System.Windows.Input;
using System.Windows;
using Microsoft.Win32;

namespace TillasDesktop.UI.Modelos
{
    // Hereda de ViewModelBase para poder notificar a la interfaz gráfica (UI) cuando una propiedad cambia.
    public class BackupViewModel : ViewModelBase
    {
        // Campo privado que almacena el texto con la información del último respaldo.
        private string _ultimoBackupInfo;

        // Propiedad pública vinculada (binding) a la vista. 
        // El 'set' llama a OnPropertyChanged() para que la pantalla se actualice al instante si el texto cambia.
        public string UltimoBackupInfo
        {
            get => _ultimoBackupInfo;
            set { _ultimoBackupInfo = value; OnPropertyChanged(); }
        }

        // Comando que se ejecutará al presionar el botón "Generar Backup" en la interfaz.
        public ICommand GenerarBackupCommand { get; }

        public BackupViewModel()
        {
            // Estado inicial al abrir la pantalla de copias de seguridad.
            UltimoBackupInfo = "No se han registrado copias de seguridad en esta sesión.";

            // Vinculamos el comando con el método 'GenerarBackup' definido más abajo.
            GenerarBackupCommand = new RelayCommand(GenerarBackup);
        }

        private void GenerarBackup(object parametro)
        {
            // Instanciamos el cuadro de diálogo nativo de Windows para que el usuario elija dónde guardar el archivo.
            var dialog = new SaveFileDialog
            {
                Title = "Seleccionar ubicación para el Respaldo",
                // Forzamos a que el archivo solo pueda ser .sql
                Filter = "Archivos de Base de Datos SQL (*.sql)|*.sql",
                // Sugerimos un nombre de archivo automático basado en la fecha y hora actual (ej: TillasDB_Backup_20260921_053700.sql)
                FileName = $"TillasDB_Backup_{DateTime.Now:yyyyMMdd_HHmmss}.sql"
            };

            // ShowDialog() abre la ventana. Si el usuario hace clic en "Guardar", devuelve 'true'.
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    // Escribe físicamente un archivo de texto en la ruta elegida por el usuario.
                    // (Por ahora es una simulación de estructura SQL).
                    File.WriteAllText(dialog.FileName, $"-- Respaldo de TillasDesktop generado el {DateTime.Now}\n-- Estructura y datos...");

                    // Actualizamos la propiedad visual para mostrarle al usuario dónde quedó guardado.
                    UltimoBackupInfo = $"{DateTime.Now:dd/MM/yyyy a las HH:mm} hs\nRuta: {dialog.FileName}";

                    MessageBox.Show("La copia de seguridad se ha generado y guardado con éxito.", "Respaldo Completado", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    // Si falla (por ejemplo, si el usuario eligió una carpeta protegida donde no hay permisos de escritura).
                    MessageBox.Show($"Ocurrió un error al intentar guardar el archivo: {ex.Message}", "Error de Escritura", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}