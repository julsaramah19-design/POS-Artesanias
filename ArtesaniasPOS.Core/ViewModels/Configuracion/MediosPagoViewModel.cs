using System.Collections.ObjectModel;
using System.Windows.Input;
using ArtesaniasPOS.Core.Interfaces;

namespace ArtesaniasPOS.Core.ViewModels.Configuracion
{
    public class MediosPagoViewModel : ViewModelBase
    {
        private readonly IConfiguracionAdminService _service;
        private MedioPagoAdminDto? _seleccionado;
        private string _nombre = string.Empty;
        private bool _esEdicion;
        private int _editandoId;
        private string _mensajeError = string.Empty;

        public MediosPagoViewModel(IConfiguracionAdminService service)
        {
            _service = service;
            GuardarCommand = new AsyncRelayCommand(async _ => await GuardarAsync());
            EditarCommand = new RelayCommand(_ => PrepararEdicion(), _ => Seleccionado != null);
            EliminarCommand = new AsyncRelayCommand(async _ => await EliminarAsync(), _ => Seleccionado != null);
            CancelarCommand = new RelayCommand(_ => LimpiarFormulario());
        }

        public ObservableCollection<MedioPagoAdminDto> MediosPago { get; } = new();
        public MedioPagoAdminDto? Seleccionado { get => _seleccionado; set => SetProperty(ref _seleccionado, value); }
        public string Nombre { get => _nombre; set => SetProperty(ref _nombre, value); }
        public bool EsEdicion { get => _esEdicion; set { SetProperty(ref _esEdicion, value); OnPropertyChanged(nameof(TextoBoton)); } }
        public string TextoBoton => EsEdicion ? "Actualizar" : "Agregar";
        public string MensajeError { get => _mensajeError; set => SetProperty(ref _mensajeError, value); }

        public ICommand GuardarCommand { get; }
        public ICommand EditarCommand { get; }
        public ICommand EliminarCommand { get; }
        public ICommand CancelarCommand { get; }

        public async Task CargarAsync()
        {
            var medios = await _service.ObtenerMediosPagoAsync();
            MediosPago.Clear();
            foreach (var m in medios) MediosPago.Add(m);
        }

        private void PrepararEdicion()
        {
            if (Seleccionado == null) return;
            EsEdicion = true; _editandoId = Seleccionado.Id;
            Nombre = Seleccionado.Nombre; MensajeError = string.Empty;
        }

        private async Task GuardarAsync()
        {
            MensajeError = string.Empty;
            if (string.IsNullOrWhiteSpace(Nombre)) { MensajeError = "El nombre es obligatorio."; return; }
            try
            {
                if (EsEdicion) await _service.ActualizarMedioPagoAsync(_editandoId, Nombre);
                else await _service.CrearMedioPagoAsync(Nombre);
                LimpiarFormulario(); await CargarAsync();
            }
            catch (Exception ex) { MensajeError = $"Error: {ex.Message}"; }
        }

        private async Task EliminarAsync()
        {
            if (Seleccionado == null) return;
            await _service.DesactivarMedioPagoAsync(Seleccionado.Id);
            await CargarAsync();
        }

        private void LimpiarFormulario()
        { Nombre = string.Empty; EsEdicion = false; _editandoId = 0; MensajeError = string.Empty; }
    }
}
