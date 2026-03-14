using System.Collections.ObjectModel;
using System.Windows.Input;
using ArtesaniasPOS.Core.Interfaces;

namespace ArtesaniasPOS.Core.ViewModels.Configuracion
{
    public class UsuariosViewModel : ViewModelBase
    {
        private readonly IConfiguracionAdminService _service;
        private UsuarioListaDto? _seleccionado;
        private string _nombre = string.Empty;
        private string _nombreUsuario = string.Empty;
        private string _password = string.Empty;
        private PerfilDto? _perfilSeleccionado;
        private bool _esEdicion;
        private int _editandoId;
        private string _mensajeError = string.Empty;
        private string _mensajeExito = string.Empty;
        private bool _formularioVisible;

        public UsuariosViewModel(IConfiguracionAdminService service)
        {
            _service = service;
            NuevoCommand = new AsyncRelayCommand(async _ => await PrepararNuevo());
            GuardarCommand = new AsyncRelayCommand(async _ => await GuardarAsync());
            EditarCommand = new AsyncRelayCommand(async _ => PrepararEdicion(), _ => Seleccionado != null);
            EliminarCommand = new AsyncRelayCommand(async _ => await EliminarAsync(), _ => Seleccionado != null);
            CancelarCommand = new RelayCommand(_ => { FormularioVisible = false; });
        }

        public ObservableCollection<UsuarioListaDto> Usuarios { get; } = new();
        public ObservableCollection<PerfilDto> Perfiles { get; } = new();

        public UsuarioListaDto? Seleccionado { get => _seleccionado; set => SetProperty(ref _seleccionado, value); }
        public string Nombre { get => _nombre; set => SetProperty(ref _nombre, value); }
        public string NombreUsuario { get => _nombreUsuario; set => SetProperty(ref _nombreUsuario, value); }
        public string Password { get => _password; set => SetProperty(ref _password, value); }
        public PerfilDto? PerfilSeleccionado { get => _perfilSeleccionado; set => SetProperty(ref _perfilSeleccionado, value); }
        public bool EsEdicion { get => _esEdicion; set { SetProperty(ref _esEdicion, value); OnPropertyChanged(nameof(TituloFormulario)); } }
        public string TituloFormulario => EsEdicion ? "Editar usuario" : "Nuevo usuario";
        public bool FormularioVisible { get => _formularioVisible; set => SetProperty(ref _formularioVisible, value); }
        public string MensajeError { get => _mensajeError; set => SetProperty(ref _mensajeError, value); }
        public string MensajeExito { get => _mensajeExito; set => SetProperty(ref _mensajeExito, value); }

        public ICommand NuevoCommand { get; }
        public ICommand GuardarCommand { get; }
        public ICommand EditarCommand { get; }
        public ICommand EliminarCommand { get; }
        public ICommand CancelarCommand { get; }

        public async Task CargarAsync()
        {
            var usuarios = await _service.ObtenerUsuariosAsync();
            Usuarios.Clear();
            foreach (var u in usuarios) Usuarios.Add(u);

            var perfiles = await _service.ObtenerPerfilesAsync();
            Perfiles.Clear();
            foreach (var p in perfiles) Perfiles.Add(p);
        }

        private async Task PrepararNuevo()
        {
            EsEdicion = false; _editandoId = 0;
            Nombre = string.Empty; NombreUsuario = string.Empty; Password = string.Empty;
            PerfilSeleccionado = Perfiles.FirstOrDefault(p => p.Nombre == "Vendedor") ?? Perfiles.FirstOrDefault();
            MensajeError = string.Empty; MensajeExito = string.Empty;
            FormularioVisible = true;
        }

        private async Task PrepararEdicion()
        {
            if (Seleccionado == null) return;
            EsEdicion = true; _editandoId = Seleccionado.Id;
            Nombre = Seleccionado.Nombre; NombreUsuario = Seleccionado.NombreUsuario; Password = string.Empty;
            PerfilSeleccionado = Perfiles.FirstOrDefault(p => p.Id == Seleccionado.PerfilId);
            MensajeError = string.Empty; MensajeExito = string.Empty;
            FormularioVisible = true;
        }

        private async Task GuardarAsync()
        {
            MensajeError = string.Empty; MensajeExito = string.Empty;

            if (string.IsNullOrWhiteSpace(Nombre)) { MensajeError = "El nombre es obligatorio."; return; }
            if (string.IsNullOrWhiteSpace(NombreUsuario)) { MensajeError = "El nombre de usuario es obligatorio."; return; }
            if (!EsEdicion && string.IsNullOrWhiteSpace(Password)) { MensajeError = "La contraseña es obligatoria."; return; }
            if (!string.IsNullOrWhiteSpace(Password) && Password.Length < 6) { MensajeError = "Mínimo 6 caracteres."; return; }
            if (PerfilSeleccionado == null) { MensajeError = "Selecciona un perfil."; return; }

            try
            {
                if (EsEdicion)
                {
                    await _service.ActualizarUsuarioAsync(new UsuarioEditarDto
                    { Id = _editandoId, Nombre = Nombre.Trim(), NombreUsuario = NombreUsuario.Trim(),
                      NuevoPassword = string.IsNullOrWhiteSpace(Password) ? null : Password, PerfilId = PerfilSeleccionado.Id });
                    MensajeExito = "Usuario actualizado.";
                }
                else
                {
                    await _service.CrearUsuarioAsync(new UsuarioCrearDto
                    { Nombre = Nombre.Trim(), NombreUsuario = NombreUsuario.Trim(),
                      Password = Password, PerfilId = PerfilSeleccionado.Id });
                    MensajeExito = "Usuario creado.";
                }
                FormularioVisible = false;
                await CargarAsync();
            }
            catch (Exception ex) { MensajeError = $"Error: {ex.Message}"; }
        }

        private async Task EliminarAsync()
        {
            if (Seleccionado == null) return;
            await _service.DesactivarUsuarioAsync(Seleccionado.Id);
            await CargarAsync();
        }
    }
}
