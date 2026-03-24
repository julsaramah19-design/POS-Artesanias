namespace ArtesaniasPOS.Core.ViewModels.Ventas
{
    public class CarritoItem : ViewModelBase
    {
        private int _cantidad;
        private double _descuentoItem;
        private double _montoMinimoDescuento = 60_000;

        public int ProductoId { get; set; }
        public string CodigoBarras { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public double PrecioUnitario { get; set; }
        public int StockDisponible { get; set; }

        public double MontoMinimoDescuento
        {
            get => _montoMinimoDescuento;
            set
            {
                if (SetProperty(ref _montoMinimoDescuento, value))
                    OnPropertyChanged(nameof(PermiteDescuento));
            }
        }

        public bool PermiteDescuento => PrecioUnitario >= MontoMinimoDescuento;

        public int Cantidad
        {
            get => _cantidad;
            set
            {
                var nuevaCantidad = Math.Max(1, Math.Min(value, StockDisponible));
                if (SetProperty(ref _cantidad, nuevaCantidad))
                {
                    var subtotalBruto = PrecioUnitario * nuevaCantidad;
                    if (_descuentoItem > subtotalBruto)
                    {
                        _descuentoItem = subtotalBruto;
                        OnPropertyChanged(nameof(DescuentoItem));
                    }
                    OnPropertyChanged(nameof(Subtotal));
                }
            }
        }

        public double DescuentoItem
        {
            get => _descuentoItem;
            set
            {
                if (!PermiteDescuento)
                {
                    _descuentoItem = 0;
                    return;
                }
                var subtotalBruto = PrecioUnitario * Cantidad;
                var nuevoDescuento = Math.Max(0, Math.Min(value, subtotalBruto));
                if (SetProperty(ref _descuentoItem, nuevoDescuento))
                    OnPropertyChanged(nameof(Subtotal));
            }
        }

        public double Subtotal => (PrecioUnitario * Cantidad) - DescuentoItem;
    }
}