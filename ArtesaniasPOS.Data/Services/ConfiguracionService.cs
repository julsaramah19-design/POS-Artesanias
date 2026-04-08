using Dapper;
using Microsoft.Data.Sqlite;
using ArtesaniasPOS.Core.Interfaces;

namespace ArtesaniasPOS.Data.Services
{
    public class ConfiguracionService : IConfiguracionService
    {
        private readonly string _connectionString;

        public ConfiguracionService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<string> ObtenerValorAsync(string clave)
        {
            using var connection = new SqliteConnection(_connectionString);
            var valor = await connection.QueryFirstOrDefaultAsync<string>(
                "SELECT Valor FROM ConfiguracionNegocio WHERE Clave = @Clave",
                new { Clave = clave });

            return valor ?? string.Empty;
        }

        public async Task<Dictionary<string, string>> ObtenerTodasAsync()
        {
            using var connection = new SqliteConnection(_connectionString);
            var registros = await connection.QueryAsync<(string Clave, string Valor)>(
                "SELECT Clave, Valor FROM ConfiguracionNegocio");

            return registros.ToDictionary(r => r.Clave, r => r.Valor);
        }

        public async Task GuardarValorAsync(string clave, string valor)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.ExecuteAsync(
                "UPDATE ConfiguracionNegocio SET Valor = @Valor WHERE Clave = @Clave",
                new { Clave = clave, Valor = valor });
        }

        public async Task GuardarVariosAsync(Dictionary<string, string> configuraciones)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            using var transaction = connection.BeginTransaction();
            try
            {
                foreach (var config in configuraciones)
                {
                    await connection.ExecuteAsync(
                        "UPDATE ConfiguracionNegocio SET Valor = @Valor WHERE Clave = @Clave",
                        new { Clave = config.Key, Valor = config.Value },
                        transaction);
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<bool> WizardCompletadoAsync()
        {
            var valor = await ObtenerValorAsync("WizardCompletado");
            return valor == "1";
        }

        public async Task MarcarWizardCompletadoAsync()
        {
            await GuardarValorAsync("WizardCompletado", "1");
        }
    }
}
