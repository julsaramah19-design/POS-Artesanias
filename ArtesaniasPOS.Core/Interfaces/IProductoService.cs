namespace ArtesaniasPOS.Core.Interfaces
{
    /// <summary>
    /// Contrato para el servicio de productos.
    /// Cubre: listado con filtros, CRUD, y consulta de categorías.
    /// </summary>
    public interface IProductoService
    {
        Task<IEnumerable<ProductoListaDto>> ObtenerTodosAsync(string? busqueda = null, int? categoriaId = null);
        Task<ProductoDetalleDto?> ObtenerPorIdAsync(int id);
        Task<int> CrearAsync(ProductoGuardarDto producto);
        Task ActualizarAsync(ProductoGuardarDto producto);
        Task DesactivarAsync(int id);
        Task<IEnumerable<CategoriaDto>> ObtenerCategoriasAsync();
        Task<string> GenerarCodigoBarrasAsync();
    }

    /// <summary>
    /// DTO para el listado (DataGrid). Solo los campos visibles en la tabla.
    /// Mantiene la consulta SQL liviana — no trae campos innecesarios.
    /// </summary>
    public class ProductoListaDto
    {
        public int Id { get; set; }
        public string CodigoBarras { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string CategoriaNombre { get; set; } = string.Empty;
        public double PrecioBase { get; set; }
        public int StockActual { get; set; }
        public int StockMinimo { get; set; }
        public bool Activo { get; set; }

        /// <summary>
        /// Indica visualmente si el stock está bajo.
        /// La vista puede usar esto para pintar la fila de rojo.
        /// </summary>
        public bool StockBajo => StockActual <= StockMinimo;
    }

    /// <summary>
    /// DTO completo para el formulario de edición.
    /// </summary>
    public class ProductoDetalleDto
    {
        public int Id { get; set; }
        public int CategoriaId { get; set; }
        public string CodigoBarras { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public double PrecioBase { get; set; }
        public int StockActual { get; set; }
        public int StockMinimo { get; set; }
    }

    /// <summary>
    /// DTO para crear/actualizar un producto.
    /// Si Id == 0, es creación. Si Id > 0, es actualización.
    /// </summary>
    public class ProductoGuardarDto
    {
        public int Id { get; set; }
        public int CategoriaId { get; set; }
        public string CodigoBarras { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public double PrecioBase { get; set; }
        public int StockActual { get; set; }
        public int StockMinimo { get; set; }
    }

    /// <summary>
    /// DTO para el combo de categorías.
    /// </summary>
    public class CategoriaDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
    }
}
