using Dapper;
using Microsoft.Data.Sqlite;
using ArtesaniasPOS.Core.Interfaces;

namespace ArtesaniasPOS.Data.Services
{
    public class ProductoService : IProductoService
    {
        private readonly string _connectionString;

        public ProductoService(string connectionString)
        {
            _connectionString = connectionString;
        }
        public async Task<IEnumerable<ProductoListaDto>> ObtenerTodosAsync(
            string? busqueda = null, int? categoriaId = null)
        {
            using var connection = new SqliteConnection(_connectionString);

            var sql = @"
                SELECT 
                    p.Id, p.CodigoBarras, p.Nombre, 
                    c.Nombre AS CategoriaNombre,
                    p.PrecioBase, p.StockActual, p.StockMinimo, p.Activo
                FROM Producto p
                INNER JOIN Categoria c ON c.Id = p.CategoriaId
                WHERE p.Activo = 1";

            var parametros = new DynamicParameters();

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                sql += " AND (p.Nombre LIKE @Busqueda OR p.CodigoBarras LIKE @Busqueda)";
                parametros.Add("Busqueda", $"%{busqueda.Trim()}%");
            }

            if (categoriaId.HasValue && categoriaId.Value > 0)
            {
                sql += " AND p.CategoriaId = @CategoriaId";
                parametros.Add("CategoriaId", categoriaId.Value);
            }

            sql += " ORDER BY p.Nombre";

            return await connection.QueryAsync<ProductoListaDto>(sql, parametros);
        }

        public async Task<ProductoBusquedaDto?> ObtenerPorCodigoBarrasAsync(string codigo)
        {
            using var connection = new SqliteConnection(_connectionString);

            // Consulta SQL para buscar la coincidencia exacta del código de barras
            var sql = @"
                SELECT 
                    p.Id, 
                    p.CodigoBarras, 
                    p.Nombre, 
                    p.PrecioBase, 
                    p.StockActual
                FROM Producto p
                WHERE p.CodigoBarras = @codigo AND p.Activo = 1 
                LIMIT 1";

            // Ejecutamos la consulta usando Dapper
            return await connection.QueryFirstOrDefaultAsync<ProductoBusquedaDto>(sql, new { codigo });
        }

        public async Task<ProductoDetalleDto?> ObtenerPorIdAsync(int id)
        {
            using var connection = new SqliteConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<ProductoDetalleDto>(
                @"SELECT Id, CategoriaId, CodigoBarras, Nombre, Descripcion, 
                         PrecioBase, StockActual, StockMinimo
                  FROM Producto WHERE Id = @Id", new { Id = id });
        }

        public async Task<int> CrearAsync(ProductoGuardarDto producto)
        {
            using var connection = new SqliteConnection(_connectionString);
            var ahora = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            var id = await connection.ExecuteScalarAsync<int>(@"
                INSERT INTO Producto 
                    (CategoriaId, CodigoBarras, Nombre, Descripcion, 
                     PrecioBase, StockActual, StockMinimo, Activo, 
                     FechaCreacion, FechaActualizacion)
                VALUES 
                    (@CategoriaId, @CodigoBarras, @Nombre, @Descripcion,
                     @PrecioBase, @StockActual, @StockMinimo, 1,
                     @Fecha, @Fecha);
                SELECT last_insert_rowid();",
                new
                {
                    producto.CategoriaId,
                    producto.CodigoBarras,
                    producto.Nombre,
                    producto.Descripcion,
                    producto.PrecioBase,
                    producto.StockActual,
                    producto.StockMinimo,
                    Fecha = ahora
                });

            return id;
        }

        public async Task ActualizarAsync(ProductoGuardarDto producto)
        {
            using var connection = new SqliteConnection(_connectionString);
            var ahora = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            await connection.ExecuteAsync(@"
                UPDATE Producto SET
                    CategoriaId         = @CategoriaId,
                    CodigoBarras        = @CodigoBarras,
                    Nombre              = @Nombre,
                    Descripcion         = @Descripcion,
                    PrecioBase          = @PrecioBase,
                    StockActual         = @StockActual,
                    StockMinimo         = @StockMinimo,
                    FechaActualizacion  = @Fecha
                WHERE Id = @Id",
                new
                {
                    producto.Id,
                    producto.CategoriaId,
                    producto.CodigoBarras,
                    producto.Nombre,
                    producto.Descripcion,
                    producto.PrecioBase,
                    producto.StockActual,
                    producto.StockMinimo,
                    Fecha = ahora
                });
        }

        public async Task DesactivarAsync(int id)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.ExecuteAsync(
                "UPDATE Producto SET Activo = 0 WHERE Id = @Id",
                new { Id = id });
        }
        public async Task<IEnumerable<CategoriaDto>> ObtenerCategoriasAsync()
        {
            using var connection = new SqliteConnection(_connectionString);
            return await connection.QueryAsync<CategoriaDto>(
                "SELECT Id, Nombre FROM Categoria WHERE Activo = 1 ORDER BY Nombre");
        }

        public async Task<string> GenerarCodigoBarrasAsync()
        {
            using var connection = new SqliteConnection(_connectionString);
            var random = new Random();
            string codigo;

            do
            {
                codigo = $"ART-{random.Next(10000000, 99999999)}";
                var existe = await connection.ExecuteScalarAsync<int>(
                    "SELECT COUNT(*) FROM Producto WHERE CodigoBarras = @Codigo",
                    new { Codigo = codigo });

                if (existe == 0) break;
            } while (true);

            return codigo;
        }
    }
}
