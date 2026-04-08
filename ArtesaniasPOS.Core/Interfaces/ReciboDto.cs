namespace ArtesaniasPOS.Core.Interfaces
{
    public class ReciboDto
    {
        public int VentaId { get; set; }
        public string NombreNegocio { get; set; } = string.Empty;
        public string Vendedor { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public string MedioPago { get; set; } = string.Empty;
        public string Moneda { get; set; } = string.Empty;
        public List<ReciboItemDto> Items { get; set; } = new();
        public double Subtotal { get; set; }
        public double Total { get; set; }
        public double MontoPagado { get; set; }
        public double Cambio { get; set; }
        public string FechaFormateada => Fecha.ToString("dd/MM/yyyy hh:mm tt");
    }

    public class ReciboItemDto
    {
        public string Nombre { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public double PrecioUnitario { get; set; }
        public double Descuento { get; set; }
        public double Subtotal { get; set; }
        public bool TieneDescuento => Descuento > 0;
    }
}