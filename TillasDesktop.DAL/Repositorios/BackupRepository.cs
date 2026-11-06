// Importa clases fundamentales del sistema (como tipos básicos y manejo general)
using System;
// Importa el proveedor oficial de SQL Server para .NET (Microsoft.Data.SqlClient) para ejecutar comandos de base de datos
using Microsoft.Data.SqlClient;
// Importa la clase base de conexión de la capa de acceso a datos (DAL) para heredar los métodos de conexión
using TillasDesktop.DAL.Base;


// Define el espacio de nombres correspondiente a los repositorios dentro de la capa de acceso a datos (DAL)
namespace TillasDesktop.DAL.Repositorios
{
    // Declaración de la clase 'BackupRepository' que hereda de 'ConexionDb' para poder obtener la cadena de conexión fácilmente
    public class BackupRepository : ConexionDb
    {
        // Método público encargado de generar y exportar una copia de respaldo física (.bak) de la base de datos
        public void GenerarBackup(string rutaDestino)
        {
            // Utiliza un bloque 'using' para abrir y cerrar automáticamente la conexión a la base de datos
            using (var conexion = ObtenerConexion())
            {
                // Comando SQL nativo de SQL Server para respaldar la base de datos [TillasDB] hacia el archivo en disco especificado por la ruta.
                // FORMAT, INIT: Asegura que si el usuario sobreescribe un archivo .bak existente, 
                // este se formatee desde cero en lugar de "anexar" el nuevo respaldo al viejo, optimizando espacio.
                string query = $"BACKUP DATABASE [TillasDB] TO DISK = @Ruta WITH FORMAT, INIT, NAME = 'Respaldo Completo TillasDB'";

                // Crea el comando SQL pasándole la consulta y la conexión activa
                using (var cmd = new SqlCommand(query, conexion))
                {
                    // Agrega de forma segura el parámetro de la ruta para evitar inyecciones SQL
                    cmd.Parameters.AddWithValue("@Ruta", rutaDestino);
                    conexion.Open(); // Abre la conexión física con el motor SQL
                    cmd.ExecuteNonQuery(); // Ejecutamos sin esperar filas de retorno
                }
            }
        }


        // Método público encargado de restaurar el sistema completo a partir de un archivo de respaldo (.bak) previo
        public void RestaurarBackup(string rutaOrigen)
        {
            // Elimina cualquier conexion activa a la base de datos.
            SqlConnection.ClearAllPools();

            // No podemos restaurar TillasDB si estamos "parados" dentro de TillasDB. 
            // Usamos un constructor de conexión para reescribir la ruta y apuntar a la base de datos principal del motor ('master').
            var constructorConexion = new SqlConnectionStringBuilder(ObtenerConexion().ConnectionString)
            {
                // Cambia el destino de la conexión a la base de datos global del sistema SQL Server
                InitialCatalog = "master"
            };

            // Abre una conexión directa contra la base de datos del sistema 'master'
            using (var conexionMaster = new SqlConnection(constructorConexion.ConnectionString))
            {
                conexionMaster.Open();

                // USE master: Nos posicionamos en la base de sistema.
                // SINGLE_USER WITH ROLLBACK IMMEDIATE: Echa a cualquier otro empleado que esté usando TillasDB en ese momento.
                // RESTORE: Pega el archivo .bak encima de los datos actuales (REPLACE).
                // MULTI_USER: Vuelve a habilitar la base de datos para uso normal.
                string query = @"USE master;
                                ALTER DATABASE [TillasDB] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                                RESTORE DATABASE [TillasDB] FROM DISK = @Ruta WITH REPLACE;
                                ALTER DATABASE [TillasDB] SET MULTI_USER;";

                // Prepara el comando SQL para ser ejecutado contra la base 'master'
                using (var cmd = new SqlCommand(query, conexionMaster))
                {
                    // Inyectamos la ruta del archivo de origen de forma segura mediante parámetros
                    cmd.Parameters.AddWithValue("@Ruta", rutaOrigen);

                    // Ejecuta el bloque de restauración completo en SQL Server
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}