using System;
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
            var boton = sender as Button;
            var panelBotones = boton?.Parent as FrameworkElement;

            try
            {
                if (panelBotones != null) panelBotones.Visibility = Visibility.Collapsed;

                var printDialog = new PrintDialog();
                if (printDialog.ShowDialog() == true)
                {
                    var contenido = ReciboContent;
                    var capabilities = printDialog.PrintQueue.GetPrintCapabilities(printDialog.PrintTicket);
                    var area = capabilities.PageImageableArea;

                    if (area != null)
                    {
                        double margenSeguridad = 0.95;
                        double anchoDisponible = area.ExtentWidth * margenSeguridad;

                        var scale = anchoDisponible / contenido.ActualWidth;
                        contenido.LayoutTransform = new ScaleTransform(scale, scale);

                        Size sz = new Size(area.ExtentWidth, double.PositiveInfinity);
                        contenido.Measure(sz);
                        contenido.Arrange(new Rect(new Point(area.OriginWidth, area.OriginHeight), contenido.DesiredSize));
                    }

                    printDialog.PrintVisual(contenido, "Recibo Venta - ArtesaniasPOS");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al imprimir: {ex.Message}", "Hardware Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (panelBotones != null) panelBotones.Visibility = Visibility.Visible;
                ReciboContent.LayoutTransform = Transform.Identity;
                ReciboContent.UpdateLayout();
            }
        }

        private void OnCerrar(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}