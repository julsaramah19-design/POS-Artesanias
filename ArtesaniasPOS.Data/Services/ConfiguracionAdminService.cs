using Dapper;
using Microsoft.Data.Sqlite;
using ArtesaniasPOS.Core.Interfaces;

namespace ArtesaniasPOS.Data.Services
{
    public class ConfiguracionAdminService : IConfiguracionAdminService
    {
        private readonly string _connectionString;

        public ConfiguracionAdminService(string connectionString)
        {
            _connectionString = connectionString;
        }

        // ===================== USUARIOS =====================

        public async Task<IEnumerable<UsuarioListaDto>> ObtenerUsuariosAsync()
        {
            using var connection = new SqliteConnection(_connectionString);
            return await connection.QueryAsync<UsuarioListaDto>(@"
                SELECT u.Id, u.Nombre, u.NombreUsuario, p.Nombre AS PerfilNombre,
                       u.PerfilId, u.Activo, u.FechaCreacion
                FROM Usuario u
                INNER JOIN Perfil p ON p.Id = u.PerfilId
                ORDER BY u.Activo DESC, u.Nombre");
        }

        public async Task CrearUsuarioAsync(UsuarioCrearDto usuario)
        {
            var hash = BCrypt.Net.BCrypt.HashPassword(usuario.Password);
            var ahora = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            using var connection = new SqliteConnection(_connectionString);

            // ¿Ya existe ese nombre de usuario? (la columna es UNIQUE)
            var existente = await connection.QueryFirstOrDefaultAsync<(int Id, long Activo)?>(
                "SELECT Id, Activo FROM Usuario WHERE NombreUsuario = @u COLLATE NOCASE",
                new { u = usuario.NombreUsuario });

            if (existente is { } e)
            {
                if (e.Activo == 1)
                    throw new InvalidOperationException(
                        $"Ya existe un usuario con el nombre de usuario \"{usuario.NombreUsuario}\".");

                // Estaba inactivo: lo reactivamos con los nuevos datos.
                await connection.ExecuteAsync(@"
                    UPDATE Usuario SET Nombre = @Nombre, PerfilId = @PerfilId,
                           PasswordHash = @Hash, Activo = 1 WHERE Id = @Id",
                    new { e.Id, usuario.Nombre, usuario.PerfilId, Hash = hash });
                return;
            }

            await connection.ExecuteAsync(@"
                INSERT INTO Usuario (PerfilId, Nombre, NombreUsuario, PasswordHash, Activo, FechaCreacion)
                VALUES (@PerfilId, @Nombre, @NombreUsuario, @Hash, 1, @Fecha)",
                new { usuario.PerfilId, usuario.Nombre, usuario.NombreUsuario, Hash = hash, Fecha = ahora });
        }

        public async Task ActualizarUsuarioAsync(UsuarioEditarDto usuario)
        {
            using var connection = new SqliteConnection(_connectionString);

            if (!string.IsNullOrWhiteSpace(usuario.NuevoPassword))
            {
                var hash = BCrypt.Net.BCrypt.HashPassword(usuario.NuevoPassword);
                await connection.ExecuteAsync(@"
                    UPDATE Usuario SET Nombre = @Nombre, NombreUsuario = @NombreUsuario,
                           PasswordHash = @Hash, PerfilId = @PerfilId WHERE Id = @Id",
                    new { usuario.Id, usuario.Nombre, usuario.NombreUsuario, Hash = hash, usuario.PerfilId });
            }
            else
            {
                await connection.ExecuteAsync(@"
                    UPDATE Usuario SET Nombre = @Nombre, NombreUsuario = @NombreUsuario,
                           PerfilId = @PerfilId WHERE Id = @Id",
                    new { usuario.Id, usuario.Nombre, usuario.NombreUsuario, usuario.PerfilId });
            }
        }

        public async Task DesactivarUsuarioAsync(int id)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.ExecuteAsync("UPDATE Usuario SET Activo = 0 WHERE Id = @Id", new { Id = id });
        }

        public async Task ActivarUsuarioAsync(int id)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.ExecuteAsync("UPDATE Usuario SET Activo = 1 WHERE Id = @Id", new { Id = id });
        }

