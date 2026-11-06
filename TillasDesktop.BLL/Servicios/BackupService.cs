using System;
using TillasDesktop.DAL.Repositorios;

// Define el espacio de nombres (namespace) correspondiente a los servicios de la capa de lógica de negocio (BLL)
namespace TillasDesktop.BLL.Services
{

    // Declaración de la clase pública 'BackupService', encargada de gestionar las reglas de negocio de las copias de seguridad
    public class BackupService
    {

        // Declara un campo privado y de solo lectura que contendrá la instancia del repositorio de respaldos
        private readonly BackupRepository _backupRepo;

        // Constructor de la clase que se ejecuta al instanciar el servicio
        public BackupService()
        {
            // Inicializa la variable creando un nuevo objeto de la clase 'BackupRepository' (responsable de hablar con la base de datos)
            _backupRepo = new BackupRepository();
        }


        // Método público para la creación del respaldo, el cual recibe como parámetro la ruta (string) donde se guardará el archivo
        public void CrearRespaldo(string ruta)
        {
            try
            {
                // Intenta ejecutar el método 'GenerarBackup' del repositorio pasándole la ruta especificada
                _backupRepo.GenerarBackup(ruta);
            }

            // Si ocurre cualquier error o excepción durante el proceso
            catch (Exception ex)
            {
                // Si falla SQL, lanzamos un error personalizado indicando un posible problema de permisos de carpeta
                throw new Exception("Error al generar el archivo de respaldo.\nVerifica tener permisos para guardar en esta carpeta.\nDetalle: " + ex.Message);
            }
        }

        // Método público para la restauración del sistema, el cual recibe la ruta del archivo de respaldo a utilizar
        public void RestaurarSistema(string ruta)
        {
            try
            {
                // Intenta ejecutar el método 'RestaurarBackup' del repositorio pasándole la ruta del archivo
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