using ArtesaniasPOS.Core.Entities;
using ArtesaniasPOS.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;

namespace ArtesaniasPOS.UI.Views.Ventas
{
    public partial class ReciboWindow : Window
    {
        public ReciboWindow()
        {
            InitializeComponent();
        }

        private void OnImprimir(object sender, RoutedEventArgs e)
        {
            if (!(this.DataContext is ReciboDto recibo))
            {
                MessageBox.Show("Error: El contexto de datos no es un ReciboDto válido.");
                return;
            }

            var settings = new PrinterSettings();
            string printerName = settings.PrinterName;

            if (string.IsNullOrEmpty(printerName))
            {
                MessageBox.Show("No se encontró impresora.");
                return;
            }

            var sb = new System.Text.StringBuilder();
            sb.Append("\x1B\x40");

            sb.Append("\x1B\x61\x01");      
            sb.Append("\x1B\x21\x30");      
            sb.AppendLine(recibo.NombreNegocio);
            sb.Append("\x1B\x21\x00");
            sb.AppendLine(new string('=', 32));

            // Cabecera: alinear izquierda
            sb.Append("\x1B\x61\x00");
            sb.AppendLine(Col("Venta #:", recibo.VentaId.ToString()));
            sb.AppendLine(Col("Fecha:", recibo.FechaFormateada));
            sb.AppendLine(Col("Vendedor:", recibo.Vendedor));
            sb.AppendLine(Col("Pago:", recibo.MedioPago));
            sb.AppendLine(new string('-', 32));

            // Productos
            foreach (var item in recibo.Items)
            {
                sb.AppendLine($"{item.Cantidad} x {item.Nombre}");
                sb.AppendLine(Col("", "$" + item.Subtotal.ToString("N0")));
                if (item.TieneDescuento)
                    sb.AppendLine(Col("  Desc:", "-$" + item.Descuento.ToString("N0")));
            }

            sb.AppendLine(new string('-', 32));

            // Totales
            sb.AppendLine(Col("Subtotal:", "$" + recibo.Subtotal.ToString("N0")));

            sb.Append("\x1B\x21\x08");      // negrita
            sb.AppendLine(Col("TOTAL:", "$" + recibo.Total.ToString("N0")));
            sb.Append("\x1B\x21\x00");      // normal

            sb.AppendLine(new string('-', 32));
            sb.AppendLine(Col("Pago:", "$" + recibo.MontoPagado.ToString("N0")));
            sb.AppendLine(Col("Cambio:", "$" + recibo.Cambio.ToString("N0")));
            sb.AppendLine(new string('=', 32));

            sb.Append("\x1B\x21\x08");   // negrita ON
            sb.Append("\x1B\x61\x01");   // centrar
            sb.AppendLine("** Gracias por su compra! **");
            sb.Append("\x1B\x21\x00");   // negrita OFF
            sb.Append("\x1B\x64\x04");
            sb.Append("\x1D\x56\x41\x00");

            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            byte[] data = System.Text.Encoding.GetEncoding("ibm850").GetBytes(sb.ToString());
            SendToPrinter(printerName, data);
        }

        private string Col(string left, string right, int width = 32)
        {
            int spaces = width - left.Length - right.Length;
            return left + new string(' ', Math.Max(1, spaces)) + right;
        }

        private void SendToPrinter(string printerName, byte[] data)
        {
            var di = new DOCINFOA { pDocName = "Recibo", pDataType = "RAW" };
            IntPtr hPrinter = IntPtr.Zero;

            if (!OpenPrinter(printerName, out hPrinter, IntPtr.Zero)) return;
            if (!StartDocPrinter(hPrinter, 1, di)) { ClosePrinter(hPrinter); return; }
            if (!StartPagePrinter(hPrinter)) { EndDocPrinter(hPrinter); ClosePrinter(hPrinter); return; }

            IntPtr pBytes = Marshal.AllocHGlobal(data.Length);
            Marshal.Copy(data, 0, pBytes, data.Length);
            WritePrinter(hPrinter, pBytes, data.Length, out int _);
            Marshal.FreeHGlobal(pBytes);

            EndPagePrinter(hPrinter);
            EndDocPrinter(hPrinter);
            ClosePrinter(hPrinter);
        }

        private void OnCerrar(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        [DllImport("winspool.Drv", EntryPoint = "OpenPrinterA")]
        private static extern bool OpenPrinter(string pPrinterName, out IntPtr phPrinter, IntPtr pDefault);

        [DllImport("winspool.Drv", EntryPoint = "ClosePrinter")]
        private static extern bool ClosePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "StartDocPrinterA")]
        private static extern bool StartDocPrinter(IntPtr hPrinter, int level, [In] DOCINFOA di);

        [DllImport("winspool.Drv", EntryPoint = "EndDocPrinter")]
        private static extern bool EndDocPrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "StartPagePrinter")]
        private static extern bool StartPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "EndPagePrinter")]
        private static extern bool EndPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "WritePrinter")]
        private static extern bool WritePrinter(IntPtr hPrinter, IntPtr pBytes, int dwCount, out int dwWritten);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        private class DOCINFOA
        {
            [MarshalAs(UnmanagedType.LPStr)] public string pDocName;
            [MarshalAs(UnmanagedType.LPStr)] public string pOutputFile = null;
            [MarshalAs(UnmanagedType.LPStr)] public string pDataType;
        }
    }
}