using System;

namespace ArtesaniasPOS.Core.ViewModels
{
    /// <summary>
    /// Pantalla de inicio: da la bienvenida y funciona como guía/instructivo
    /// de cómo usar toda la aplicación. Se muestra al entrar (módulo Dashboard).
    /// </summary>
    public class InicioViewModel : ViewModelBase
    {
        public string NombreNegocio { get; }
        public string NombreUsuario { get; }
        public bool EsAdmin { get; }

        /// <summary>Saludo según la hora del día.</summary>
        public string Saludo { get; }

        public InicioViewModel(string nombreNegocio, string nombreUsuario, bool esAdmin)
        {
            NombreNegocio = nombreNegocio;
            NombreUsuario = nombreUsuario;
            EsAdmin = esAdmin;

            var hora = DateTime.Now.Hour;
            Saludo = hora < 12 ? "Buenos días"
                   : hora < 19 ? "Buenas tardes"
                   : "Buenas noches";
        }
    }
}
