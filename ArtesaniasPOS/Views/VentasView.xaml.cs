using System.Windows.Controls;
using ArtesaniasPOS.Core.ViewModels.Ventas;
using ArtesaniasPOS.Core.Interfaces;

namespace ArtesaniasPOS.UI.Views.Ventas
{
    public partial class VentasView : UserControl
    {
        public VentasView()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is VentasViewModel oldVm)
                oldVm.ReciboGenerado -= OnReciboGenerado;

            if (e.NewValue is VentasViewModel newVm)
                newVm.ReciboGenerado += OnReciboGenerado;
        }

        private void OnReciboGenerado(object? sender, ReciboDto recibo)
        {
            var ventana = new ReciboWindow { DataContext = recibo };
            ventana.ShowDialog();
        }
    }
}