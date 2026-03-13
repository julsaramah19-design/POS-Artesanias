using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Media;
using ArtesaniasPOS.Core.ViewModels;

namespace ArtesaniasPOS.UI.Views
{
    public partial class ShellView : UserControl
    {
        public ShellView()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender,
            System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is ShellViewModel oldVm)
                oldVm.PropertyChanged -= OnShellPropertyChanged;

            if (e.NewValue is ShellViewModel newVm)
            {
                newVm.PropertyChanged += OnShellPropertyChanged;
                AplicarColores(newVm.ColorPrimario, newVm.ColorSecundario);
            }
        }

        private void OnShellPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is ShellViewModel vm &&
                (e.PropertyName == nameof(vm.ColorPrimario) ||
                 e.PropertyName == nameof(vm.ColorSecundario)))
            {
                AplicarColores(vm.ColorPrimario, vm.ColorSecundario);
            }
        }

        private void AplicarColores(string primario, string secundario)
        {
            try
            {
                var colorPrimario = (Color)ColorConverter.ConvertFromString(primario);
                var colorSecundario = (Color)ColorConverter.ConvertFromString(secundario);

                SidebarGradientStart.Color = colorPrimario;
                SidebarGradientEnd.Color = colorSecundario;
            }
            catch
            {

            }
        }
    }
}
