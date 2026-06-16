using System.Windows;

namespace ArtesaniasPOS
{
    public partial class App : Application
    {
        // La migración e inicialización de la base de datos las realiza
        // MainWindow (primero migra la BD heredada y luego inicializa sobre
        // AppSettings.DbPath). No se inicializa aquí para no crear una BD
        // suelta ni adelantarse a la migración.
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
        }
    }
}