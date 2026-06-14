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
            ActualizarTabActivo(TabHistorialBtn);
        }

        private void OnTabProductosTop(object sender, RoutedEventArgs e)
        {
            GridHistorial.Visibility = Visibility.Collapsed;
            GridProductosTop.Visibility = Visibility.Visible;
            GridVendedores.Visibility = Visibility.Collapsed;
            ActualizarTabActivo(TabProductosBtn);
        }

        private void OnTabVendedor(object sender, RoutedEventArgs e)
        {
            GridHistorial.Visibility = Visibility.Collapsed;
            GridProductosTop.Visibility = Visibility.Collapsed;
            GridVendedores.Visibility = Visibility.Visible;
            ActualizarTabActivo(TabVendedorBtn);
        }

        /// <summary>Marca un tab como activo (Tag="active") y limpia los demás.</summary>
        private void ActualizarTabActivo(Button activo)
        {
            TabHistorialBtn.Tag = null;
            TabProductosBtn.Tag = null;
            TabVendedorBtn.Tag = null;
            activo.Tag = "active";
        }
    }
}
