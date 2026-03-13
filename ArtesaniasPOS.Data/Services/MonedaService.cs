using Dapper;
using Microsoft.Data.Sqlite;
using ArtesaniasPOS.Core.Interfaces;

namespace ArtesaniasPOS.Data.Services
{
    /// <summary>
    /// Implementación concreta de IMonedaService usando Dapper + SQLite.
    /// </summary>
    public class MonedaService : IMonedaService
    {
        private readonly string _connectionString;

        public MonedaService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IEnumerable<MonedaDto>> ObtenerActivasAsync()
        {
            using var connection = new SqliteConnection(_connectionString);
            return await connection.QueryAsync<MonedaDto>(
                "SELECT Id, Codigo, Nombre, Simbolo, TasaCambio, EsMonedaBase FROM Moneda WHERE Activo = 1");
        }

        public async Task EstablecerMonedaBaseAsync(int monedaId)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            using var transaction = connection.BeginTransaction();
            try
            {
                await connection.ExecuteAsync(
                    "UPDATE Moneda SET EsMonedaBase = 0 WHERE EsMonedaBase = 1",
                    transaction: transaction);

                await connection.ExecuteAsync(
                    "UPDATE Moneda SET EsMonedaBase = 1, TasaCambio = 1.0 WHERE Id = @Id",
                    new { Id = monedaId },
                    transaction);

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}
