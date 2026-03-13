using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ArtesaniasPOS.Core.ViewModels.Wizard;

namespace ArtesaniasPOS.Views.Wizard
{
    public partial class CurrencyColorStepView : UserControl
    {
        public CurrencyColorStepView()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext is CurrencyColorStepViewModel vm)
            {
                ActualizarPreviewPrimario(vm.ColorPrimario);
                ActualizarPreviewSecundario(vm.ColorSecundario);
            }
        }

        private void OnColorPrimarioClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element &&
                element.DataContext is string colorHex &&
                DataContext is CurrencyColorStepViewModel vm)
            {
                vm.ColorPrimario = colorHex;
                ActualizarPreviewPrimario(colorHex);
            }
        }

        private void OnColorSecundarioClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element &&
                element.DataContext is string colorHex &&
                DataContext is CurrencyColorStepViewModel vm)
            {
                vm.ColorSecundario = colorHex;
                ActualizarPreviewSecundario(colorHex);
            }
        }

        private void ActualizarPreviewPrimario(string hex)
        {
            try
            {
                var color = (Color)ColorConverter.ConvertFromString(hex);
                PrimaryColorBrush.Color = color;
            }
            catch { }
        }

        private void ActualizarPreviewSecundario(string hex)
        {
            try
            {
                var color = (Color)ColorConverter.ConvertFromString(hex);
                SecondaryColorBrush.Color = color;
            }
            catch { }
        }
    }
}
