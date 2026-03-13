using System.Windows;
using System.Windows.Controls;
using ArtesaniasPOS.Core.ViewModels.Wizard;

namespace ArtesaniasPOS.Views.Wizard
{
    /// <summary>
    /// Code-behind para AdminUserStepView.
    /// 
    /// ¿Por qué code-behind para los passwords?
    /// WPF no permite binding directo al PasswordBox.Password por seguridad.
    /// La propiedad Password no es una DependencyProperty, así que no
    /// se puede enlazar con {Binding}. Usamos el evento PasswordChanged
    /// para pasar el valor al ViewModel manualmente.
    /// 
    /// Esto es una excepción aceptada en MVVM — no es lógica de negocio,
    /// es una limitación técnica de WPF que se maneja en la vista.
    /// </summary>
    public partial class AdminUserStepView : UserControl
    {
        public AdminUserStepView()
        {
            InitializeComponent();
        }

        private void OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is AdminUserStepViewModel vm)
            {
                vm.Password = PasswordField.Password;
                ActualizarIndicadorCoincidencia(vm);
            }
        }

        private void OnConfirmPasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is AdminUserStepViewModel vm)
            {
                vm.ConfirmarPassword = ConfirmPasswordField.Password;
                ActualizarIndicadorCoincidencia(vm);
            }
        }

        private void ActualizarIndicadorCoincidencia(AdminUserStepViewModel vm)
        {
            if (string.IsNullOrEmpty(ConfirmPasswordField.Password))
            {
                PasswordMatchIndicator.Visibility = Visibility.Collapsed;
                return;
            }

            PasswordMatchIndicator.Visibility = Visibility.Visible;

            if (vm.PasswordsCoinciden)
            {
                PasswordMatchIndicator.Text = "✅ Las contraseñas coinciden";
                PasswordMatchIndicator.Foreground = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#059669"));
            }
            else
            {
                PasswordMatchIndicator.Text = "❌ Las contraseñas no coinciden";
                PasswordMatchIndicator.Foreground = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#DC2626"));
            }
        }
    }
}
