using Dapper;
using Microsoft.Data.Sqlite;
using ArtesaniasPOS.Core.Interfaces;

namespace ArtesaniasPOS.Data.Services
{
    public class ReporteService : IReporteService
    {
        private readonly string _connectionString;

        public ReporteService(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Obtiene los 4 KPIs principales del dashboard.
        /// Un solo query con subqueries para minimizar viajes a la BD.
        /// </summary>
        public async Task<ReporteResumenDto> ObtenerResumenAsync(DateTime desde, DateTime hasta)
        {
            using var connection = new SqliteConnection(_connectionString);

            var desdeStr = desde.ToString("yyyy-MM-dd 00:00:00");
            var hastaStr = hasta.ToString("yyyy-MM-dd 23:59:59");

            var resultado = await connection.QueryFirstOrDefaultAsync<ReporteResumenDto>(@"
                SELECT 
                    COUNT(v.Id) AS TotalVentas,
                    COALESCE(SUM(v.Total), 0) AS IngresoTotal,
                    COALESCE(SUM(sub.TotalItems), 0) AS ProductosVendidos,
                    CASE WHEN COUNT(v.Id) > 0 
                         THEN COALESCE(SUM(v.Total), 0) / COUNT(v.Id) 
                         ELSE 0 END AS TicketPromedio
                FROM Venta v
                LEFT JOIN (
                    SELECT VentaId, SUM(Cantidad) AS TotalItems
                    FROM DetalleVenta
                    GROUP BY VentaId
                ) sub ON sub.VentaId = v.Id
                WHERE v.Estado = 'Completada'
                  AND v.FechaVenta BETWEEN @Desde AND @Hasta",
                new { Desde = desdeStr, Hasta = hastaStr });

            return resultado ?? new ReporteResumenDto();
        }

        public async Task<IEnumerable<DetalleVentaHistorialDto>> ObtenerDetalleVentaAsync(int ventaId)
        {
            using var connection = new SqliteConnection(_connectionString);
            return await connection.QueryAsync<DetalleVentaHistorialDto>(@"
            SELECT 
                p.Nombre AS Producto,
                p.CodigoBarras,
                dv.Cantidad,
                dv.PrecioUnitario,
                dv.Subtotal
            FROM DetalleVenta dv
            INNER JOIN Producto p ON p.Id = dv.ProductoId
            WHERE dv.VentaId = @VentaId
            ORDER BY p.Nombre",
                new { VentaId = ventaId });
        }



        public async Task<IEnumerable<ProductoVendidoDto>> ObtenerProductosMasVendidosAsync(
            DateTime desde, DateTime hasta, int top = 10)
        {
            using var connection = new SqliteConnection(_connectionString);

            var desdeStr = desde.ToString("yyyy-MM-dd 00:00:00");
            var hastaStr = hasta.ToString("yyyy-MM-dd 23:59:59");

            return await connection.QueryAsync<ProductoVendidoDto>(@"
                SELECT 
                    p.Nombre,
                    p.CodigoBarras,
                    SUM(dv.Cantidad) AS CantidadVendida,
                    SUM(dv.Subtotal) AS TotalGenerado
                FROM DetalleVenta dv
                INNER JOIN Producto p ON p.Id = dv.ProductoId
                INNER JOIN Venta v ON v.Id = dv.VentaId
                WHERE v.Estado = 'Completada'
                  AND v.FechaVenta BETWEEN @Desde AND @Hasta
                GROUP BY dv.ProductoId, p.Nombre, p.CodigoBarras
                ORDER BY CantidadVendida DESC
                LIMIT @Top",
                new { Desde = desdeStr, Hasta = hastaStr, Top = top });
        }

        /// <summary>
        /// Ventas agrupadas por vendedor.
        /// </summary>
        public async Task<IEnumerable<VentaPorVendedorDto>> ObtenerVentasPorVendedorAsync(
            DateTime desde, DateTime hasta)
        {
            using var connection = new SqliteConnection(_connectionString);

            var desdeStr = desde.ToString("yyyy-MM-dd 00:00:00");
            var hastaStr = hasta.ToString("yyyy-MM-dd 23:59:59");

            return await connection.QueryAsync<VentaPorVendedorDto>(@"
                SELECT 
                    u.Nombre AS NombreVendedor,
                    COUNT(v.Id) AS CantidadVentas,
                    SUM(v.Total) AS TotalVendido
                FROM Venta v
                INNER JOIN Usuario u ON u.Id = v.UsuarioId
                WHERE v.Estado = 'Completada'
                  AND v.FechaVenta BETWEEN @Desde AND @Hasta
                GROUP BY v.UsuarioId, u.Nombre
                ORDER BY TotalVendido DESC",
                new { Desde = desdeStr, Hasta = hastaStr });
        }

        /// <summary>
        /// Historial detallado de ventas con toda la info relevante.
        /// </summary>
        public async Task<IEnumerable<VentaHistorialDto>> ObtenerHistorialVentasAsync(
            DateTime desde, DateTime hasta)
        {
            using var connection = new SqliteConnection(_connectionString);

            var desdeStr = desde.ToString("yyyy-MM-dd 00:00:00");
            var hastaStr = hasta.ToString("yyyy-MM-dd 23:59:59");

            return await connection.QueryAsync<VentaHistorialDto>(@"
                SELECT 
                    v.Id AS VentaId,
                    u.Nombre AS Vendedor,
                    mp.Nombre AS MedioPago,
                    m.Codigo AS Moneda,
                    v.Total,
                    v.Descuento,
                    v.Estado,
                    v.FechaVenta,
                    COALESCE(sub.TotalItems, 0) AS CantidadItems
                FROM Venta v
                INNER JOIN Usuario u ON u.Id = v.UsuarioId
                INNER JOIN MedioPago mp ON mp.Id = v.MedioPagoId
                INNER JOIN Moneda m ON m.Id = v.MonedaId
                LEFT JOIN (
                    SELECT VentaId, SUM(Cantidad) AS TotalItems
                    FROM DetalleVenta
                    GROUP BY VentaId
                ) sub ON sub.VentaId = v.Id
                WHERE v.FechaVenta BETWEEN @Desde AND @Hasta
                ORDER BY v.FechaVenta DESC",
                new { Desde = desdeStr, Hasta = hastaStr });
        }
    }
}
