using System.Windows.Input;
using ArtesaniasPOS.Core.Interfaces;

namespace ArtesaniasPOS.Core.ViewModels.Configuracion
{
    public class ConfiguracionContainerViewModel : ViewModelBase
    {
        private readonly IConfiguracionAdminService _service;
        private ViewModelBase? _seccionActual;
        private int _seccionIndex;

        public ConfiguracionContainerViewModel(IConfiguracionAdminService service)
        {
            _service = service;
            NavUsuariosCommand = new AsyncRelayCommand(async _ => await NavegarA(0));
            NavCategoriasCommand = new AsyncRelayCommand(async _ => await NavegarA(1));
            NavMediosPagoCommand = new AsyncRelayCommand(async _ => await NavegarA(2));
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

        public ICommand NavUsuariosCommand { get; }
        public ICommand NavCategoriasCommand { get; }
        public ICommand NavMediosPagoCommand { get; }

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
                    var usuariosVm = new UsuariosViewModel(_service);
                    SeccionActual = usuariosVm;
                    await usuariosVm.CargarAsync();
                    break;
                case 1:
                    var categoriasVm = new CategoriasViewModel(_service);
                    SeccionActual = categoriasVm;
                    await categoriasVm.CargarAsync();
                    break;
                case 2:
                    var mediosVm = new MediosPagoViewModel(_service);
                    SeccionActual = mediosVm;
                    await mediosVm.CargarAsync();
                    break;
            }
        }
    }
}
