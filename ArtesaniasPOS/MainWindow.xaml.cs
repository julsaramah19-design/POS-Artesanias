using System.Windows;
using ArtesaniasPOS.Core.Interfaces;
using ArtesaniasPOS.Core.ViewModels;
using ArtesaniasPOS.Core.ViewModels.Wizard;
using ArtesaniasPOS.Data.Services;

namespace ArtesaniasPOS.UI
{
    public partial class MainWindow : Window
    {
        private readonly IConfiguracionService _configuracionService;
        private readonly IMonedaService _monedaService;
        private readonly IUsuarioService _usuarioService;
        private readonly IAuthService _authService;
        private readonly IProductoService _productoService;
        private readonly IVentaService _ventaService;
        private readonly IReporteService _reporteService;
        private readonly IConfiguracionAdminService _configuracionAdminService;

        public MainWindow()
        {
            InitializeComponent();

            var connectionString = $"Data Source={ObtenerRutaBD()}";

            _configuracionService = new ConfiguracionService(connectionString);
            _monedaService = new MonedaService(connectionString);
            _usuarioService = new UsuarioService(connectionString);
            _authService = new AuthService(connectionString);
            _productoService = new ProductoService(connectionString);
            _ventaService = new VentaService(connectionString);
            _reporteService = new ReporteService(connectionString);
            _configuracionAdminService = new ConfiguracionAdminService(connectionString);

            var dbInit = new ArtesaniasPOS.Data.Database.DatabaseInitializer(connectionString);
            dbInit.Initialize();

            Loaded += async (s, e) => await VerificarWizardAsync();
        }

        private string ObtenerRutaBD()
        {
            var carpeta = AppDomain.CurrentDomain.BaseDirectory;
            return System.IO.Path.Combine(carpeta, "ArtesaniasPOS.db");
        }
        private async Task VerificarWizardAsync()
        {
            try
            {
                var wizardCompletado = await _configuracionService.WizardCompletadoAsync();

                if (wizardCompletado)
                    MostrarLogin();
                else
                    await MostrarWizardAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al iniciar:\n{ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                LoadingPanel.Visibility = Visibility.Collapsed;
            }
        }

        private async Task MostrarWizardAsync()
        {
            var wizardVm = new WizardContainerViewModel(
                _configuracionService, _monedaService, _usuarioService);

            wizardVm.WizardFinalizado += (s, e) => MostrarLogin();

            WizardContainer.DataContext = wizardVm;
            OcultarTodo();
            WizardContainer.Visibility = Visibility.Visible;

            await wizardVm.InicializarAsync();
        }

        private void MostrarLogin()
        {
            var loginVm = new LoginViewModel(_authService, _configuracionService);

            loginVm.LoginExitoso += async (s, sesion) =>
            {
                await MostrarShellAsync(sesion);
            };

            LoginContainer.DataContext = loginVm;
            OcultarTodo();
            LoginContainer.Visibility = Visibility.Visible;

            Title = "ArtesaniasPOS — Iniciar sesión";
        }

        private async Task MostrarShellAsync(SesionUsuario sesion)
        {
            var shellVm = new ShellViewModel(sesion, _configuracionService, _productoService, _ventaService, _monedaService, _reporteService, _configuracionAdminService);


            shellVm.SesionCerrada += (s, e) => MostrarLogin();

            ShellContainer.DataContext = shellVm;
            OcultarTodo();
            ShellContainer.Visibility = Visibility.Visible;

            await shellVm.CargarConfiguracionAsync();

            Title = $"ArtesaniasPOS — {sesion.Nombre}";
        }

        private void OcultarTodo()
        {
            WizardContainer.Visibility = Visibility.Collapsed;
            LoginContainer.Visibility = Visibility.Collapsed;
            ShellContainer.Visibility = Visibility.Collapsed;
            LoadingPanel.Visibility = Visibility.Collapsed;
        }
    }
}
