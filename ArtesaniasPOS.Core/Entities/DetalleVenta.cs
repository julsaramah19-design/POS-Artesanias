namespace ArtesaniasPOS.Core.Entities
{
    public class DetalleVenta
    {
        public int Id { get; set; }
        public int VentaId { get; set; }
        public int ProductoId { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }

        // Navegación
        public string NombreProducto { get; set; } = string.Empty;
        public string CodigoBarras { get; set; } = string.Empty;
    }
}