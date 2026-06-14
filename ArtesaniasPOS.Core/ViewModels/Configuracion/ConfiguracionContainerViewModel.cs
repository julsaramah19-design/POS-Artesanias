using System.Windows.Input;
using ArtesaniasPOS.Core.Interfaces;

namespace ArtesaniasPOS.Core.ViewModels.Configuracion
{
    public class ConfiguracionContainerViewModel : ViewModelBase
    {
        private readonly IConfiguracionAdminService _service;
        private readonly IConfiguracionService _configService;
        private readonly IBackupService _backupService;
        private ViewModelBase? _seccionActual;
        private int _seccionIndex;

        public ConfiguracionContainerViewModel(
            IConfiguracionAdminService service,
            IConfiguracionService configService,
            IBackupService backupService)
        {
            _service = service;
            _configService = configService;
            _backupService = backupService;

            NavNegocioCommand = new AsyncRelayCommand(async _ => await NavegarA(0));
            NavUsuariosCommand = new AsyncRelayCommand(async _ => await NavegarA(1));
            NavCategoriasCommand = new AsyncRelayCommand(async _ => await NavegarA(2));
            NavMediosPagoCommand = new AsyncRelayCommand(async _ => await NavegarA(3));
            NavRespaldoCommand = new AsyncRelayCommand(async _ => await NavegarA(4));
        }

        public ViewModelBase? SeccionActual
        {
            get => _seccionActual;
            set => SetProperty(ref _seccionActual, value);
        }

        public int SeccionIndex
        {
            get => _seccionIndex;
            set => SetProperty(ref _seccionIndex, value);
        }

        public ICommand NavNegocioCommand { get; }
        public ICommand NavUsuariosCommand { get; }
        public ICommand NavCategoriasCommand { get; }
        public ICommand NavMediosPagoCommand { get; }
        public ICommand NavRespaldoCommand { get; }

        public async Task InicializarAsync()
        {
            await NavegarA(0);
        }

        private async Task NavegarA(int seccion)
        {
            SeccionIndex = seccion;
            switch (seccion)
            {
                case 0:
                    var negocioVm = new NegocioViewModel(_configService);
                    SeccionActual = negocioVm;
                    await negocioVm.CargarAsync();
                    break;
                case 1:
                    var usuariosVm = new UsuariosViewModel(_service);
                    SeccionActual = usuariosVm;
                    await usuariosVm.CargarAsync();
                    break;
                case 2:
                    var categoriasVm = new CategoriasViewModel(_service);
                    SeccionActual = categoriasVm;
                    await categoriasVm.CargarAsync();
                    break;
                case 3:
                    var mediosVm = new MediosPagoViewModel(_service);
                    SeccionActual = mediosVm;
                    await mediosVm.CargarAsync();
                    break;
                case 4:
                    SeccionActual = new RespaldoViewModel(_backupService);
                    break;
            }
        }
    }
}
