using Dapper;
using Microsoft.Data.Sqlite;
using ArtesaniasPOS.Core.Interfaces;

namespace ArtesaniasPOS.Data.Services
{
    /// <summary>
    /// Implementación de IProductoService con Dapper + SQLite.
    /// </summary>
    public class ProductoService : IProductoService
    {
        private readonly string _connectionString;

        public ProductoService(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Obtiene productos con filtro opcional de búsqueda y categoría.
        /// 
        /// El query usa LIKE para buscar por nombre o código de barras.
        /// Solo trae productos activos por defecto.
        /// JOIN con Categoría para mostrar el nombre en el DataGrid.
        /// ORDER BY Nombre para que el listado sea predecible.
        /// </summary>
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

        /// <summary>
        /// Obtiene un producto por Id para el formulario de edición.
        /// </summary>
        public async Task<ProductoDetalleDto?> ObtenerPorIdAsync(int id)
        {
            using var connection = new SqliteConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<ProductoDetalleDto>(
                @"SELECT Id, CategoriaId, CodigoBarras, Nombre, Descripcion, 
                         PrecioBase, StockActual, StockMinimo
                  FROM Producto WHERE Id = @Id", new { Id = id });
        }

        /// <summary>
        /// Crea un producto nuevo. Retorna el Id generado.
        /// 
        /// last_insert_rowid() es el equivalente SQLite de SCOPE_IDENTITY() 
        /// en SQL Server. Retorna el Id del último INSERT.
        /// </summary>
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

        /// <summary>
        /// Actualiza un producto existente.
        /// No toca FechaCreacion — solo FechaActualizacion.
        /// </summary>
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

        /// <summary>
        /// Soft delete: marca el producto como inactivo.
        /// 
        /// ¿Por qué no DELETE físico?
        /// Porque el producto puede estar referenciado en ventas pasadas.
        /// Si lo borras, pierdes la integridad de los reportes.
        /// El soft delete preserva el historial.
        /// </summary>
        public async Task DesactivarAsync(int id)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.ExecuteAsync(
                "UPDATE Producto SET Activo = 0 WHERE Id = @Id",
                new { Id = id });
        }

        /// <summary>
        /// Obtiene categorías activas para el combo del formulario.
        /// </summary>
        public async Task<IEnumerable<CategoriaDto>> ObtenerCategoriasAsync()
        {
            using var connection = new SqliteConnection(_connectionString);
            return await connection.QueryAsync<CategoriaDto>(
                "SELECT Id, Nombre FROM Categoria WHERE Activo = 1 ORDER BY Nombre");
        }

        /// <summary>
        /// Genera un código de barras único automáticamente.
        /// Formato: ART-XXXXXXXX (8 dígitos aleatorios).
        /// Verifica que no exista antes de retornarlo.
        /// </summary>
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
