using System.Collections.ObjectModel;
using System.Windows.Input;
using ArtesaniasPOS.Core.Interfaces;
using ArtesaniasPOS.Core.Models;

namespace ArtesaniasPOS.Core.ViewModels
{
    public class ShellViewModel : ViewModelBase
    {
        private readonly SesionUsuario _sesion;
        private readonly IConfiguracionService _configuracionService;

        private MenuItemModel? _menuSeleccionado;
        private ViewModelBase? _contenidoActual;
        private string _colorPrimario = "#3B82F6";
        private string _colorSecundario = "#1E40AF";
        private string _nombreNegocio = string.Empty;
        private readonly IProductoService _productoService;

        public ShellViewModel(
            SesionUsuario sesion,
            IConfiguracionService configuracionService,
            IProductoService productoService) 
        {
            _sesion = sesion;
            _configuracionService = configuracionService;
            _productoService = productoService;

            CerrarSesionCommand = new RelayCommand(_ => CerrarSesion());
            ConstruirMenu();
        }

        #region Propiedades

        public string NombreUsuario => _sesion.Nombre;
        public string PerfilNombre => _sesion.PerfilNombre;
        public string InicialUsuario => _sesion.Nombre.Length > 0
            ? _sesion.Nombre[0].ToString().ToUpper()
            : "?";

        public string NombreNegocio
        {
            get => _nombreNegocio;
            set => SetProperty(ref _nombreNegocio, value);
        }

        public string ColorPrimario
        {
            get => _colorPrimario;
            set => SetProperty(ref _colorPrimario, value);
        }

        public string ColorSecundario
        {
            get => _colorSecundario;
            set => SetProperty(ref _colorSecundario, value);
        }

        /// <summary>
        /// Items del menú lateral, filtrados por el perfil del usuario.
        /// </summary>
        public ObservableCollection<MenuItemModel> MenuItems { get; } = new();

        public MenuItemModel? MenuSeleccionado
        {
            get => _menuSeleccionado;
            set
            {
                if (SetProperty(ref _menuSeleccionado, value) && value != null)
                    NavegarAModulo(value);
            }
        }

        /// <summary>
        /// El ViewModel del módulo que se muestra en el área central.
        /// ContentControl en la vista se bindea a esto.
        /// </summary>
        public ViewModelBase? ContenidoActual
        {
            get => _contenidoActual;
            set => SetProperty(ref _contenidoActual, value);
        }

        #endregion

        public ICommand CerrarSesionCommand { get; }


        public event EventHandler? SesionCerrada;

        public async Task CargarConfiguracionAsync()
        {
            var config = await _configuracionService.ObtenerTodasAsync();

            NombreNegocio = config.GetValueOrDefault("NombreNegocio", "ArtesaniasPOS");
            ColorPrimario = config.GetValueOrDefault("ColorPrimario", "#3B82F6");
            ColorSecundario = config.GetValueOrDefault("ColorSecundario", "#1E40AF");


            if (MenuItems.Count > 0)
                MenuSeleccionado = MenuItems[0];
        }

        private void ConstruirMenu()
        {
            var todosLosModulos = new List<MenuItemModel>
            {
                new()
                {
                    Titulo = "Inicio",
                    Icono = "🏠",
                    Modulo = "Dashboard",
                    PerfilesPermitidos = new List<int> { 1, 2 }
                },
                new()
                {
                    Titulo = "Productos",
                    Icono = "📦",
                    Modulo = "Productos",
                    PerfilesPermitidos = new List<int> { 1, 2 }
                },
                new()
                {
                    Titulo = "Ventas",
                    Icono = "💰",
                    Modulo = "Ventas",
                    PerfilesPermitidos = new List<int> { 1, 2 }
                },
                new()
                {
                    Titulo = "Inventario",
                    Icono = "📋",
                    Modulo = "Inventario",
                    PerfilesPermitidos = new List<int> { 1 }
                },
                new()
                {
                    Titulo = "Reportes",
                    Icono = "📊",
                    Modulo = "Reportes",
                    PerfilesPermitidos = new List<int> { 1 }
                },
                new()
                {
                    Titulo = "Configuración",
                    Icono = "⚙️",
                    Modulo = "Configuracion",
                    PerfilesPermitidos = new List<int> { 1 }
                }
            };

            MenuItems.Clear();
            foreach (var modulo in todosLosModulos)
            {
                if (modulo.PerfilesPermitidos.Contains(_sesion.PerfilId))
                    MenuItems.Add(modulo);
            }
        }


        private async void NavegarAModulo(MenuItemModel menuItem)
        {
            switch (menuItem.Modulo)
            {
                case "Productos":
                    var productosVm = new ViewModels.Productos.ProductosViewModel(
                        _productoService, _sesion.EsAdmin);
                    ContenidoActual = productosVm;
                    await productosVm.InicializarAsync();
                    break;

                default:
                    ContenidoActual = new PlaceholderViewModel(menuItem.Titulo, menuItem.Icono);
                    break;
            }
        }

        private void CerrarSesion()
        {
            SesionCerrada?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// ViewModel temporal para módulos que aún no están implementados.
    /// Muestra el nombre del módulo como placeholder.
    /// </summary>
    public class PlaceholderViewModel : ViewModelBase
    {
        public string Titulo { get; }
        public string Icono { get; }
        public string Mensaje { get; }

        public PlaceholderViewModel(string titulo, string icono)
        {
            Titulo = titulo;
            Icono = icono;
            Mensaje = $"Módulo de {titulo} — próximamente";
        }
    }
}
