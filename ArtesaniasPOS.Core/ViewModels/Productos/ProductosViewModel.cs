using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using ArtesaniasPOS.Core.Interfaces;

namespace ArtesaniasPOS.Core.ViewModels.Productos
{
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
            ImprimirEtiquetaCommand = new RelayCommand(_ => ImprimirEtiqueta(), _ => ProductoSeleccionado != null);
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

        public bool PanelVisible
        {
            get => _panelVisible;
            set => SetProperty(ref _panelVisible, value);
        }

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

        public bool PuedeEditar => _esAdmin;

        #endregion

        #region Comandos

        public ICommand BuscarCommand { get; }
        public ICommand NuevoCommand { get; }
        public ICommand EditarCommand { get; }
        public ICommand EliminarCommand { get; }
        public ICommand CerrarPanelCommand { get; }
        public ICommand ImprimirEtiquetaCommand { get; }

        #endregion

        public async Task InicializarAsync()
        {
            IsLoading = true;
            try
            {
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

        private async Task AbrirNuevoAsync()
        {
            var codigo = await _productoService.GenerarCodigoBarrasAsync();
            var categorias = await _productoService.ObtenerCategoriasAsync();

            FormularioVm = new ProductoFormularioViewModel(
                _productoService, categorias, null, codigo);

            FormularioVm.Guardado += async (s, e) => { CerrarPanel(); await CargarProductosAsync(); };
            FormularioVm.Cancelado += (s, e) => CerrarPanel();

            PanelVisible = true;
        }

        private async Task AbrirEditarAsync()
        {
            if (ProductoSeleccionado == null) return;

            var producto = await _productoService.ObtenerPorIdAsync(ProductoSeleccionado.Id);
            if (producto == null) return;

            var categorias = await _productoService.ObtenerCategoriasAsync();

            FormularioVm = new ProductoFormularioViewModel(
                _productoService, categorias, producto, null);

            FormularioVm.Guardado += async (s, e) => { CerrarPanel(); await CargarProductosAsync(); };
            FormularioVm.Cancelado += (s, e) => CerrarPanel();

            PanelVisible = true;
        }

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

        #region Impresión de etiquetas

        private string GetDefaultPrinterName()
        {
            using var key = Microsoft.Win32.Registry.CurrentUser
                .OpenSubKey(@"Software\Microsoft\Windows NT\CurrentVersion\Windows");
            return key?.GetValue("Device")?.ToString()?.Split(',')[0] ?? string.Empty;
        }

        private void ImprimirEtiqueta()
        {
            if (ProductoSeleccionado == null) return;

            string printerName = GetDefaultPrinterName();
            if (string.IsNullOrEmpty(printerName))
            {
                System.Windows.MessageBox.Show("No se encontró impresora.");
                return;
            }

            System.Text.Encoding.RegisterProvider(
                System.Text.CodePagesEncodingProvider.Instance);
            var enc = System.Text.Encoding.GetEncoding("ibm850");

            var bytes = new List<byte>();

            // Reset + centrar
            bytes.AddRange(enc.GetBytes("\x1B\x40\x1B\x61\x00"));

            // Precio en doble alto + negrita
            bytes.AddRange(enc.GetBytes("\x1B\x21\x18"));
            bytes.AddRange(enc.GetBytes("$" + ProductoSeleccionado.PrecioBase.ToString("N0") + "\n"));
            bytes.AddRange(enc.GetBytes("\x1B\x21\x00"));

            // Código de barras
            bytes.AddRange(new byte[] { 0x1D, 0x68, 0x40 }); // altura 64 puntos
            bytes.AddRange(new byte[] { 0x1D, 0x77, 0x02 }); // ancho normal
            bytes.AddRange(new byte[] { 0x1D, 0x48, 0x02 }); // HRI abajo

            byte[] codigoBytes = System.Text.Encoding.ASCII
                .GetBytes(ProductoSeleccionado.CodigoBarras);
            bytes.AddRange(new byte[] { 0x1D, 0x6B, 0x04 });
            bytes.AddRange(codigoBytes);
            bytes.Add(0x00);

            // Corte mínimo
            bytes.AddRange(new byte[] { 0x1B, 0x64, 0x01 });
            bytes.AddRange(new byte[] { 0x1D, 0x56, 0x41, 0x00 });

            SendToPrinter(printerName, bytes.ToArray());
        }

        private void SendToPrinter(string printerName, byte[] data)
        {
            var di = new DOCINFOA { pDocName = "Etiqueta", pDataType = "RAW" };
            if (!OpenPrinter(printerName, out IntPtr hPrinter, IntPtr.Zero)) return;
            if (!StartDocPrinter(hPrinter, 1, di)) { ClosePrinter(hPrinter); return; }
            if (!StartPagePrinter(hPrinter)) { EndDocPrinter(hPrinter); ClosePrinter(hPrinter); return; }

            IntPtr pBytes = System.Runtime.InteropServices.Marshal.AllocHGlobal(data.Length);
            System.Runtime.InteropServices.Marshal.Copy(data, 0, pBytes, data.Length);
            WritePrinter(hPrinter, pBytes, data.Length, out int _);
            System.Runtime.InteropServices.Marshal.FreeHGlobal(pBytes);

            EndPagePrinter(hPrinter);
            EndDocPrinter(hPrinter);
            ClosePrinter(hPrinter);
        }

        [System.Runtime.InteropServices.DllImport("winspool.Drv", EntryPoint = "OpenPrinterA")]
        private static extern bool OpenPrinter(string n, out IntPtr h, IntPtr d);
        [System.Runtime.InteropServices.DllImport("winspool.Drv", EntryPoint = "ClosePrinter")]
        private static extern bool ClosePrinter(IntPtr h);
        [System.Runtime.InteropServices.DllImport("winspool.Drv", EntryPoint = "StartDocPrinterA")]
        private static extern bool StartDocPrinter(IntPtr h, int l, [System.Runtime.InteropServices.In] DOCINFOA di);
        [System.Runtime.InteropServices.DllImport("winspool.Drv", EntryPoint = "EndDocPrinter")]
        private static extern bool EndDocPrinter(IntPtr h);
        [System.Runtime.InteropServices.DllImport("winspool.Drv", EntryPoint = "StartPagePrinter")]
        private static extern bool StartPagePrinter(IntPtr h);
        [System.Runtime.InteropServices.DllImport("winspool.Drv", EntryPoint = "EndPagePrinter")]
        private static extern bool EndPagePrinter(IntPtr h);
        [System.Runtime.InteropServices.DllImport("winspool.Drv", EntryPoint = "WritePrinter")]
        private static extern bool WritePrinter(IntPtr h, IntPtr p, int c, out int w);

        [System.Runtime.InteropServices.StructLayout(
            System.Runtime.InteropServices.LayoutKind.Sequential,
            CharSet = System.Runtime.InteropServices.CharSet.Ansi)]
        private class DOCINFOA
        {
            [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPStr)]
            public string pDocName;
            [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPStr)]
            public string pOutputFile = null;
            [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPStr)]
            public string pDataType;
        }

        #endregion
    }
}