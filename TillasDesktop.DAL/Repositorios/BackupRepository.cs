using System;
using Microsoft.Data.SqlClient;
using TillasDesktop.DAL.Base;

namespace TillasDesktop.DAL.Repositorios
{
    public class BackupRepository : ConexionDb
    {
        public void GenerarBackup(string rutaDestino)
        {
            using (var conexion = ObtenerConexion())
            {
                // FORMAT, INIT: Asegura que si el usuario sobreescribe un archivo .bak existente, 
                // este se formatee desde cero en lugar de "anexar" el nuevo respaldo al viejo.
                string query = $"BACKUP DATABASE [TillasDB] TO DISK = @Ruta WITH FORMAT, INIT, NAME = 'Respaldo Completo TillasDB'";

                using (var cmd = new SqlCommand(query, conexion))
                {
                    cmd.Parameters.AddWithValue("@Ruta", rutaDestino);
                    conexion.Open();
                    cmd.ExecuteNonQuery(); // Ejecutamos sin esperar filas de retorno
                }
            }
        }

        public void RestaurarBackup(string rutaOrigen)
        {
            // Elimina cualquier conexion activa a la base de datos.
            SqlConnection.ClearAllPools();

            // No podemos restaurar TillasDB si estamos "parados" dentro de TillasDB. 
            // Usamos un constructor de conexión para reescribir la ruta y apuntar a la base de datos principal del motor ('master').
            var constructorConexion = new SqlConnectionStringBuilder(ObtenerConexion().ConnectionString)
            {
                InitialCatalog = "master"
            };

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

                using (var cmd = new SqlCommand(query, conexionMaster))
                {
                    // Inyectamos la ruta de forma segura para evitar inyecciones SQL
                    cmd.Parameters.AddWithValue("@Ruta", rutaOrigen);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}