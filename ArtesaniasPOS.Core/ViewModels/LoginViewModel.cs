using System.Windows.Input;
using ArtesaniasPOS.Core.Interfaces;

namespace ArtesaniasPOS.Core.ViewModels
{
    /// <summary>
    /// ViewModel para la pantalla de Login.
    /// 
    /// Maneja la validación de credenciales y notifica al MainWindow
    /// cuando el login es exitoso, pasando la SesionUsuario para
    /// que el menú se filtre por perfil.
    /// </summary>
    public class LoginViewModel : ViewModelBase
    {
        private readonly IAuthService _authService;

        private string _nombreUsuario = string.Empty;
        private string _password = string.Empty;
        private string _mensajeError = string.Empty;
        private bool _isLoading;
        private string _nombreNegocio = string.Empty;

        public LoginViewModel(IAuthService authService, IConfiguracionService configuracionService)
        {
            _authService = authService;

            LoginCommand = new AsyncRelayCommand(
                execute: async _ => await LoginAsync(),
                canExecute: _ => !IsLoading);

            // Cargar nombre del negocio para mostrarlo en el login
            _ = CargarNombreNegocioAsync(configuracionService);
        }

        #region Propiedades

        public string NombreUsuario
        {
            get => _nombreUsuario;
            set
            {
                SetProperty(ref _nombreUsuario, value);
                MensajeError = string.Empty;
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                SetProperty(ref _password, value);
                MensajeError = string.Empty;
            }
        }

        public string MensajeError
        {
            get => _mensajeError;
            set => SetProperty(ref _mensajeError, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public string NombreNegocio
        {
            get => _nombreNegocio;
            set => SetProperty(ref _nombreNegocio, value);
        }

        #endregion

        public ICommand LoginCommand { get; }

        /// <summary>
        /// Evento que MainWindow escucha para saber que el login fue exitoso.
        /// Pasa la SesionUsuario con los datos del usuario logueado.
        /// </summary>
        public event EventHandler<SesionUsuario>? LoginExitoso;

        private async Task CargarNombreNegocioAsync(IConfiguracionService configuracionService)
        {
            NombreNegocio = await configuracionService.ObtenerValorAsync("NombreNegocio");
        }

        private async Task LoginAsync()
        {
            MensajeError = string.Empty;

            if (string.IsNullOrWhiteSpace(NombreUsuario))
            {
                MensajeError = "Ingresa tu nombre de usuario.";
                return;
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                MensajeError = "Ingresa tu contraseña.";
                return;
            }

            IsLoading = true;
            try
            {
                var sesion = await _authService.LoginAsync(NombreUsuario.Trim(), Password);

                if (sesion == null)
                {
                    MensajeError = "Usuario o contraseña incorrectos.";
                    return;
                }

                LoginExitoso?.Invoke(this, sesion);
            }
            catch (Exception ex)
            {
                MensajeError = $"Error al conectar: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
