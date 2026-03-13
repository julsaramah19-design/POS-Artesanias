namespace ArtesaniasPOS.Core.ViewModels.Ventas
{
    /// <summary>
    /// Representa un producto agregado al carrito.
    /// 
    /// Hereda de ViewModelBase porque la cantidad es editable
    /// directamente en el DataGrid del carrito, y necesita
    /// notificar a la UI cuando cambia para recalcular el subtotal.
    /// </summary>
    public class CarritoItem : ViewModelBase
    {
        private int _cantidad;

        public int ProductoId { get; set; }
        public string CodigoBarras { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public double PrecioUnitario { get; set; }
        public int StockDisponible { get; set; }

        public int Cantidad
        {
            get => _cantidad;
            set
            {
                // No permitir más de lo que hay en stock
                var nuevaCantidad = Math.Max(1, Math.Min(value, StockDisponible));
                if (SetProperty(ref _cantidad, nuevaCantidad))
                    OnPropertyChanged(nameof(Subtotal));
            }
        }

        /// <summary>
        /// Subtotal calculado automáticamente.
        /// Se recalcula cada vez que cambia la cantidad.
        /// </summary>
        public double Subtotal => PrecioUnitario * Cantidad;
    }
}
