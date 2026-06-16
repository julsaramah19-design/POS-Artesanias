using Microsoft.Data.Sqlite;

namespace ArtesaniasPOS.Data.Database
{
    /// <summary>
    /// Única fuente de verdad para la cadena de conexión. La ruta del archivo
    /// vive en <see cref="AppSettings.DbPath"/> (%APPDATA%), nunca en la carpeta
    /// del ejecutable (bin), que se borra al recompilar. Se activa
    /// <c>Foreign Keys</c> aquí para que TODAS las conexiones validen las llaves
    /// foráneas (en SQLite el PRAGMA es por conexión).
    /// </summary>
    public static class DatabaseConfig
    {
        public static string ConnectionString =>
            new SqliteConnectionStringBuilder
            {
                DataSource = AppSettings.DbPath,
                ForeignKeys = true
            }.ToString();

        public static SqliteConnection GetConnection()
        {
            var connection = new SqliteConnection(ConnectionString);
            return connection;
        }
    }
}