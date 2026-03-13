using ArtesaniasPOS.Core.Entities;

namespace ArtesaniasPOS.Core.Session
{
    public static class SesionActual 
    {
        public static Usuario? Usuario { get; private set; }

        public static bool EstaLogueado => Usuario != null;
        public static bool EsAdmin => Usuario?.NombrePerfil == "Admin";

        public static void Iniciar(Usuario usuario)
        {
            Usuario = usuario;
        }

        public static void Cerrar()
        {
            Usuario = null;
        }
    }
}