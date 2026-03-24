using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Input;
using ArtesaniasPOS.Core.Interfaces;

namespace ArtesaniasPOS.Core.ViewModels.Ventas
{
    public class VentasViewModel : ViewModelBase
    {
        private readonly IVentaService _ventaService;
        private readonly IMonedaService _monedaService;
        private readonly int _usuarioId;
        private readonly IConfiguracionService _configuracionService;
        private readonly string _nombreVendedor;
        private string _nombreNegocio = string.Empty;
        private double _montoMinimoDescuento = 60_000;

        private string _textoBusqueda = string.Empty;
        private ProductoBusquedaDto? _productoSeleccionado;
        private MonedaDto? _monedaSeleccionada;
        private MedioPagoDto? _medioPagoSeleccionado;
        private double _descuento;
        private double _montoPagado;
        private string _mensajeError = string.Empty;
        private string _mensajeExito = string.Empty;
        private bool _isLoading;

        public VentasViewModel(IVentaService ventaService, IMonedaService monedaService, IConfiguracionService configuracionService, int usuarioId, string nombreVendedor)
        {
            _ventaService = ventaService;
            _monedaService = monedaService;
            _configuracionService = configuracionService;
            _usuarioId = usuarioId;
            _nombreVendedor = nombreVendedor;

            Carrito.CollectionChanged += OnCarritoChanged;

            BuscarCommand = new AsyncRelayCommand(async _ => await BuscarAsync());
            AgregarAlCarritoCommand = new RelayCommand(
                _ => AgregarAlCarrito(),
                _ => ProductoSeleccionado != null);
            QuitarDelCarritoCommand = new RelayCommand(QuitarDelCarrito);
            ConfirmarVentaCommand = new AsyncRelayCommand(
                async _ => await ConfirmarVentaAsync(),
                _ => PuedeConfirmar());
            LimpiarCarritoCommand = new RelayCommand(_ => LimpiarCarrito());
        }

        #region Colecciones

        public ObservableCollection<ProductoBusquedaDto> ResultadosBusqueda { get; } = new();
        public ObservableCollection<CarritoItem> Carrito { get; } = new();
        public ObservableCollection<MonedaDto> Monedas { get; } = new();
        public ObservableCollection<MedioPagoDto> MediosPago { get; } = new();

        #endregion

        #region Propiedades de búsqueda

        public string TextoBusqueda
        {
            get => _textoBusqueda;
            set => SetProperty(ref _textoBusqueda, value);
        }

        public ProductoBusquedaDto? ProductoSeleccionado
        {
            get => _productoSeleccionado;
            set => SetProperty(ref _productoSeleccionado, value);
        }

        #endregion

        #region Propiedades de pago

        public MonedaDto? MonedaSeleccionada
        {
            get => _monedaSeleccionada;
            set
            {
                if (SetProperty(ref _monedaSeleccionada, value))
                {
                    OnPropertyChanged(nameof(TotalEnMoneda));
                    OnPropertyChanged(nameof(SimboloMoneda));
                    OnPropertyChanged(nameof(Cambio));
                }
            }
        }

        public bool PuedeAplicarDescuentoGlobal => Carrito.Any(i => i.PrecioUnitario >= MontoMinimoDescuento);

        public MedioPagoDto? MedioPagoSeleccionado
        {
            get => _medioPagoSeleccionado;
            set => SetProperty(ref _medioPagoSeleccionado, value);
        }

        public double Descuento
        {
            get => _descuento;
            set
            {
                if (!PuedeAplicarDescuentoGlobal)
                    value = 0;

                var nuevo = Math.Max(0, Math.Min(value, SubtotalCarrito));
                if (SetProperty(ref _descuento, nuevo))
                {
                    OnPropertyChanged(nameof(TotalVenta));
                    OnPropertyChanged(nameof(TotalEnMoneda));
                    OnPropertyChanged(nameof(Cambio));
                }
            }
        }

        public double MontoPagado
        {
            get => _montoPagado;
            set
            {
                if (SetProperty(ref _montoPagado, value))
                    OnPropertyChanged(nameof(Cambio));
            }
        }

        public double MontoMinimoDescuento
        {
            get => _montoMinimoDescuento;
            set => SetProperty(ref _montoMinimoDescuento, value);
        }

        #endregion

        #region Propiedades calculadas

