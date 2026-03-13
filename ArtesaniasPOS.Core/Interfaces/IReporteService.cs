namespace ArtesaniasPOS.Core.Interfaces
{
    public interface IReporteService
    {
        Task<ReporteResumenDto> ObtenerResumenAsync(DateTime desde, DateTime hasta);
        Task<IEnumerable<ProductoVendidoDto>> ObtenerProductosMasVendidosAsync(DateTime desde, DateTime hasta, int top = 10);
        Task<IEnumerable<VentaPorVendedorDto>> ObtenerVentasPorVendedorAsync(DateTime desde, DateTime hasta);
        Task<IEnumerable<VentaHistorialDto>> ObtenerHistorialVentasAsync(DateTime desde, DateTime hasta);
        Task<IEnumerable<DetalleVentaHistorialDto>> ObtenerDetalleVentaAsync(int ventaId);

    }

    public class DetalleVentaHistorialDto
    {
        public string Producto { get; set; } = string.Empty;
        public string CodigoBarras { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public double PrecioUnitario { get; set; }
        public double Subtotal { get; set; }
    }


    public class ReporteResumenDto
    {
        public int TotalVentas { get; set; }
        public double IngresoTotal { get; set; }
        public int ProductosVendidos { get; set; }
        public double TicketPromedio { get; set; }
    }

    public class ProductoVendidoDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string CodigoBarras { get; set; } = string.Empty;
        public int CantidadVendida { get; set; }
        public double TotalGenerado { get; set; }
    }

    public class VentaPorVendedorDto
    {
        public string NombreVendedor { get; set; } = string.Empty;
        public int CantidadVentas { get; set; }
        public double TotalVendido { get; set; }
    }

    public class VentaHistorialDto
    {
        public int VentaId { get; set; }
        public string Vendedor { get; set; } = string.Empty;
        public string MedioPago { get; set; } = string.Empty;
        public string Moneda { get; set; } = string.Empty;
        public double Total { get; set; }
        public double Descuento { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string FechaVenta { get; set; } = string.Empty;
        public int CantidadItems { get; set; }
    }
}
