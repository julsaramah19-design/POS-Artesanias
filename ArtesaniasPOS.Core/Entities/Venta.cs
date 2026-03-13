namespace ArtesaniasPOS.Core.Entities
{
    public class Venta
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public int MedioPagoId { get; set; }
        public int MonedaId { get; set; }
        public decimal TasaCambioUsada { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Descuento { get; set; }
        public decimal Total { get; set; }
        public decimal TotalMonedaVenta { get; set; }
        public decimal? MontoPagado { get; set; }
        public decimal? Cambio { get; set; }
        public string Estado { get; set; } = "Completada";
        public string? Observaciones { get; set; }
        public DateTime FechaVenta { get; set; }


        public string NombreUsuario { get; set; } = string.Empty;
        public string NombreMedioPago { get; set; } = string.Empty;
        public string SimboloMoneda { get; set; } = string.Empty;
        public List<DetalleVenta> Detalles { get; set; } = new();
    }
}