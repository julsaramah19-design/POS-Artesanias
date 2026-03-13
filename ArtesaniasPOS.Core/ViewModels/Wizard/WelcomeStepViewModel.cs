namespace ArtesaniasPOS.Core.ViewModels.Wizard
{
    /// <summary>
    /// ViewModel para el paso de Bienvenida.
    /// No tiene dependencias — solo expone textos para la vista.
    /// </summary>
    public class WelcomeStepViewModel : ViewModelBase
    {
        public string TituloProducto => "ArtesaniasPOS";
        public string MensajeBienvenida => "¡Bienvenido! Vamos a configurar tu sistema de ventas en pocos pasos.";
        public string Descripcion => "Este asistente te guiará para dejar tu negocio listo para vender.";
    }
}
