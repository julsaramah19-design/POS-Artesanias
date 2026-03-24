using System.Collections.ObjectModel;
using ArtesaniasPOS.Core.Interfaces;

namespace ArtesaniasPOS.Core.ViewModels.Wizard
{
    public class CurrencyColorStepViewModel : ViewModelBase
    {
        private readonly IMonedaService _monedaService;
        private readonly IConfiguracionService _configuracionService;

        private MonedaDto? _monedaSeleccionada;
        private string _colorPrimario = "#3B82F6";
        private string _colorSecundario = "#1E40AF";
        private bool _isLoading;

        public CurrencyColorStepViewModel(
            IMonedaService monedaService,
            IConfiguracionService configuracionService)
        {
            _monedaService = monedaService;
            _configuracionService = configuracionService;
        }

        #region Propiedades

        public ObservableCollection<MonedaDto> Monedas { get; } = new();

        public MonedaDto? MonedaSeleccionada
        {
            get => _monedaSeleccionada;
            set => SetProperty(ref _monedaSeleccionada, value);
        }

        public string ColorPrimario
        {
            get => _colorPrimario;
            set => SetProperty(ref _colorPrimario, value);
        }

        public string ColorSecundario
        {
            get => _colorSecundario;
            set => SetProperty(ref _colorSecundario, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public List<string> ColoresPredefinidos { get; } = new()
        {
            "#3B82F6", "#10B981", "#F59E0B", "#EF4444", "#8B5CF6",
            "#EC4899", "#06B6D4", "#F97316", "#6366F1", "#14B8A6"
        };

        #endregion

        public async Task CargarDatosAsync()
        {
            IsLoading = true;
            try
            {
                var monedas = await _monedaService.ObtenerActivasAsync();
                Monedas.Clear();
                foreach (var moneda in monedas)
                    Monedas.Add(moneda);

                MonedaSeleccionada = Monedas.FirstOrDefault(m => m.EsMonedaBase)
                                     ?? Monedas.FirstOrDefault();

                var config = await _configuracionService.ObtenerTodasAsync();
                ColorPrimario = config.GetValueOrDefault("ColorPrimario", "#3B82F6");
                ColorSecundario = config.GetValueOrDefault("ColorSecundario", "#1E40AF");
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task GuardarDatosAsync()
        {
            if (MonedaSeleccionada != null)
                await _monedaService.EstablecerMonedaBaseAsync(MonedaSeleccionada.Id);

            var colores = new Dictionary<string, string>
            {
                ["ColorPrimario"] = ColorPrimario,
                ["ColorSecundario"] = ColorSecundario
            };

            await _configuracionService.GuardarVariosAsync(colores);
        }

        public bool EsValido => MonedaSeleccionada != null;
    }
}
