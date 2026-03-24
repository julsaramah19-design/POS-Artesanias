using System.Windows.Input;
using ArtesaniasPOS.Core.Interfaces;

namespace ArtesaniasPOS.Core.ViewModels.Wizard
{
    /// <summary>
    /// Orquestador principal del Wizard.
    /// Controla navegación, validación, guardado y finalización.
    /// Recibe interfaces — no sabe nada de Dapper, SQLite ni Data.
    /// </summary>
    public class WizardContainerViewModel : ViewModelBase
    {
        private readonly IConfiguracionService _configuracionService;
        private readonly List<ViewModelBase> _pasos;

        private int _indicePasoActual;
        private ViewModelBase _pasoActualViewModel;
        private bool _isLoading;
        private string _mensajeError = string.Empty;

        public WizardContainerViewModel(
            IConfiguracionService configuracionService,
            IMonedaService monedaService,
            IUsuarioService usuarioService)
        {
            _configuracionService = configuracionService;

            _pasos = new List<ViewModelBase>
            {
                new WelcomeStepViewModel(),
                new BusinessInfoStepViewModel(configuracionService),
                new CurrencyColorStepViewModel(monedaService, configuracionService),
                new AdminUserStepViewModel(usuarioService)
            };

            _pasoActualViewModel = _pasos[0];
            _indicePasoActual = 0;

            SiguienteCommand = new AsyncRelayCommand(
                execute: async _ => await SiguienteAsync(),
                canExecute: _ => PuedeAvanzar());

            AnteriorCommand = new RelayCommand(
                execute: _ => Anterior(),
                canExecute: _ => PuedeRetroceder());
        }

        #region Propiedades

        public ViewModelBase PasoActualViewModel
        {
            get => _pasoActualViewModel;
            private set => SetProperty(ref _pasoActualViewModel, value);
        }

        public int IndicePasoActual
        {
            get => _indicePasoActual;
            private set
            {
                SetProperty(ref _indicePasoActual, value);
                OnPropertyChanged(nameof(NumeroPaso));
                OnPropertyChanged(nameof(TotalPasos));
                OnPropertyChanged(nameof(EsPrimerPaso));
                OnPropertyChanged(nameof(EsUltimoPaso));
                OnPropertyChanged(nameof(TextoBotonSiguiente));
            }
        }

        public int NumeroPaso => _indicePasoActual + 1;
        public int TotalPasos => _pasos.Count;
        public bool EsPrimerPaso => _indicePasoActual == 0;
        public bool EsUltimoPaso => _indicePasoActual == _pasos.Count - 1;
        public string TextoBotonSiguiente => EsUltimoPaso ? "Finalizar" : "Siguiente";

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

        public ICommand SiguienteCommand { get; }
        public ICommand AnteriorCommand { get; }

        #endregion

        public event EventHandler? WizardFinalizado;

        public async Task InicializarAsync()
        {
            await CargarDatosPasoActualAsync();
        }

        private async Task SiguienteAsync()
        {
            MensajeError = string.Empty;
            IsLoading = true;

            try
            {
                var guardadoExitoso = await GuardarPasoActualAsync();
                if (!guardadoExitoso) return;

                if (EsUltimoPaso)
                {
                    await FinalizarWizardAsync();
                    return;
                }

                IndicePasoActual++;
                PasoActualViewModel = _pasos[_indicePasoActual];
                await CargarDatosPasoActualAsync();
            }
            catch (Exception ex)
            {
                MensajeError = $"Error: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void Anterior()
        {
            if (!PuedeRetroceder()) return;

            MensajeError = string.Empty;
            IndicePasoActual--;
            PasoActualViewModel = _pasos[_indicePasoActual];
        }

        private bool PuedeAvanzar() => !IsLoading;
        private bool PuedeRetroceder() => _indicePasoActual > 0 && !IsLoading;

        private async Task<bool> GuardarPasoActualAsync()
        {
            switch (PasoActualViewModel)
            {
                case BusinessInfoStepViewModel businessVm:
                    if (!businessVm.EsValido)
                    {
                        MensajeError = "El nombre del negocio es obligatorio.";
                        return false;
                    }
                    await businessVm.GuardarDatosAsync();
                    return true;

                case CurrencyColorStepViewModel currencyVm:
                    if (!currencyVm.EsValido)
                    {
                        MensajeError = "Debes seleccionar una moneda base.";
                        return false;
                    }
                    await currencyVm.GuardarDatosAsync();
                    return true;

                case AdminUserStepViewModel adminVm:
                    return await adminVm.GuardarDatosAsync();

                default:
                    return true;
            }
        }

        private async Task CargarDatosPasoActualAsync()
        {
            switch (PasoActualViewModel)
            {
                case BusinessInfoStepViewModel businessVm:
                    await businessVm.CargarDatosAsync();
                    break;

                case CurrencyColorStepViewModel currencyVm:
                    await currencyVm.CargarDatosAsync();
                    break;

                case AdminUserStepViewModel adminVm:
                    await adminVm.CargarDatosAsync();
                    break;
            }
        }

        private async Task FinalizarWizardAsync()
        {
            await _configuracionService.MarcarWizardCompletadoAsync();
            WizardFinalizado?.Invoke(this, EventArgs.Empty);
        }
    }
}
