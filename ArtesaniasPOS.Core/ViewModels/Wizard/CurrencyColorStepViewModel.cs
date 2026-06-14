using System.Collections.ObjectModel;
using ArtesaniasPOS.Core.Interfaces;

namespace ArtesaniasPOS.Core.ViewModels.Wizard
{
    public class CurrencyColorStepViewModel : ViewModelBase
    {
        private readonly IMonedaService _monedaService;

        private MonedaDto? _monedaSeleccionada;
        private bool _isLoading;

        public CurrencyColorStepViewModel(
            IMonedaService monedaService,
            IConfiguracionService configuracionService)
        {
            _monedaService = monedaService;
        }

        #region Propiedades

        public ObservableCollection<MonedaDto> Monedas { get; } = new();

        public MonedaDto? MonedaSeleccionada
        {
            get => _monedaSeleccionada;
            set => SetProperty(ref _monedaSeleccionada, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

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
        }

        public bool EsValido => MonedaSeleccionada != null;
    }
}
