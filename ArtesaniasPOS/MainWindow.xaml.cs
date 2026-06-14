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
        private readonly IBackupService _backupService;

        public MainWindow()
        {
            InitializeComponent();

            MigrarBaseDatosSiHaceFalta();

            var rutaBd = ArtesaniasPOS.Data.Database.AppSettings.DbPath;
            var connectionString = $"Data Source={rutaBd}";

            _configuracionService = new ConfiguracionService(connectionString);
            _monedaService = new MonedaService(connectionString);
            _usuarioService = new UsuarioService(connectionString);
            _authService = new AuthService(connectionString);
            _productoService = new ProductoService(connectionString);
            _ventaService = new VentaService(connectionString);
            _reporteService = new ReporteService(connectionString);
            _configuracionAdminService = new ConfiguracionAdminService(connectionString);
            _backupService = new BackupService();

            var dbInit = new ArtesaniasPOS.Data.Database.DatabaseInitializer(connectionString);
            dbInit.Initialize();

            // Notificaciones (toasts) disponibles en toda la app.
            ToastService.Initialize(ToastHost);
            ArtesaniasPOS.Core.Notifier.Handler = (mensaje, tipo) => ToastService.Show(mensaje, tipo);

            // Respaldo automático al cerrar la aplicación.
            Closed += (s, e) => _backupService.RespaldoAutomatico();

            Loaded += async (s, e) => await VerificarWizardAsync();
        }

        /// <summary>
        /// Versiones anteriores guardaban la base de datos junto al ejecutable
        /// (carpeta bin), que se borra al recompilar. Si existe esa base antigua
        /// y aún no hay una en la ubicación persistente (%APPDATA%), la copiamos
        /// para no perder la información.
        /// </summary>
        private void MigrarBaseDatosSiHaceFalta()
        {
            try
            {
                var destino = ArtesaniasPOS.Data.Database.AppSettings.DbPath;
                if (System.IO.File.Exists(destino)) return;

                var legacy = System.IO.Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory, "ArtesaniasPOS.db");
                if (System.IO.File.Exists(legacy))
                {
                    System.IO.Directory.CreateDirectory(
                        System.IO.Path.GetDirectoryName(destino)!);
                    System.IO.File.Copy(legacy, destino);
                }
            }
            catch { /* si falla la migración, se creará una base nueva */ }
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
            var shellVm = new ShellViewModel(sesion, _configuracionService, _productoService, _ventaService, _monedaService, _reporteService, _configuracionAdminService, _backupService);


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
