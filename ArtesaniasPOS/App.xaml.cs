using ArtesaniasPOS.Data.Database;
using System.Windows;

namespace ArtesaniasPOS
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var initializer = new DatabaseInitializer(DatabaseConfig.ConnectionString);
            initializer.Initialize();
        }
    }
}