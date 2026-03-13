namespace ArtesaniasPOS.Core.Interfaces
{
    /// <summary>
    /// Contrato para el servicio de ventas.
    /// Cubre: búsqueda de productos para el POS, registro de ventas,
    /// y consulta de medios de pago.
    /// </summary>
    public interface IVentaService
    {
        /// <summary>
        /// Busca productos activos por código de barras o nombre.
        /// Retorna solo los que tienen stock > 0.
        /// </summary>
        Task<IEnumerable<ProductoBusquedaDto>> BuscarProductosAsync(string termino);

        /// <summary>
        /// Obtiene medios de pago activos.
        /// </summary>
        Task<IEnumerable<MedioPagoDto>> ObtenerMediosPagoAsync();

        /// <summary>
        /// Registra una venta completa en una transacción:
        /// 1. Inserta Venta
        /// 2. Inserta DetalleVenta por cada item del carrito
        /// 3. Descuenta stock de cada producto
        /// 4. Registra movimientos de inventario
        /// Retorna el Id de la venta creada.
        /// </summary>
        Task<int> RegistrarVentaAsync(VentaRegistroDto venta);
    }

    /// <summary>
    /// DTO para resultados de búsqueda en el POS.
    /// Incluye solo lo necesario para agregar al carrito.
    /// </summary>
    public class ProductoBusquedaDto
    {
        public int Id { get; set; }
        public string CodigoBarras { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public double PrecioBase { get; set; }
        public int StockActual { get; set; }
    }

    public class MedioPagoDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO para registrar una venta completa.
    /// El ViewModel arma este objeto con toda la info del carrito
    /// y lo pasa al servicio para persistir en BD.
    /// </summary>
    public class VentaRegistroDto
    {
        public int UsuarioId { get; set; }
        public int MedioPagoId { get; set; }
        public int MonedaId { get; set; }
        public double TasaCambioUsada { get; set; }
        public double Subtotal { get; set; }
        public double Descuento { get; set; }
        public double Total { get; set; }
        public double TotalMonedaVenta { get; set; }
        public double? MontoPagado { get; set; }
        public double? Cambio { get; set; }
        public string? Observaciones { get; set; }
        public List<DetalleVentaRegistroDto> Detalles { get; set; } = new();
    }

    public class DetalleVentaRegistroDto
    {
        public int ProductoId { get; set; }
        public int Cantidad { get; set; }
        public double PrecioUnitario { get; set; }
        public double Subtotal { get; set; }
    }
}
