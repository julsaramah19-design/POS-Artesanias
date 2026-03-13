using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Input;
using ArtesaniasPOS.Core.Interfaces;

namespace ArtesaniasPOS.Core.ViewModels.Ventas
{
    /// <summary>
    /// ViewModel del punto de venta (POS).
    /// 
    /// Layout: Búsqueda arriba + Carrito derecha + Resumen abajo.
    /// 
    /// Flujo de una venta:
    /// 1. Buscar producto por código o nombre
    /// 2. Seleccionar producto → se agrega al carrito
    /// 3. Ajustar cantidades en el carrito
    /// 4. Opcionalmente aplicar descuento
    /// 5. Seleccionar moneda y medio de pago
    /// 6. Ingresar monto pagado → se calcula cambio
    /// 7. Confirmar venta → se registra y descuenta stock
    /// 8. Carrito se limpia para la siguiente venta
    /// </summary>
    public class VentasViewModel : ViewModelBase
    {
        private readonly IVentaService _ventaService;
        private readonly IMonedaService _monedaService;
        private readonly int _usuarioId;

        private string _textoBusqueda = string.Empty;
        private ProductoBusquedaDto? _productoSeleccionado;
        private MonedaDto? _monedaSeleccionada;
        private MedioPagoDto? _medioPagoSeleccionado;
        private double _descuento;
        private double _montoPagado;
        private string _mensajeError = string.Empty;
        private string _mensajeExito = string.Empty;
        private bool _isLoading;

        public VentasViewModel(
            IVentaService ventaService,
            IMonedaService monedaService,
            int usuarioId)
        {
            _ventaService = ventaService;
            _monedaService = monedaService;
            _usuarioId = usuarioId;

            // Escuchar cambios en el carrito para recalcular totales
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
                // No permitir descuento mayor al subtotal ni negativo
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

        #endregion

        #region Propiedades calculadas

        /// <summary>
        /// Suma de todos los subtotales del carrito (en moneda base).
        /// </summary>
        public double SubtotalCarrito => Carrito.Sum(i => i.Subtotal);

        /// <summary>
        /// Total después de descuento (en moneda base).
        /// </summary>
        public double TotalVenta => SubtotalCarrito - Descuento;

        /// <summary>
        /// Total convertido a la moneda seleccionada.
        /// Si la moneda es la base (tasa=1), queda igual.
        /// Si es USD con tasa 4100, divide: 50000 COP / 4100 = 12.19 USD
        /// </summary>
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

        /// <summary>
        /// Cambio a devolver al cliente en la moneda seleccionada.
        /// </summary>
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

        /// <summary>
        /// Carga inicial: monedas y medios de pago.
        /// </summary>
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

        /// <summary>
        /// Busca productos por código o nombre.
        /// </summary>
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

                // Si hay exactamente un resultado (ej: escaneó código de barras), agregar directo
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

        /// <summary>
        /// Agrega el producto seleccionado al carrito.
        /// Si ya existe, incrementa la cantidad.
        /// </summary>
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
                    Cantidad = 1
                });
            }

            RecalcularTotales();
        }

        /// <summary>
        /// Quita un item del carrito. Recibe el CarritoItem como parámetro
        /// del CommandParameter en XAML.
        /// </summary>
        private void QuitarDelCarrito(object? parameter)
        {
            if (parameter is CarritoItem item)
            {
                Carrito.Remove(item);
                RecalcularTotales();
            }
        }

        /// <summary>
        /// Confirma y registra la venta.
        /// </summary>
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

                MensajeExito = $"Venta #{ventaId} registrada. Cambio: {SimboloMoneda}{Cambio:N2}";

                // Limpiar para la siguiente venta
                LimpiarCarrito();
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

        /// <summary>
        /// Recalcula todas las propiedades calculadas y notifica a la UI.
        /// Se llama cada vez que cambia el carrito.
        /// </summary>
        private void RecalcularTotales()
        {
            OnPropertyChanged(nameof(SubtotalCarrito));
            OnPropertyChanged(nameof(TotalVenta));
            OnPropertyChanged(nameof(TotalEnMoneda));
            OnPropertyChanged(nameof(Cambio));
            OnPropertyChanged(nameof(CantidadItems));
        }

        /// <summary>
        /// Escucha cambios en la colección del carrito Y en cada item.
        /// Cuando se agrega un item, suscribe a su PropertyChanged
        /// para recalcular cuando cambie la cantidad.
        /// </summary>
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
