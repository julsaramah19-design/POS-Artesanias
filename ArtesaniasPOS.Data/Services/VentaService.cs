using Dapper;
using Microsoft.Data.Sqlite;
using ArtesaniasPOS.Core.Interfaces;

namespace ArtesaniasPOS.Data.Services
{
    /// <summary>
    /// Implementación de IVentaService con Dapper + SQLite.
    /// 
    /// El método más importante es RegistrarVentaAsync que ejecuta
    /// todo dentro de una transacción: si falla cualquier paso
    /// (insertar venta, detalle, descontar stock), se revierte TODO.
    /// Esto garantiza que nunca quede una venta sin descontar stock
    /// ni stock descontado sin venta.
    /// </summary>
    public class VentaService : IVentaService
    {
        private readonly string _connectionString;

        public VentaService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IEnumerable<ProductoBusquedaDto>> BuscarProductosAsync(string termino)
        {
            using var connection = new SqliteConnection(_connectionString);
            return await connection.QueryAsync<ProductoBusquedaDto>(@"
                SELECT Id, CodigoBarras, Nombre, PrecioBase, StockActual
                FROM Producto
                WHERE Activo = 1 
                  AND StockActual > 0
                  AND (Nombre LIKE @Termino OR CodigoBarras LIKE @Termino)
                ORDER BY Nombre
                LIMIT 10",
                new { Termino = $"%{termino.Trim()}%" });
        }

        public async Task<IEnumerable<MedioPagoDto>> ObtenerMediosPagoAsync()
        {
            using var connection = new SqliteConnection(_connectionString);
            return await connection.QueryAsync<MedioPagoDto>(
                "SELECT Id, Nombre FROM MedioPago WHERE Activo = 1 ORDER BY Nombre");
        }

        /// <summary>
        /// Registra la venta completa en una transacción atómica.
        /// 
        /// Pasos:
        /// 1. INSERT Venta → obtener ventaId
        /// 2. INSERT DetalleVenta por cada item
        /// 3. UPDATE Producto.StockActual (descontar)
        /// 4. INSERT MovimientoInventario (trazabilidad)
        /// 
        /// Si cualquier paso falla → ROLLBACK de todo.
        /// </summary>
        public async Task<int> RegistrarVentaAsync(VentaRegistroDto venta)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            using var transaction = connection.BeginTransaction();
            try
            {
                var ahora = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                // 1. Insertar la venta
                var ventaId = await connection.ExecuteScalarAsync<int>(@"
                    INSERT INTO Venta 
                        (UsuarioId, MedioPagoId, MonedaId, TasaCambioUsada,
                         Subtotal, Descuento, Total, TotalMonedaVenta,
                         MontoPagado, Cambio, Estado, Observaciones, FechaVenta)
                    VALUES 
                        (@UsuarioId, @MedioPagoId, @MonedaId, @TasaCambioUsada,
                         @Subtotal, @Descuento, @Total, @TotalMonedaVenta,
                         @MontoPagado, @Cambio, 'Completada', @Observaciones, @Fecha);
                    SELECT last_insert_rowid();",
                    new
                    {
                        venta.UsuarioId,
                        venta.MedioPagoId,
                        venta.MonedaId,
                        venta.TasaCambioUsada,
                        venta.Subtotal,
                        venta.Descuento,
                        venta.Total,
                        venta.TotalMonedaVenta,
                        venta.MontoPagado,
                        venta.Cambio,
                        venta.Observaciones,
                        Fecha = ahora
                    },
                    transaction);

                // 2. Insertar detalles y descontar stock
                foreach (var detalle in venta.Detalles)
                {
                    // Insertar detalle
                    await connection.ExecuteAsync(@"
                        INSERT INTO DetalleVenta 
                            (VentaId, ProductoId, Cantidad, PrecioUnitario, Subtotal)
                        VALUES 
                            (@VentaId, @ProductoId, @Cantidad, @PrecioUnitario, @Subtotal)",
                        new
                        {
                            VentaId = ventaId,
                            detalle.ProductoId,
                            detalle.Cantidad,
                            detalle.PrecioUnitario,
                            detalle.Subtotal
                        },
                        transaction);

                    // 3. Obtener stock actual antes de descontar
                    var stockAnterior = await connection.ExecuteScalarAsync<int>(
                        "SELECT StockActual FROM Producto WHERE Id = @Id",
                        new { Id = detalle.ProductoId },
                        transaction);

                    var stockResultante = stockAnterior - detalle.Cantidad;

                    // Descontar stock
                    await connection.ExecuteAsync(
                        "UPDATE Producto SET StockActual = @Stock WHERE Id = @Id",
                        new { Stock = stockResultante, Id = detalle.ProductoId },
                        transaction);

                    // 4. Registrar movimiento de inventario
                    await connection.ExecuteAsync(@"
                        INSERT INTO MovimientoInventario 
                            (ProductoId, UsuarioId, TipoMovimiento, Cantidad,
                             StockAnterior, StockResultante, Referencia, Motivo, Fecha)
                        VALUES 
                            (@ProductoId, @UsuarioId, 'Salida', @Cantidad,
                             @StockAnterior, @StockResultante, @Referencia, 'Venta', @Fecha)",
                        new
                        {
                            detalle.ProductoId,
                            venta.UsuarioId,
                            detalle.Cantidad,
                            StockAnterior = stockAnterior,
                            StockResultante = stockResultante,
                            Referencia = $"Venta #{ventaId}",
                            Fecha = ahora
                        },
                        transaction);
                }

                transaction.Commit();
                return ventaId;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}
