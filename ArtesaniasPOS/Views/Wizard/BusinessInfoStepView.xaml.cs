using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using ArtesaniasPOS.Core.ViewModels.Wizard;
using Microsoft.Win32;

namespace ArtesaniasPOS.Views.Wizard
{
    /// <summary>
    /// Code-behind para BusinessInfoStepView.
    /// 
    /// El único código aquí es el OpenFileDialog para seleccionar el logo.
    /// En MVVM puro, los diálogos del SO se manejan desde la vista (code-behind)
    /// porque son componentes nativos de UI — no lógica de negocio.
    /// El resultado (la ruta del archivo) se pasa al ViewModel vía binding.
    /// </summary>
    public partial class BusinessInfoStepView : UserControl
    {
        public BusinessInfoStepView()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is BusinessInfoStepViewModel vm)
            {
                // Interceptar el comando de logo para abrir el diálogo
                LogoDropArea.InputBindings.Clear();
                LogoDropArea.MouseLeftButtonUp += (s, args) => SeleccionarLogo(vm);

                // Mostrar preview si ya hay logo
                ActualizarPreviewLogo(vm.LogoPath);
            }
        }

        private void SeleccionarLogo(BusinessInfoStepViewModel vm)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Seleccionar logo del negocio",
                Filter = "Imágenes|*.png;*.jpg;*.jpeg;*.bmp|Todos los archivos|*.*",
                CheckFileExists = true
            };

            if (dialog.ShowDialog() == true)
            {
                vm.LogoPath = dialog.FileName;
                ActualizarPreviewLogo(dialog.FileName);
            }
        }

        private void ActualizarPreviewLogo(string path)
        {
            if (!string.IsNullOrEmpty(path) && System.IO.File.Exists(path))
            {
                try
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(path, UriKind.Absolute);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();

                    LogoPreview.Source = bitmap;
                    LogoPreview.Visibility = Visibility.Visible;
                }
                catch
                {
                    LogoPreview.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                LogoPreview.Visibility = Visibility.Collapsed;
            }
        }
    }
}
