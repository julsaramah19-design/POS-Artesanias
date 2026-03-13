using System.Collections.ObjectModel;
using System.Windows.Input;
using ArtesaniasPOS.Core.Interfaces;

namespace ArtesaniasPOS.Core.ViewModels.Productos
{
    /// <summary>
    /// ViewModel del formulario de crear/editar producto.
    /// Se muestra en el panel lateral derecho.
    /// 
    /// Si ProductoId == 0 → modo creación.
    /// Si ProductoId > 0 → modo edición.
    /// </summary>
    public class ProductoFormularioViewModel : ViewModelBase
    {
        private readonly IProductoService _productoService;

        private int _productoId;
        private string _codigoBarras = string.Empty;
        private string _nombre = string.Empty;
        private string _descripcion = string.Empty;
        private double _precioBase;
        private int _stockActual;
        private int _stockMinimo = 5;
        private CategoriaDto? _categoriaSeleccionada;
        private string _mensajeError = string.Empty;
        private bool _isLoading;

        public ProductoFormularioViewModel(
            IProductoService productoService,
            IEnumerable<CategoriaDto> categorias,
            ProductoDetalleDto? productoExistente,
            string? codigoBarrasNuevo)
        {
            _productoService = productoService;

            // Cargar categorías
            foreach (var cat in categorias)
                Categorias.Add(cat);

            // Si hay producto existente → modo edición
            if (productoExistente != null)
            {
                _productoId = productoExistente.Id;
                _codigoBarras = productoExistente.CodigoBarras;
                _nombre = productoExistente.Nombre;
                _descripcion = productoExistente.Descripcion ?? string.Empty;
                _precioBase = productoExistente.PrecioBase;
                _stockActual = productoExistente.StockActual;
                _stockMinimo = productoExistente.StockMinimo;
                _categoriaSeleccionada = Categorias.FirstOrDefault(
                    c => c.Id == productoExistente.CategoriaId);
            }
            else
            {
                // Modo creación
                _codigoBarras = codigoBarrasNuevo ?? string.Empty;
            }

            GuardarCommand = new AsyncRelayCommand(
                async _ => await GuardarAsync(),
                _ => !IsLoading);

            CancelarCommand = new RelayCommand(_ => Cancelado?.Invoke(this, EventArgs.Empty));
        }

        #region Propiedades

        public bool EsEdicion => _productoId > 0;
        public string TituloFormulario => EsEdicion ? "Editar producto" : "Nuevo producto";

        public ObservableCollection<CategoriaDto> Categorias { get; } = new();

        public CategoriaDto? CategoriaSeleccionada
        {
            get => _categoriaSeleccionada;
            set => SetProperty(ref _categoriaSeleccionada, value);
        }

        public string CodigoBarras
        {
            get => _codigoBarras;
            set => SetProperty(ref _codigoBarras, value);
        }

        public string Nombre
        {
            get => _nombre;
            set => SetProperty(ref _nombre, value);
        }

        public string Descripcion
        {
            get => _descripcion;
            set => SetProperty(ref _descripcion, value);
        }

        public double PrecioBase
        {
            get => _precioBase;
            set => SetProperty(ref _precioBase, value);
        }

        public int StockActual
        {
            get => _stockActual;
            set => SetProperty(ref _stockActual, value);
        }

        public int StockMinimo
        {
            get => _stockMinimo;
            set => SetProperty(ref _stockMinimo, value);
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

        #endregion

        #region Comandos y Eventos

        public ICommand GuardarCommand { get; }
        public ICommand CancelarCommand { get; }

        /// <summary>
        /// Se dispara cuando el producto se guardó correctamente.
        /// ProductosViewModel escucha esto para cerrar el panel y refrescar.
        /// </summary>
        public event EventHandler? Guardado;
        public event EventHandler? Cancelado;

        #endregion

        private async Task GuardarAsync()
        {
            MensajeError = string.Empty;

            // Validaciones
            if (CategoriaSeleccionada == null)
            {
                MensajeError = "Selecciona una categoría.";
                return;
            }

            if (string.IsNullOrWhiteSpace(CodigoBarras))
            {
                MensajeError = "El código de barras es obligatorio.";
                return;
            }

            if (string.IsNullOrWhiteSpace(Nombre))
            {
                MensajeError = "El nombre es obligatorio.";
                return;
            }

            if (PrecioBase <= 0)
            {
                MensajeError = "El precio debe ser mayor a 0.";
                return;
            }

            if (StockActual < 0)
            {
                MensajeError = "El stock no puede ser negativo.";
                return;
            }

            IsLoading = true;
            try
            {
                var dto = new ProductoGuardarDto
                {
                    Id = _productoId,
                    CategoriaId = CategoriaSeleccionada.Id,
                    CodigoBarras = CodigoBarras.Trim(),
                    Nombre = Nombre.Trim(),
                    Descripcion = string.IsNullOrWhiteSpace(Descripcion) ? null : Descripcion.Trim(),
                    PrecioBase = PrecioBase,
                    StockActual = StockActual,
                    StockMinimo = StockMinimo
                };

                if (EsEdicion)
                    await _productoService.ActualizarAsync(dto);
                else
                    await _productoService.CrearAsync(dto);

                Guardado?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                MensajeError = $"Error al guardar: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
