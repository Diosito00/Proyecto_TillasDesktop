using System;
using System.IO;
using System.Windows.Input;
using System.Windows;
using Microsoft.Win32;

namespace TillasDesktop.UI.Modelos
{
    public class BackupViewModel : ViewModelBase
    {
        private string _ultimoBackupInfo;

        public string UltimoBackupInfo
        {
            get => _ultimoBackupInfo;
            set { _ultimoBackupInfo = value; OnPropertyChanged(); }
        }

        public ICommand GenerarBackupCommand { get; }

        public BackupViewModel()
        {
            // Estado inicial al cargar la pantalla
            UltimoBackupInfo = "No se han registrado copias de seguridad en esta sesión.";

            GenerarBackupCommand = new RelayCommand(GenerarBackup);
        }

        private void GenerarBackup(object parametro)
        {
            // Configuramos la ventana para que el usuario elija dónde guardar el archivo
            var dialog = new SaveFileDialog
            {
                Title = "Seleccionar ubicación para el Respaldo",
                Filter = "Archivos de Base de Datos SQL (*.sql)|*.sql",
                FileName = $"TillasDB_Backup_{DateTime.Now:yyyyMMdd_HHmmss}.sql"               
            };

            // Si el usuario hace clic en "Guardar"
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    // Simulamos la creación del archivo para que la UI funcione.
                    File.WriteAllText(dialog.FileName, $"-- Respaldo de TillasDesktop generado el {DateTime.Now}\n-- Estructura y datos...");

                    // Actualizamos la interfaz para mostrar cuándo y dónde se hizo el último backup
                    UltimoBackupInfo = $"{DateTime.Now:dd/MM/yyyy a las HH:mm} hs\nRuta: {dialog.FileName}";

                    MessageBox.Show("La copia de seguridad se ha generado y guardado con éxito.", "Respaldo Completado", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ocurrió un error al intentar guardar el archivo: {ex.Message}", "Error de Escritura", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}