using System.Collections.ObjectModel;
using System.Windows.Input;
using ArtesaniasPOS.Core.Interfaces;

namespace ArtesaniasPOS.Core.ViewModels.Configuracion
{
    public class CategoriasViewModel : ViewModelBase
    {
        private readonly IConfiguracionAdminService _service;
        private CategoriaAdminDto? _seleccionada;
        private string _nombre = string.Empty;
        private bool _esEdicion;
        private int _editandoId;
        private string _mensajeError = string.Empty;

        public CategoriasViewModel(IConfiguracionAdminService service)
        {
            _service = service;
            GuardarCommand = new AsyncRelayCommand(async _ => await GuardarAsync());
            EditarCommand = new RelayCommand(_ => PrepararEdicion(), _ => Seleccionada != null);
            EliminarCommand = new AsyncRelayCommand(async _ => await EliminarAsync(), _ => Seleccionada != null);
            CancelarCommand = new RelayCommand(_ => LimpiarFormulario());
        }

        public ObservableCollection<CategoriaAdminDto> Categorias { get; } = new();
        public CategoriaAdminDto? Seleccionada { get => _seleccionada; set => SetProperty(ref _seleccionada, value); }
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
            var categorias = await _service.ObtenerCategoriasAsync();
            Categorias.Clear();
            foreach (var c in categorias) Categorias.Add(c);
        }

        private void PrepararEdicion()
        {
            if (Seleccionada == null) return;
            EsEdicion = true; _editandoId = Seleccionada.Id;
            Nombre = Seleccionada.Nombre; MensajeError = string.Empty;
        }

        private async Task GuardarAsync()
        {
            MensajeError = string.Empty;
            if (string.IsNullOrWhiteSpace(Nombre)) { MensajeError = "El nombre es obligatorio."; return; }
            try
            {
                if (EsEdicion) await _service.ActualizarCategoriaAsync(_editandoId, Nombre);
                else await _service.CrearCategoriaAsync(Nombre);
                LimpiarFormulario(); await CargarAsync();
            }
            catch (Exception ex) { MensajeError = $"Error: {ex.Message}"; }
        }

        private async Task EliminarAsync()
        {
            if (Seleccionada == null) return;
            await _service.DesactivarCategoriaAsync(Seleccionada.Id);
            await CargarAsync();
        }

        private void LimpiarFormulario()
        { Nombre = string.Empty; EsEdicion = false; _editandoId = 0; MensajeError = string.Empty; }
    }
}
