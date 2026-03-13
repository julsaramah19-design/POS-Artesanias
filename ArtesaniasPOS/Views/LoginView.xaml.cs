using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ArtesaniasPOS.Core.ViewModels;

namespace ArtesaniasPOS.UI.Views
{
    public partial class LoginView : UserControl
    {
        public LoginView()
        {
            InitializeComponent();
        }
        private void OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is LoginViewModel vm)
            {
                vm.Password = PasswordField.Password;
            }
        }
    }
}
