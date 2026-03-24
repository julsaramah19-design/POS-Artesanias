using ArtesaniasPOS.Core.Interfaces;

namespace ArtesaniasPOS.Core.ViewModels.Wizard
{
    public class AdminUserStepViewModel : ViewModelBase
    {
        private readonly IUsuarioService _usuarioService;

        private string _nombre = string.Empty;
        private string _nombreUsuario = string.Empty;
        private string _password = string.Empty;
        private string _confirmarPassword = string.Empty;
        private string _mensajeError = string.Empty;
        private bool _isLoading;

        public AdminUserStepViewModel(IUsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        #region Propiedades

        public string Nombre
        {
            get => _nombre;
            set => SetProperty(ref _nombre, value);
        }

        public string NombreUsuario
        {
            get => _nombreUsuario;
            set => SetProperty(ref _nombreUsuario, value);
        }

        public string Password
        {
            get => _password;
            set
            {
                SetProperty(ref _password, value);
                OnPropertyChanged(nameof(EsValido));
                OnPropertyChanged(nameof(PasswordsCoinciden));
            }
        }

        public string ConfirmarPassword
        {
            get => _confirmarPassword;
            set
            {
                SetProperty(ref _confirmarPassword, value);
                OnPropertyChanged(nameof(EsValido));
                OnPropertyChanged(nameof(PasswordsCoinciden));
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

        public bool PasswordsCoinciden =>
            !string.IsNullOrEmpty(Password) &&
            Password == ConfirmarPassword;

        #endregion

        public async Task CargarDatosAsync()
        {
            IsLoading = true;
            try
            {
                var admin = await _usuarioService.ObtenerAdminActualAsync();
                if (admin != null)
                {
                    Nombre = admin.Nombre;
                    NombreUsuario = admin.NombreUsuario;
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task<bool> GuardarDatosAsync()
        {
            MensajeError = string.Empty;

            if (string.IsNullOrWhiteSpace(Nombre))
            {
                MensajeError = "El nombre es obligatorio.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(NombreUsuario))
            {
                MensajeError = "El nombre de usuario es obligatorio.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                MensajeError = "La contraseña es obligatoria.";
                return false;
            }

            if (Password.Length < 6)
            {
                MensajeError = "La contraseña debe tener al menos 6 caracteres.";
                return false;
            }

            if (Password != ConfirmarPassword)
            {
                MensajeError = "Las contraseñas no coinciden.";
                return false;
            }

            var existe = await _usuarioService.ExisteNombreUsuarioAsync(NombreUsuario);
            if (existe)
            {
                MensajeError = "Ese nombre de usuario ya está en uso.";
                return false;
            }

            await _usuarioService.ActualizarAdminAsync(Nombre, NombreUsuario, Password);
            return true;
        }

        public bool EsValido =>
            !string.IsNullOrWhiteSpace(Nombre) &&
            !string.IsNullOrWhiteSpace(NombreUsuario) &&
            !string.IsNullOrWhiteSpace(Password) &&
            Password.Length >= 6 &&
            Password == ConfirmarPassword;
    }
}
