using ArtesaniasPOS.Core.Entities;
using ArtesaniasPOS.Data.Database;
using ArtesaniasPOS.Core.Interfaces;
using Dapper;
using Microsoft.Data.Sqlite;

namespace ArtesaniasPOS.Data.Repositories
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly string _connectionString;

        public UsuarioRepository()
        {
            _connectionString = DatabaseConfig.ConnectionString;
        }

        public async Task<Usuario?> ObtenerPorNombreUsuarioAsync(string nombreUsuario)
        {
            const string sql = @"
                SELECT 
                    u.Id, u.PerfilId, u.Nombre, u.NombreUsuario,
                    u.PasswordHash, u.Activo, u.FechaCreacion,
                    p.Nombre AS NombrePerfil
                FROM Usuario u
                INNER JOIN Perfil p ON p.Id = u.PerfilId
                WHERE u.NombreUsuario = @NombreUsuario
                  AND u.Activo = 1";

            using var connection = new SqliteConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<Usuario>(
                sql, new { NombreUsuario = nombreUsuario });
        }

        public async Task<IEnumerable<Usuario>> ObtenerTodosAsync()
        {
            const string sql = @"
                SELECT 
                    u.Id, u.PerfilId, u.Nombre, u.NombreUsuario,
                    u.Activo, u.FechaCreacion,
                    p.Nombre AS NombrePerfil
                FROM Usuario u
                INNER JOIN Perfil p ON p.Id = u.PerfilId
                ORDER BY u.Nombre";

            using var connection = new SqliteConnection(_connectionString);
            return await connection.QueryAsync<Usuario>(sql);
        }

        public async Task<int> CrearAsync(Usuario usuario)
        {
            const string sql = @"
                INSERT INTO Usuario 
                    (PerfilId, Nombre, NombreUsuario, PasswordHash, Activo, FechaCreacion)
                VALUES 
                    (@PerfilId, @Nombre, @NombreUsuario, @PasswordHash, 1, @FechaCreacion);
                SELECT last_insert_rowid();";

            using var connection = new SqliteConnection(_connectionString);
            return await connection.ExecuteScalarAsync<int>(sql, new
            {
                usuario.PerfilId,
                usuario.Nombre,
                usuario.NombreUsuario,
                usuario.PasswordHash,
                FechaCreacion = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            });
        }

        public async Task ActualizarAsync(Usuario usuario)
        {
            const string sql = @"
                UPDATE Usuario SET
                    PerfilId        = @PerfilId,
                    Nombre          = @Nombre,
                    NombreUsuario   = @NombreUsuario,
                    PasswordHash    = @PasswordHash
                WHERE Id = @Id";

            using var connection = new SqliteConnection(_connectionString);
            await connection.ExecuteAsync(sql, new
            {
                usuario.PerfilId,
                usuario.Nombre,
                usuario.NombreUsuario,
                usuario.PasswordHash,
                usuario.Id
            });
        }

        public async Task DesactivarAsync(int id)
        {
            const string sql = "UPDATE Usuario SET Activo = 0 WHERE Id = @Id";

            using var connection = new SqliteConnection(_connectionString);
            await connection.ExecuteAsync(sql, new { Id = id });
        }
    }
}