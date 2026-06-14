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
            // El sidebar es parte de la identidad de marca: usa siempre el
            // degradado espresso cálido para mantener la coherencia con la
            // paleta terracota/crema, sin depender de un color almacenado.
            var gradiente = new LinearGradientBrush
            {
                StartPoint = new System.Windows.Point(0, 0),
                EndPoint = new System.Windows.Point(0, 1)
            };
            gradiente.GradientStops.Add(new GradientStop(
                (Color)ColorConverter.ConvertFromString("#3a2c26"), 0));
            gradiente.GradientStops.Add(new GradientStop(
                (Color)ColorConverter.ConvertFromString("#2b211c"), 1));
            gradiente.Freeze();

            SidebarBorder.Background = gradiente;
        }
    }
}
