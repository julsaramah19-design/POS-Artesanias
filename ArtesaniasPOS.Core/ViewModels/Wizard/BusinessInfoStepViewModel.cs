using System.Windows.Input;
using ArtesaniasPOS.Core.Interfaces;

namespace ArtesaniasPOS.Core.ViewModels.Wizard
{
    /// <summary>
    /// ViewModel para el paso "Datos del Negocio".
    /// Recibe IConfiguracionService (interfaz), no la clase concreta.
    /// </summary>
    public class BusinessInfoStepViewModel : ViewModelBase
    {
        private readonly IConfiguracionService _configuracionService;

        private string _nombreNegocio = string.Empty;
        private string _nit = string.Empty;
        private string _telefono = string.Empty;
        private string _direccion = string.Empty;
        private string _email = string.Empty;
        private string _logoPath = string.Empty;
        private bool _isLoading;

        public BusinessInfoStepViewModel(IConfiguracionService configuracionService)
        {
            _configuracionService = configuracionService;
            SeleccionarLogoCommand = new RelayCommand(_ => SeleccionarLogo());
        }

        #region Propiedades

        public string NombreNegocio
        {
            get => _nombreNegocio;
            set => SetProperty(ref _nombreNegocio, value);
        }

        public string NIT
        {
            get => _nit;
            set => SetProperty(ref _nit, value);
        }

        public string Telefono
        {
            get => _telefono;
            set => SetProperty(ref _telefono, value);
        }

        public string Direccion
        {
            get => _direccion;
            set => SetProperty(ref _direccion, value);
        }

        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        public string LogoPath
        {
            get => _logoPath;
            set => SetProperty(ref _logoPath, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        #endregion

        public ICommand SeleccionarLogoCommand { get; }

        public async Task CargarDatosAsync()
        {
            IsLoading = true;
            try
            {
                var config = await _configuracionService.ObtenerTodasAsync();

                NombreNegocio = config.GetValueOrDefault("NombreNegocio", "");
                NIT = config.GetValueOrDefault("NIT", "");
                Telefono = config.GetValueOrDefault("Telefono", "");
                Direccion = config.GetValueOrDefault("Direccion", "");
                Email = config.GetValueOrDefault("Email", "");
                LogoPath = config.GetValueOrDefault("LogoPath", "");
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task GuardarDatosAsync()
        {
            var datos = new Dictionary<string, string>
            {
                ["NombreNegocio"] = NombreNegocio.Trim(),
                ["NIT"] = NIT.Trim(),
                ["Telefono"] = Telefono.Trim(),
                ["Direccion"] = Direccion.Trim(),
                ["Email"] = Email.Trim(),
                ["LogoPath"] = LogoPath
            };

            await _configuracionService.GuardarVariosAsync(datos);
        }

        public bool EsValido => !string.IsNullOrWhiteSpace(NombreNegocio);

        private void SeleccionarLogo()
        {
            // La vista intercepta este comando y abre el OpenFileDialog.
        }
    }
}
