using System.Windows.Input;
using ArtesaniasPOS.Core.Interfaces;

namespace ArtesaniasPOS.Core.ViewModels.Configuracion
{
    /// <summary>
    /// Edición de los datos del negocio (nombre, NIT, contacto). Estos valores
    /// se usan en el recibo y en el encabezado de la app.
    /// </summary>
    public class NegocioViewModel : ViewModelBase
    {
        private readonly IConfiguracionService _configuracionService;

        private string _nombreNegocio = string.Empty;
        private string _nit = string.Empty;
        private string _telefono = string.Empty;
        private string _direccion = string.Empty;
        private string _email = string.Empty;
        private string _mensajeExito = string.Empty;
        private string _mensajeError = string.Empty;
        private bool _isLoading;

        public NegocioViewModel(IConfiguracionService configuracionService)
        {
            _configuracionService = configuracionService;
            GuardarCommand = new AsyncRelayCommand(async _ => await GuardarAsync(), _ => !IsLoading);
        }

        public string NombreNegocio { get => _nombreNegocio; set => SetProperty(ref _nombreNegocio, value); }
        public string NIT { get => _nit; set => SetProperty(ref _nit, value); }
        public string Telefono { get => _telefono; set => SetProperty(ref _telefono, value); }
        public string Direccion { get => _direccion; set => SetProperty(ref _direccion, value); }
        public string Email { get => _email; set => SetProperty(ref _email, value); }

        public string MensajeExito { get => _mensajeExito; set => SetProperty(ref _mensajeExito, value); }
        public string MensajeError { get => _mensajeError; set => SetProperty(ref _mensajeError, value); }
        public bool IsLoading { get => _isLoading; set => SetProperty(ref _isLoading, value); }

        public ICommand GuardarCommand { get; }

        public async Task CargarAsync()
        {
            var config = await _configuracionService.ObtenerTodasAsync();
            NombreNegocio = config.GetValueOrDefault("NombreNegocio", "");
            NIT = config.GetValueOrDefault("NIT", "");
            Telefono = config.GetValueOrDefault("Telefono", "");
            Direccion = config.GetValueOrDefault("Direccion", "");
            Email = config.GetValueOrDefault("Email", "");
        }

        private async Task GuardarAsync()
        {
            MensajeError = string.Empty;
            MensajeExito = string.Empty;

            if (string.IsNullOrWhiteSpace(NombreNegocio))
            {
                MensajeError = "El nombre del negocio es obligatorio.";
                return;
            }

            IsLoading = true;
            try
            {
                await _configuracionService.GuardarVariosAsync(new Dictionary<string, string>
                {
                    ["NombreNegocio"] = NombreNegocio.Trim(),
                    ["NIT"] = NIT.Trim(),
                    ["Telefono"] = Telefono.Trim(),
                    ["Direccion"] = Direccion.Trim(),
                    ["Email"] = Email.Trim()
                });
                MensajeExito = "Datos del negocio guardados correctamente.";
                Core.Notifier.Exito("Datos del negocio guardados");
            }
            catch (Exception ex)
            {
                MensajeError = $"Error al guardar: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
