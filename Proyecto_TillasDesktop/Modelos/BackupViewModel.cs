using System;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32; // Necesario para invocar las ventanas nativas de Windows (Guardar/Abrir archivo)
using TillasDesktop.BLL.Services;

namespace TillasDesktop.UI.Modelos
{
    public class BackupViewModel : ViewModelBase
    {
        // Instancia del servicio de la Capa de Negocios
        private readonly BackupService _backupService;

        // Propiedad enlazada a la interfaz para mostrar cuándo se hizo el último respaldo
        private string _ultimoBackupInfo;
        public string UltimoBackupInfo
        {
            get => _ultimoBackupInfo;
            set { _ultimoBackupInfo = value; OnPropertyChanged(); }
        }

        public ICommand GenerarBackupCommand { get; }
        public ICommand RestaurarBackupCommand { get; }

        public BackupViewModel()
        {
            _backupService = new BackupService();
            UltimoBackupInfo = "No se han registrado copias de seguridad en esta sesión.";

            GenerarBackupCommand = new RelayCommand(GenerarBackup);
            RestaurarBackupCommand = new RelayCommand(RestaurarBackup);
        }

        private void GenerarBackup(object parametro)
        {
            // Preparamos la ventana nativa de Windows para guardar archivos
            SaveFileDialog dialogoGuardar = new SaveFileDialog
            {
                Title = "Guardar Copia de Seguridad",
                Filter = "Archivo de Base de Datos SQL (*.bak)|*.bak", // Forzamos a que sea formato .bak
                FileName = $"TillasDB_Backup_{DateTime.Now:yyyyMMdd_HHmm}.bak" // Nombre automático con fecha y hora
            };

            // Si el usuario elige una ruta y presiona "Guardar" (ShowDialog devuelve true)
            if (dialogoGuardar.ShowDialog() == true)
            {
                try
                {
                    // Delegamos la acción a la capa de negocios enviando la ruta elegida
                    _backupService.CrearRespaldo(dialogoGuardar.FileName);

                    // Actualizamos la interfaz para mostrar el éxito
                    UltimoBackupInfo = $"Éxito: {DateTime.Now:dd/MM/yyyy HH:mm} hs";
                    MessageBox.Show("La copia de seguridad se ha guardado correctamente.", "Respaldo Exitoso", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    // Atrapamos errores (ej. falta de permisos en el disco C:) y los mostramos al usuario
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void RestaurarBackup(object parametro)
        {
            // Advertencia inicial: Es una acción destructiva
            var confirmacion = MessageBox.Show(
                "⚠️ ADVERTENCIA CRÍTICA ⚠️\n\nAl restaurar la base de datos, SE BORRARÁN todas las ventas, productos y cambios registrados después de la fecha de la copia de seguridad.\n\n¿Estás completamente seguro de que deseas sobreescribir el sistema actual?",
                "Confirmar Restauración", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (confirmacion != MessageBoxResult.Yes) return; // Si dice "No", cortamos la ejecución

            // Solicitamos credenciales de un Gerente/Admin
            var ventanaAutorizacion = new TillasDesktop.UI.Vistas.AutorizacionWindow();
            bool? resultadoAuth = ventanaAutorizacion.ShowDialog(); // Abre modal bloqueante

            // Verificamos si la autorización fue exitosa (el código cruzado lo maneja la ventana en sí)
            if (resultadoAuth != true || !ventanaAutorizacion.EstaAutorizado)
            {
                return; // Cortamos si no se autorizó
            }

            // Si se autorizó, preparamos la ventana de Windows para buscar el archivo .bak
            OpenFileDialog dialogoAbrir = new OpenFileDialog
            {
                Title = "Seleccionar Archivo de Respaldo",
                Filter = "Archivo de Base de Datos SQL (*.bak)|*.bak"
            };

            if (dialogoAbrir.ShowDialog() == true)
            {
                try
                {
                    // Enviamos la ruta del archivo a la capa de negocios para restaurarlo
                    _backupService.RestaurarSistema(dialogoAbrir.FileName);

                    MessageBox.Show("El sistema se ha restaurado con éxito.\n\nPor motivos de seguridad y para refrescar los datos, el programa se cerrará. Por favor, vuelve a iniciar sesión.", "Restauración Completada", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Como cambiamos toda la base de datos, cerramos programa para limpiar la memoria y evitar que los datos viejos causen conflictos.
                    Application.Current.Shutdown();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error de Restauración", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}