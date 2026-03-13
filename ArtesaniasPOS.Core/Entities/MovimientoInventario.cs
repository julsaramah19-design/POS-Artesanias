namespace ArtesaniasPOS.Core.Entities
{
    public class MovimientoInventario
    {
        public int Id { get; set; }
        public int ProductoId { get; set; }
        public int UsuarioId { get; set; }
        public string TipoMovimiento { get; set; } = string.Empty;
        // 'Entrada', 'Salida', 'Ajuste', 'VentaAutomatica', 'Devolucion'
        public int Cantidad { get; set; }
        public int StockAnterior { get; set; }
        public int StockResultante { get; set; }
        public string? Referencia { get; set; }
        public string? Motivo { get; set; }
        public DateTime Fecha { get; set; }

        // Navegación
        public string NombreProducto { get; set; } = string.Empty;
        public string NombreUsuario { get; set; } = string.Empty;
    }
}