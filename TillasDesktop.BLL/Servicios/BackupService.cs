using System;
using TillasDesktop.DAL.Repositorios;

namespace TillasDesktop.BLL.Services
{
    public class BackupService
    {
        private readonly BackupRepository _backupRepo;

        public BackupService()
        {
            _backupRepo = new BackupRepository();
        }

        // Creación del respaldo
        public void CrearRespaldo(string ruta)
        {
            try
            {
                _backupRepo.GenerarBackup(ruta);
            }
            catch (Exception ex)
            {
                // Si falla SQL, lanzamos un error personalizado indicando un posible problema de permisos de carpeta
                throw new Exception("Error al generar el archivo de respaldo.\nVerifica tener permisos para guardar en esta carpeta.\nDetalle: " + ex.Message);
            }
        }

        // Restauración del sistema
        public void RestaurarSistema(string ruta)
        {
            try
            {
                _backupRepo.RestaurarBackup(ruta);
            }
            catch (Exception ex)
            {
                // Si falla la restauración, encapsulamos el error para que la interfaz gráfica lo atrape
                throw new Exception("Error crítico al intentar restaurar la base de datos.\nDetalle: " + ex.Message);
            }
        }
    }
}