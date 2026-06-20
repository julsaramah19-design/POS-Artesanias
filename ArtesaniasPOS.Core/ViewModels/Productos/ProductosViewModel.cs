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
            // El código inicia vacío: el usuario escanea el código de barras
            // impreso del producto (o lo genera con el botón si no tiene uno).
            var categorias = await _productoService.ObtenerCategoriasAsync();

            FormularioVm = new ProductoFormularioViewModel(
                _productoService, categorias, null, null);

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
            Core.Notifier.Info("Producto desactivado");
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
            // Usa la API del spooler (respeta la predeterminada que administra
            // Windows). Es la misma fuente que usa el recibo vía PrinterSettings.
            int size = 0;
            GetDefaultPrinter(null, ref size); // 1ra llamada: obtener tamaño
            if (size > 0)
            {
                var sb = new System.Text.StringBuilder(size);
                if (GetDefaultPrinter(sb, ref size) && sb.Length > 0)
                    return sb.ToString();
            }

            // Respaldo: llave de registro legacy (puede estar vacía/obsoleta).
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

            // En modo etiqueta la impresora (JAL-838L / familia Xprinter) habla
            // TSPL, NO ESC/POS. Por eso antes "aceptaba" los bytes pero no
            // imprimía nada. Etiqueta de 50 x 30 mm.
            string nombre = (ProductoSeleccionado.Nombre ?? string.Empty).Replace("\"", "'");
            if (nombre.Length > 32) nombre = nombre.Substring(0, 32);
            string precio = "$" + ProductoSeleccionado.PrecioBase.ToString("N0");
            string codigo = (ProductoSeleccionado.CodigoBarras ?? string.Empty).Replace("\"", "'");

            const string nl = "\r\n";
            string tspl =
                "SIZE 50 mm,30 mm" + nl +
                "GAP 2 mm,0 mm" + nl +
                "DIRECTION 1" + nl +
                "CLS" + nl +
                "TEXT 16,15,\"4\",0,1,1,\"" + precio + "\"" + nl +     // precio grande arriba
                "BARCODE 16,60,\"39\",70,0,0,2,4,\"" + codigo + "\"" + nl + // CODE39 sin texto (HRI=0)
                "TEXT 16,150,\"3\",0,1,1,\"" + nombre + "\"" + nl +    // nombre legible debajo
                "PRINT 1,1" + nl;

            SendToPrinter(printerName, enc.GetBytes(tspl));
        }

        private void SendToPrinter(string printerName, byte[] data)
        {
            var di = new DOCINFOA { pDocName = "Etiqueta", pDataType = "RAW" };
            if (!OpenPrinter(printerName, out IntPtr hPrinter, IntPtr.Zero))
            {
                System.Windows.MessageBox.Show(
                    $"No se pudo abrir la impresora \"{printerName}\".");
                return;
            }
            if (!StartDocPrinter(hPrinter, 1, di))
            {
                ClosePrinter(hPrinter);
                System.Windows.MessageBox.Show("No se pudo iniciar el documento de impresión.");
                return;
            }
            if (!StartPagePrinter(hPrinter))
            {
                EndDocPrinter(hPrinter);
                ClosePrinter(hPrinter);
                System.Windows.MessageBox.Show("No se pudo iniciar la página de impresión.");
                return;
            }

            IntPtr pBytes = System.Runtime.InteropServices.Marshal.AllocHGlobal(data.Length);
            System.Runtime.InteropServices.Marshal.Copy(data, 0, pBytes, data.Length);
            bool escrito = WritePrinter(hPrinter, pBytes, data.Length, out int _);
            System.Runtime.InteropServices.Marshal.FreeHGlobal(pBytes);

            EndPagePrinter(hPrinter);
            EndDocPrinter(hPrinter);
            ClosePrinter(hPrinter);

            if (!escrito)
                System.Windows.MessageBox.Show("La impresora no aceptó los datos de la etiqueta.");
        }

        [System.Runtime.InteropServices.DllImport("winspool.Drv", EntryPoint = "GetDefaultPrinterW",
            SetLastError = true, CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
        private static extern bool GetDefaultPrinter(System.Text.StringBuilder? buffer, ref int size);
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