        public async Task<IEnumerable<PerfilDto>> ObtenerPerfilesAsync()
        {
            using var connection = new SqliteConnection(_connectionString);
            return await connection.QueryAsync<PerfilDto>(
                "SELECT Id, Nombre FROM Perfil WHERE Activo = 1 ORDER BY Nombre");
        }

        // ===================== CATEGORÍAS =====================

        public async Task<IEnumerable<CategoriaAdminDto>> ObtenerCategoriasAsync()
        {
            using var connection = new SqliteConnection(_connectionString);
            return await connection.QueryAsync<CategoriaAdminDto>(
                "SELECT Id, Nombre, Activo FROM Categoria ORDER BY Activo DESC, Nombre");
        }

        public async Task CrearCategoriaAsync(string nombre)
        {
            var nombreT = nombre.Trim();
            using var connection = new SqliteConnection(_connectionString);

            var existente = await connection.QueryFirstOrDefaultAsync<(int Id, long Activo)?>(
                "SELECT Id, Activo FROM Categoria WHERE Nombre = @n COLLATE NOCASE", new { n = nombreT });

            if (existente is { } e)
            {
                if (e.Activo == 1)
                    throw new InvalidOperationException($"Ya existe una categoría llamada \"{nombreT}\".");

                await connection.ExecuteAsync(
                    "UPDATE Categoria SET Activo = 1, Nombre = @n WHERE Id = @Id", new { n = nombreT, e.Id });
                return;
            }

            await connection.ExecuteAsync(
                "INSERT INTO Categoria (Nombre, Activo) VALUES (@Nombre, 1)", new { Nombre = nombreT });
        }

        public async Task ActualizarCategoriaAsync(int id, string nombre)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.ExecuteAsync(
                "UPDATE Categoria SET Nombre = @Nombre WHERE Id = @Id", new { Id = id, Nombre = nombre.Trim() });
        }

        public async Task DesactivarCategoriaAsync(int id)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.ExecuteAsync("UPDATE Categoria SET Activo = 0 WHERE Id = @Id", new { Id = id });
        }

        public async Task ActivarCategoriaAsync(int id)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.ExecuteAsync("UPDATE Categoria SET Activo = 1 WHERE Id = @Id", new { Id = id });
        }

        // ===================== MEDIOS DE PAGO =====================

        public async Task<IEnumerable<MedioPagoAdminDto>> ObtenerMediosPagoAsync()
        {
            using var connection = new SqliteConnection(_connectionString);
            return await connection.QueryAsync<MedioPagoAdminDto>(
                "SELECT Id, Nombre, Activo FROM MedioPago ORDER BY Activo DESC, Nombre");
        }

        public async Task CrearMedioPagoAsync(string nombre)
        {
            var nombreT = nombre.Trim();
            using var connection = new SqliteConnection(_connectionString);

            var existente = await connection.QueryFirstOrDefaultAsync<(int Id, long Activo)?>(
                "SELECT Id, Activo FROM MedioPago WHERE Nombre = @n COLLATE NOCASE", new { n = nombreT });

            if (existente is { } e)
            {
                if (e.Activo == 1)
                    throw new InvalidOperationException($"Ya existe un medio de pago llamado \"{nombreT}\".");

                await connection.ExecuteAsync(
                    "UPDATE MedioPago SET Activo = 1, Nombre = @n WHERE Id = @Id", new { n = nombreT, e.Id });
                return;
            }

            await connection.ExecuteAsync(
                "INSERT INTO MedioPago (Nombre, Activo) VALUES (@Nombre, 1)", new { Nombre = nombreT });
        }

        public async Task ActualizarMedioPagoAsync(int id, string nombre)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.ExecuteAsync(
                "UPDATE MedioPago SET Nombre = @Nombre WHERE Id = @Id", new { Id = id, Nombre = nombre.Trim() });
        }

        public async Task DesactivarMedioPagoAsync(int id)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.ExecuteAsync("UPDATE MedioPago SET Activo = 0 WHERE Id = @Id", new { Id = id });
        }

        public async Task ActivarMedioPagoAsync(int id)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.ExecuteAsync("UPDATE MedioPago SET Activo = 1 WHERE Id = @Id", new { Id = id });
        }
    }
}
