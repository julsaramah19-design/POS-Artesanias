using System.Windows;
using System.Windows.Controls;

namespace ArtesaniasPOS.UI.Views.Reportes
{
    public partial class ReportesView : UserControl
    {
        public ReportesView()
        {
            InitializeComponent();
        }

        private void OnTabHistorial(object sender, RoutedEventArgs e)
        {
            GridHistorial.Visibility = Visibility.Visible;
            GridProductosTop.Visibility = Visibility.Collapsed;
            GridVendedores.Visibility = Visibility.Collapsed;
        }

        private void OnTabProductosTop(object sender, RoutedEventArgs e)
        {
            GridHistorial.Visibility = Visibility.Collapsed;
            GridProductosTop.Visibility = Visibility.Visible;
            GridVendedores.Visibility = Visibility.Collapsed;
        }

        private void OnTabVendedor(object sender, RoutedEventArgs e)
        {
            GridHistorial.Visibility = Visibility.Collapsed;
            GridProductosTop.Visibility = Visibility.Collapsed;
            GridVendedores.Visibility = Visibility.Visible;
        }
    }
}
