using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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
            var printDialog = new PrintDialog();
            if (printDialog.ShowDialog() == true)
            {
                // Escalar el contenido para formato ticket
                var contenido = ReciboContent;
                var capabilities = printDialog.PrintQueue.GetPrintCapabilities(printDialog.PrintTicket);
                var area = capabilities.PageImageableArea;

                if (area != null)
                {
                    var scale = area.ExtentWidth / contenido.ActualWidth;
                    contenido.LayoutTransform = new ScaleTransform(scale, scale);
                    var sz = new Size(area.ExtentWidth, double.PositiveInfinity);
                    contenido.Measure(sz);
                    contenido.Arrange(new Rect(new Point(area.OriginWidth, area.OriginHeight),
                        contenido.DesiredSize));
                }

                printDialog.PrintVisual(contenido, $"Recibo Venta");

                // Restaurar escala
                contenido.LayoutTransform = Transform.Identity;
                contenido.InvalidateMeasure();
                contenido.UpdateLayout();
            }
        }

        private void OnCerrar(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}