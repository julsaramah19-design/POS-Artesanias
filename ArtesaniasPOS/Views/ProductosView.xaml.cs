using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ArtesaniasPOS.Core.ViewModels.Productos;

namespace ArtesaniasPOS.UI.Views.Productos
{
    public partial class ProductosView : UserControl
    {
        public ProductosView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Al abrir el formulario para un producto nuevo, deja el foco en el
        /// campo de código de barras y lo selecciona, de modo que el lector
        /// (que actúa como teclado) escriba el código directamente ahí.
        /// </summary>
        private void FormPanel_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is not true) return;
            if (CodigoBarrasTextBox.DataContext is not ProductoFormularioViewModel vm) return;
            if (vm.EsEdicion) return;

            // Esperar a que el panel termine de mostrarse antes de enfocar.
            Dispatcher.BeginInvoke(new System.Action(() =>
            {
                CodigoBarrasTextBox.Focus();
                CodigoBarrasTextBox.SelectAll();
            }), System.Windows.Threading.DispatcherPriority.Input);
        }

        /// <summary>
        /// Los lectores de código de barras envían un Enter al final de la
        /// lectura. Lo capturamos para saltar al siguiente campo en vez de
        /// dejar el cursor en el código.
        /// </summary>
        private void CodigoBarrasTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.Enter or Key.Return)
            {
                e.Handled = true;
                ((TextBox)sender).MoveFocus(
                    new TraversalRequest(FocusNavigationDirection.Next));
            }
        }
    }
}
