using Microsoft.Data.Sqlite;

namespace ArtesaniasPOS.Data.Database
{
    public static class DatabaseConfig
    {
        private static readonly string _dbPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "artesanias.db"
        );

        public static string ConnectionString =>
            $"Data Source={_dbPath}";

        public static SqliteConnection GetConnection()
        {
            var connection = new SqliteConnection(ConnectionString);
            return connection;
        }
    }
}