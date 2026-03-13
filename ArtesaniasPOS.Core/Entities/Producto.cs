namespace ArtesaniasPOS.Core.Entities
{
    public class Producto
    {
        public int Id { get; set; }
        public int CategoriaId { get; set; }
        public string CodigoBarras { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public decimal PrecioBase { get; set; }
        public int StockActual { get; set; }
        public int StockMinimo { get; set; } = 5;
        public bool Activo { get; set; } = true;
        public DateTime FechaCreacion { get; set; }
        public DateTime FechaActualizacion { get; set; }

        // Navegación
        public string NombreCategoria { get; set; } = string.Empty;

        // Propiedad calculada — no va a BD
        public bool StockBajo => StockActual <= StockMinimo;
    }
}