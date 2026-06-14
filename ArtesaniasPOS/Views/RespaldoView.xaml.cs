using System.Windows;
using System.Windows.Controls;
using ArtesaniasPOS.Core.ViewModels.Configuracion;
using Microsoft.Win32;

namespace ArtesaniasPOS.UI.Views.Configuracion
{
    public partial class RespaldoView : UserControl
    {
        public RespaldoView()
        {
            InitializeComponent();
        }

        private RespaldoViewModel? Vm => DataContext as RespaldoViewModel;

        private void BtnRespaldar_Click(object sender, RoutedEventArgs e)
        {
            if (Vm is null) return;

            var dlg = new SaveFileDialog
            {
                Title = "Guardar copia de seguridad",
                FileName = Vm.NombreSugerido,
                Filter = "Base de datos (*.db)|*.db|Todos los archivos (*.*)|*.*",
                DefaultExt = ".db"
            };

            if (dlg.ShowDialog() == true)
                Vm.CrearRespaldo(dlg.FileName);
        }

        private void BtnRestaurar_Click(object sender, RoutedEventArgs e)
        {
            if (Vm is null) return;

            var confirmar = MessageBox.Show(
                "Esto reemplazará TODOS los datos actuales (productos, ventas, usuarios) " +
                "con los de la copia seleccionada.\n\n" +
                "Se guardará una copia de los datos actuales como respaldo (.bak).\n\n" +
                "¿Deseas continuar?",
                "Restaurar copia de seguridad",
                MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (confirmar != MessageBoxResult.Yes) return;

            var dlg = new OpenFileDialog
            {
                Title = "Selecciona la copia de seguridad",
                Filter = "Base de datos (*.db)|*.db|Todos los archivos (*.*)|*.*",
                CheckFileExists = true
            };

            if (dlg.ShowDialog() != true) return;

            if (Vm.Restaurar(dlg.FileName))
                OfrecerReinicio("Datos restaurados correctamente.");
        }

        private void BtnCambiarUbicacion_Click(object sender, RoutedEventArgs e)
        {
            if (Vm is null) return;

            var dlg = new OpenFolderDialog
            {
                Title = "Elige la carpeta donde se guardará la base de datos"
            };

            if (dlg.ShowDialog() != true) return;

            if (Vm.CambiarUbicacion(dlg.FolderName))
                OfrecerReinicio("La base de datos se movió a la nueva ubicación.");
        }

        private void BtnCambiarCarpetaRespaldos_Click(object sender, RoutedEventArgs e)
        {
            if (Vm is null) return;

            var dlg = new OpenFolderDialog
            {
                Title = "Elige la carpeta para los respaldos automáticos"
            };

            if (dlg.ShowDialog() == true)
                Vm.CambiarCarpetaRespaldos(dlg.FolderName);
        }

        private void OfrecerReinicio(string mensaje)
        {
            var reiniciar = MessageBox.Show(
                mensaje + "\n\nLa aplicación debe reiniciarse para aplicar los cambios. ¿Cerrar ahora?",
                "Reinicio necesario",
                MessageBoxButton.YesNo, MessageBoxImage.Information);

            if (reiniciar == MessageBoxResult.Yes)
                Application.Current.Shutdown();
        }
    }
}
