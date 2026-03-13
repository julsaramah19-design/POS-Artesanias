using Dapper;
using Microsoft.Data.Sqlite;
using ArtesaniasPOS.Core.Interfaces;

namespace ArtesaniasPOS.Data.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly string _connectionString;

        public UsuarioService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task ActualizarAdminAsync(string nombre, string nombreUsuario, string password)
        {
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

            using var connection = new SqliteConnection(_connectionString);
            await connection.ExecuteAsync(@"
                UPDATE Usuario 
                SET Nombre          = @Nombre,
                    NombreUsuario   = @NombreUsuario,
                    PasswordHash    = @PasswordHash
                WHERE PerfilId = 1 
                  AND Id = (SELECT MIN(Id) FROM Usuario WHERE PerfilId = 1)",
                new
                {
                    Nombre = nombre,
                    NombreUsuario = nombreUsuario,
                    PasswordHash = passwordHash
                });
        }

        public async Task<AdminInfoDto?> ObtenerAdminActualAsync()
        {
            using var connection = new SqliteConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<AdminInfoDto>(@"
                SELECT Nombre, NombreUsuario
                FROM Usuario
                WHERE PerfilId = 1
                ORDER BY Id
                LIMIT 1");
        }

        public async Task<bool> ExisteNombreUsuarioAsync(string nombreUsuario, int excluirId = 1)
        {
            using var connection = new SqliteConnection(_connectionString);
            var count = await connection.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM Usuario WHERE NombreUsuario = @NombreUsuario AND Id != @ExcluirId",
                new { NombreUsuario = nombreUsuario, ExcluirId = excluirId });

            return count > 0;
        }
    }
}
