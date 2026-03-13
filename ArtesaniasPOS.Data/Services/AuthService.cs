using Dapper;
using Microsoft.Data.Sqlite;
using ArtesaniasPOS.Core.Interfaces;

namespace ArtesaniasPOS.Data.Services
{
    public class AuthService : IAuthService
    {
        private readonly string _connectionString;

        public AuthService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<SesionUsuario?> LoginAsync(string nombreUsuario, string password)
        {
            using var connection = new SqliteConnection(_connectionString);

            // Traer usuario con su perfil en un solo query (JOIN)
            var usuario = await connection.QueryFirstOrDefaultAsync<UsuarioLogin>(@"
                SELECT 
                    u.Id AS UsuarioId,
                    u.Nombre,
                    u.NombreUsuario,
                    u.PasswordHash,
                    u.PerfilId,
                    p.Nombre AS PerfilNombre
                FROM Usuario u
                INNER JOIN Perfil p ON p.Id = u.PerfilId
                WHERE u.NombreUsuario = @NombreUsuario
                  AND u.Activo = 1",
                new { NombreUsuario = nombreUsuario });

            if (usuario == null)
                return null;

            if (!BCrypt.Net.BCrypt.Verify(password, usuario.PasswordHash))
                return null;

            return new SesionUsuario
            {
                UsuarioId = usuario.UsuarioId,
                Nombre = usuario.Nombre,
                NombreUsuario = usuario.NombreUsuario,
                PerfilId = usuario.PerfilId,
                PerfilNombre = usuario.PerfilNombre
            };
        }
        private class UsuarioLogin
        {
            public int UsuarioId { get; set; }
            public string Nombre { get; set; } = string.Empty;
            public string NombreUsuario { get; set; } = string.Empty;
            public string PasswordHash { get; set; } = string.Empty;
            public int PerfilId { get; set; }
            public string PerfilNombre { get; set; } = string.Empty;
        }
    }
}