        public double SubtotalCarrito => Carrito.Sum(i => i.Subtotal);

        public double TotalVenta => SubtotalCarrito - Descuento;

        public double TotalEnMoneda
        {
            get
            {
                if (MonedaSeleccionada == null || MonedaSeleccionada.TasaCambio == 0)
                    return TotalVenta;

                if (MonedaSeleccionada.EsMonedaBase)
                    return TotalVenta;

                return TotalVenta / MonedaSeleccionada.TasaCambio;
            }
        }

        public double Cambio
        {
            get
            {
                var diferencia = MontoPagado - TotalEnMoneda;
                return diferencia > 0 ? diferencia : 0;
            }
        }

        public string SimboloMoneda => MonedaSeleccionada?.Simbolo ?? "$";

        public int CantidadItems => Carrito.Sum(i => i.Cantidad);

        #endregion

        #region Mensajes

        public string MensajeError
        {
            get => _mensajeError;
            set => SetProperty(ref _mensajeError, value);
        }

        public string MensajeExito
        {
            get => _mensajeExito;
            set => SetProperty(ref _mensajeExito, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        #endregion

        #region Comandos

        public ICommand BuscarCommand { get; }
        public ICommand AgregarAlCarritoCommand { get; }
        public ICommand QuitarDelCarritoCommand { get; }
        public ICommand ConfirmarVentaCommand { get; }
        public ICommand LimpiarCarritoCommand { get; }

        #endregion

        public async Task InicializarAsync()
        {
            IsLoading = true;
            try
            {
                var monedas = await _monedaService.ObtenerActivasAsync();
                Monedas.Clear();
                foreach (var m in monedas)
                    Monedas.Add(m);
                MonedaSeleccionada = Monedas.FirstOrDefault(m => m.EsMonedaBase)
                                     ?? Monedas.FirstOrDefault();

                var minDescuentoStr = await _configuracionService.ObtenerValorAsync("MontoMinimoDescuento");
                if (double.TryParse(minDescuentoStr, out var minDescuento) && minDescuento > 0)
                    MontoMinimoDescuento = minDescuento;


                _nombreNegocio = await _configuracionService.ObtenerValorAsync("NombreNegocio");

                var medios = await _ventaService.ObtenerMediosPagoAsync();
                MediosPago.Clear();
                foreach (var mp in medios)
                    MediosPago.Add(mp);
                MedioPagoSeleccionado = MediosPago.FirstOrDefault();
            }
            catch (Exception ex)
            {
                MensajeError = $"Error al cargar: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task BuscarAsync()
        {
            if (string.IsNullOrWhiteSpace(TextoBusqueda)) return;

            MensajeError = string.Empty;
            try
            {
                var resultados = await _ventaService.BuscarProductosAsync(TextoBusqueda);
                ResultadosBusqueda.Clear();
                foreach (var r in resultados)
                    ResultadosBusqueda.Add(r);


                if (ResultadosBusqueda.Count == 1)
                {
                    ProductoSeleccionado = ResultadosBusqueda[0];
                    AgregarAlCarrito();
                    TextoBusqueda = string.Empty;
                    ResultadosBusqueda.Clear();
                }
            }
            catch (Exception ex)
            {
                MensajeError = $"Error al buscar: {ex.Message}";
            }
        }

        private void AgregarAlCarrito()
        {
            if (ProductoSeleccionado == null) return;

            MensajeError = string.Empty;
            MensajeExito = string.Empty;

            var existente = Carrito.FirstOrDefault(
                i => i.ProductoId == ProductoSeleccionado.Id);

            if (existente != null)
            {
                if (existente.Cantidad < existente.StockDisponible)
                    existente.Cantidad++;
                else
                    MensajeError = $"Stock máximo alcanzado para {existente.Nombre}.";
            }
            else
            {
                Carrito.Add(new CarritoItem
                {
                    ProductoId = ProductoSeleccionado.Id,
                    CodigoBarras = ProductoSeleccionado.CodigoBarras,
                    Nombre = ProductoSeleccionado.Nombre,
                    PrecioUnitario = ProductoSeleccionado.PrecioBase,
                    StockDisponible = ProductoSeleccionado.StockActual,
                    MontoMinimoDescuento = _montoMinimoDescuento,
                    Cantidad = 1
                });
            }

            RecalcularTotales();
        }

        private void QuitarDelCarrito(object? parameter)
        {
            if (parameter is CarritoItem item)
            {
                Carrito.Remove(item);
                RecalcularTotales();
            }
        }

        private async Task ConfirmarVentaAsync()
        {
            MensajeError = string.Empty;
            MensajeExito = string.Empty;

            if (Carrito.Count == 0)
            {
                MensajeError = "Agrega productos al carrito.";
                return;
            }

            if (MedioPagoSeleccionado == null)
            {
                MensajeError = "Selecciona un medio de pago.";
                return;
            }

            if (MonedaSeleccionada == null)
            {
                MensajeError = "Selecciona una moneda.";
                return;
            }

            if (MontoPagado < TotalEnMoneda)
            {
                MensajeError = "El monto pagado es insuficiente.";
                return;
            }

            IsLoading = true;
            try
            {
                var ventaDto = new VentaRegistroDto
                {
                    UsuarioId = _usuarioId,
                    MedioPagoId = MedioPagoSeleccionado.Id,
                    MonedaId = MonedaSeleccionada.Id,
                    TasaCambioUsada = MonedaSeleccionada.TasaCambio,
                    Subtotal = SubtotalCarrito,
                    Descuento = Descuento,
                    Total = TotalVenta,
                    TotalMonedaVenta = TotalEnMoneda,
                    MontoPagado = MontoPagado,
                    Cambio = Cambio,
                    Detalles = Carrito.Select(item => new DetalleVentaRegistroDto
                    {
                        ProductoId = item.ProductoId,
                        Cantidad = item.Cantidad,
                        PrecioUnitario = item.PrecioUnitario,
                        Subtotal = item.Subtotal
                    }).ToList()
                };

                var ventaId = await _ventaService.RegistrarVentaAsync(ventaDto);

                var recibo = new ReciboDto
                {
                    VentaId = ventaId,
                    NombreNegocio = _nombreNegocio,
                    Vendedor = _nombreVendedor,
                    Fecha = DateTime.Now,
                    MedioPago = MedioPagoSeleccionado!.Nombre,
                    Moneda = MonedaSeleccionada!.Simbolo,
                    Items = Carrito.Select(i => new ReciboItemDto
                    {
                        Nombre = i.Nombre,
                        Cantidad = i.Cantidad,
                        PrecioUnitario = i.PrecioUnitario,
                        Descuento = i.DescuentoItem,
                        Subtotal = i.Subtotal
                    }).ToList(),
                    Subtotal = SubtotalCarrito,
                    Total = TotalVenta,
                    MontoPagado = MontoPagado,
                    Cambio = Cambio
                };

                MensajeExito = $"Venta #{ventaId} registrada. Cambio: {SimboloMoneda}{Cambio:N2}";

                LimpiarCarrito();

                ReciboGenerado?.Invoke(this, recibo);
            }
            catch (Exception ex)
            {
                MensajeError = $"Error al registrar venta: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private bool PuedeConfirmar() => Carrito.Count > 0 && !IsLoading;

        private void LimpiarCarrito()
        {
            Carrito.Clear();
            Descuento = 0;
            MontoPagado = 0;
            TextoBusqueda = string.Empty;
            ResultadosBusqueda.Clear();
            ProductoSeleccionado = null;
            RecalcularTotales();
        }

        public event EventHandler<ReciboDto>? ReciboGenerado;

        private void RecalcularTotales()
        {
            OnPropertyChanged(nameof(SubtotalCarrito));
            OnPropertyChanged(nameof(TotalVenta));
            OnPropertyChanged(nameof(TotalEnMoneda));
            OnPropertyChanged(nameof(Cambio));
            OnPropertyChanged(nameof(CantidadItems));
            OnPropertyChanged(nameof(PuedeAplicarDescuentoGlobal));

            if (!PuedeAplicarDescuentoGlobal && Descuento > 0)
                Descuento = 0;
        }

        private void OnCarritoChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (CarritoItem item in e.NewItems)
                    item.PropertyChanged += OnCarritoItemChanged;
            }

            if (e.OldItems != null)
            {
                foreach (CarritoItem item in e.OldItems)
                    item.PropertyChanged -= OnCarritoItemChanged;
            }

            RecalcularTotales();
        }

        private void OnCarritoItemChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CarritoItem.Subtotal))
                RecalcularTotales();
        }
    }
}