using System.Collections.ObjectModel;
using System.Windows.Input;
using ArtesaniasPOS.Core.Interfaces;

namespace ArtesaniasPOS.Core.ViewModels.Productos
{
    /// <summary>
    /// ViewModel principal del módulo de Productos.
    /// 
    /// Maneja:
    /// - Listado con búsqueda y filtro por categoría
    /// - Apertura del panel lateral para crear/editar
    /// - Desactivación de productos
    /// - Refresco del DataGrid después de operaciones
    /// 
    /// El panel lateral es controlado por PanelVisible y FormularioVm.
    /// Cuando el usuario hace click en "Nuevo" o "Editar", se crea
    /// un ProductoFormularioViewModel y se muestra el panel.
    /// </summary>
    public class ProductosViewModel : ViewModelBase
    {
        private readonly IProductoService _productoService;
        private readonly bool _esAdmin;

        private string _textoBusqueda = string.Empty;
        private CategoriaDto? _categoriaFiltro;
        private ProductoListaDto? _productoSeleccionado;
        private bool _panelVisible;
        private ProductoFormularioViewModel? _formularioVm;
        private bool _isLoading;
        private string _mensajeError = string.Empty;

        public ProductosViewModel(IProductoService productoService, bool esAdmin)
        {
            _productoService = productoService;
            _esAdmin = esAdmin;

            BuscarCommand = new AsyncRelayCommand(async _ => await CargarProductosAsync());
            NuevoCommand = new AsyncRelayCommand(async _ => await AbrirNuevoAsync());
            EditarCommand = new AsyncRelayCommand(async _ => await AbrirEditarAsync(), _ => ProductoSeleccionado != null);
            EliminarCommand = new AsyncRelayCommand(async _ => await DesactivarAsync(), _ => ProductoSeleccionado != null);
            CerrarPanelCommand = new RelayCommand(_ => CerrarPanel());
        }

        #region Propiedades

        public ObservableCollection<ProductoListaDto> Productos { get; } = new();
        public ObservableCollection<CategoriaDto> Categorias { get; } = new();

        public string TextoBusqueda
        {
            get => _textoBusqueda;
            set => SetProperty(ref _textoBusqueda, value);
        }

        public CategoriaDto? CategoriaFiltro
        {
            get => _categoriaFiltro;
            set
            {
                if (SetProperty(ref _categoriaFiltro, value))
                    _ = CargarProductosAsync();
            }
        }

        public ProductoListaDto? ProductoSeleccionado
        {
            get => _productoSeleccionado;
            set => SetProperty(ref _productoSeleccionado, value);
        }

        /// <summary>
        /// Controla la visibilidad del panel lateral de formulario.
        /// </summary>
        public bool PanelVisible
        {
            get => _panelVisible;
            set => SetProperty(ref _panelVisible, value);
        }

        /// <summary>
        /// ViewModel del formulario que se muestra en el panel lateral.
        /// </summary>
        public ProductoFormularioViewModel? FormularioVm
        {
            get => _formularioVm;
            set => SetProperty(ref _formularioVm, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public string MensajeError
        {
            get => _mensajeError;
            set => SetProperty(ref _mensajeError, value);
        }

        /// <summary>
        /// El vendedor no puede crear, editar ni eliminar.
        /// Solo ve el listado y la búsqueda.
        /// </summary>
        public bool PuedeEditar => _esAdmin;

        #endregion

        #region Comandos

        public ICommand BuscarCommand { get; }
        public ICommand NuevoCommand { get; }
        public ICommand EditarCommand { get; }
        public ICommand EliminarCommand { get; }
        public ICommand CerrarPanelCommand { get; }

        #endregion

        /// <summary>
        /// Carga inicial: categorías para el filtro + listado de productos.
        /// Se llama cuando el usuario navega a este módulo.
        /// </summary>
        public async Task InicializarAsync()
        {
            IsLoading = true;
            try
            {
                // Cargar categorías para el filtro
                var categorias = await _productoService.ObtenerCategoriasAsync();
                Categorias.Clear();
                Categorias.Add(new CategoriaDto { Id = 0, Nombre = "Todas las categorías" });
                foreach (var cat in categorias)
                    Categorias.Add(cat);

                CategoriaFiltro = Categorias[0];

                await CargarProductosAsync();
            }
            catch (Exception ex)
            {
                MensajeError = $"Error al cargar: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Recarga el listado con los filtros actuales.
        /// </summary>
        private async Task CargarProductosAsync()
        {
            IsLoading = true;
            MensajeError = string.Empty;

            try
            {
                var categoriaId = CategoriaFiltro?.Id > 0 ? CategoriaFiltro.Id : (int?)null;
                var busqueda = string.IsNullOrWhiteSpace(TextoBusqueda) ? null : TextoBusqueda;

                var productos = await _productoService.ObtenerTodosAsync(busqueda, categoriaId);

                Productos.Clear();
                foreach (var p in productos)
                    Productos.Add(p);
            }
            catch (Exception ex)
            {
                MensajeError = $"Error al buscar: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Abre el panel lateral para crear un producto nuevo.
        /// Genera un código de barras automáticamente.
        /// </summary>
        private async Task AbrirNuevoAsync()
        {
            var codigo = await _productoService.GenerarCodigoBarrasAsync();
            var categorias = await _productoService.ObtenerCategoriasAsync();

            FormularioVm = new ProductoFormularioViewModel(
                _productoService, categorias, null, codigo);

            FormularioVm.Guardado += async (s, e) =>
            {
                CerrarPanel();
                await CargarProductosAsync();
            };

            FormularioVm.Cancelado += (s, e) => CerrarPanel();

            PanelVisible = true;
        }

        /// <summary>
        /// Abre el panel lateral para editar el producto seleccionado.
        /// </summary>
        private async Task AbrirEditarAsync()
        {
            if (ProductoSeleccionado == null) return;

            var producto = await _productoService.ObtenerPorIdAsync(ProductoSeleccionado.Id);
            if (producto == null) return;

            var categorias = await _productoService.ObtenerCategoriasAsync();

            FormularioVm = new ProductoFormularioViewModel(
                _productoService, categorias, producto, null);

            FormularioVm.Guardado += async (s, e) =>
            {
                CerrarPanel();
                await CargarProductosAsync();
            };

            FormularioVm.Cancelado += (s, e) => CerrarPanel();

            PanelVisible = true;
        }

        /// <summary>
        /// Desactiva el producto seleccionado (soft delete).
        /// </summary>
        private async Task DesactivarAsync()
        {
            if (ProductoSeleccionado == null) return;

            await _productoService.DesactivarAsync(ProductoSeleccionado.Id);
            await CargarProductosAsync();
        }

        private void CerrarPanel()
        {
            PanelVisible = false;
            FormularioVm = null;
        }
    }
}
