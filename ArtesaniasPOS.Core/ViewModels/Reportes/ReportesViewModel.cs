using System.Collections.ObjectModel;
using System.Windows.Input;
using ArtesaniasPOS.Core.Interfaces;

namespace ArtesaniasPOS.Core.ViewModels.Reportes
{
    public class ReportesViewModel : ViewModelBase
    {
        private readonly IReporteService _reporteService;

        private DateTime _fechaDesde;
        private DateTime _fechaHasta;
        private int _tabSeleccionado;
        private bool _isLoading;
        private string _mensajeError = string.Empty;

        // KPIs
        private int _totalVentas;
        private double _ingresoTotal;
        private int _productosVendidos;
        private double _ticketPromedio;

        // Detalle de venta
        private VentaHistorialDto? _ventaSeleccionada;
        private bool _detalleVisible;

        public ReportesViewModel(IReporteService reporteService)
        {
            _reporteService = reporteService;

            _fechaDesde = DateTime.Today;
            _fechaHasta = DateTime.Today;

            FiltrarCommand = new AsyncRelayCommand(async _ => await CargarTodoAsync());

            HoyCommand = new AsyncRelayCommand(async _ =>
            {
                FechaDesde = DateTime.Today;
                FechaHasta = DateTime.Today;
                await CargarTodoAsync();
            });

            SemanaCommand = new AsyncRelayCommand(async _ =>
            {
                var hoy = DateTime.Today;
                var diff = (7 + (hoy.DayOfWeek - DayOfWeek.Monday)) % 7;
                FechaDesde = hoy.AddDays(-diff);
                FechaHasta = DateTime.Today;
                await CargarTodoAsync();
            });

            MesCommand = new AsyncRelayCommand(async _ =>
            {
                FechaDesde = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                FechaHasta = DateTime.Today;
                await CargarTodoAsync();
            });

            VerDetalleCommand = new AsyncRelayCommand(
                async _ => await VerDetalleAsync(),
                _ => VentaSeleccionada != null);

            CerrarDetalleCommand = new RelayCommand(_ => { DetalleVisible = false; });
        }

        #region Propiedades de filtro

        public DateTime FechaDesde
        {
            get => _fechaDesde;
            set => SetProperty(ref _fechaDesde, value);
        }

        public DateTime FechaHasta
        {
            get => _fechaHasta;
            set => SetProperty(ref _fechaHasta, value);
        }

        public int TabSeleccionado
        {
            get => _tabSeleccionado;
            set => SetProperty(ref _tabSeleccionado, value);
        }

        #endregion

        #region KPIs

        public int TotalVentas
        {
            get => _totalVentas;
            set => SetProperty(ref _totalVentas, value);
        }

        public double IngresoTotal
        {
            get => _ingresoTotal;
            set => SetProperty(ref _ingresoTotal, value);
        }

        public int ProductosVendidos
        {
            get => _productosVendidos;
            set => SetProperty(ref _productosVendidos, value);
        }

        public double TicketPromedio
        {
            get => _ticketPromedio;
            set => SetProperty(ref _ticketPromedio, value);
        }

        #endregion

        #region Detalle de venta

        public VentaHistorialDto? VentaSeleccionada
        {
            get => _ventaSeleccionada;
            set => SetProperty(ref _ventaSeleccionada, value);
        }

        public bool DetalleVisible
        {
            get => _detalleVisible;
            set => SetProperty(ref _detalleVisible, value);
        }

        public ObservableCollection<DetalleVentaHistorialDto> DetalleVenta { get; } = new();

        #endregion

        #region Colecciones

        public ObservableCollection<VentaHistorialDto> Historial { get; } = new();
        public ObservableCollection<ProductoVendidoDto> ProductosTop { get; } = new();
        public ObservableCollection<VentaPorVendedorDto> VentasPorVendedor { get; } = new();

        #endregion

        #region Estado

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public string MensajeError
        {
            get => _mensajeError;
            set => SetProperty(ref _mensajeError, value);
        }

        #endregion

        #region Comandos

        public ICommand FiltrarCommand { get; }
        public ICommand HoyCommand { get; }
        public ICommand SemanaCommand { get; }
        public ICommand MesCommand { get; }
        public ICommand VerDetalleCommand { get; }
        public ICommand CerrarDetalleCommand { get; }

        #endregion

        public async Task InicializarAsync()
        {
            await CargarTodoAsync();
        }

        private async Task CargarTodoAsync()
        {
            IsLoading = true;
            MensajeError = string.Empty;
            DetalleVisible = false;

            try
            {
                var tareaResumen = _reporteService.ObtenerResumenAsync(FechaDesde, FechaHasta);
                var tareaHistorial = _reporteService.ObtenerHistorialVentasAsync(FechaDesde, FechaHasta);
                var tareaTop = _reporteService.ObtenerProductosMasVendidosAsync(FechaDesde, FechaHasta);
                var tareaVendedores = _reporteService.ObtenerVentasPorVendedorAsync(FechaDesde, FechaHasta);

                await Task.WhenAll(tareaResumen, tareaHistorial, tareaTop, tareaVendedores);

                var resumen = tareaResumen.Result;
                TotalVentas = resumen.TotalVentas;
                IngresoTotal = resumen.IngresoTotal;
                ProductosVendidos = resumen.ProductosVendidos;
                TicketPromedio = resumen.TicketPromedio;

                Historial.Clear();
                foreach (var v in tareaHistorial.Result)
                    Historial.Add(v);

                ProductosTop.Clear();
                foreach (var p in tareaTop.Result)
                    ProductosTop.Add(p);

                VentasPorVendedor.Clear();
                foreach (var vp in tareaVendedores.Result)
                    VentasPorVendedor.Add(vp);
            }
            catch (Exception ex)
            {
                MensajeError = $"Error al cargar reportes: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task VerDetalleAsync()
        {
            if (VentaSeleccionada == null) return;

            try
            {
                var detalles = await _reporteService.ObtenerDetalleVentaAsync(VentaSeleccionada.VentaId);
                DetalleVenta.Clear();
                foreach (var d in detalles)
                    DetalleVenta.Add(d);
                DetalleVisible = true;
            }
            catch (Exception ex)
            {
                MensajeError = $"Error al cargar detalle: {ex.Message}";
            }
        }
    }
}
