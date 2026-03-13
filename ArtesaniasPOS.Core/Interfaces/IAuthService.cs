namespace ArtesaniasPOS.Core.Interfaces
{
    public interface IAuthService
    {
        Task<SesionUsuario?> LoginAsync(string nombreUsuario, string password);
    }

    public class SesionUsuario
    {
        public int UsuarioId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string NombreUsuario { get; set; } = string.Empty;
        public int PerfilId { get; set; }
        public string PerfilNombre { get; set; } = string.Empty;

        public bool EsAdmin => PerfilId == 1;
        public bool EsVendedor => PerfilId == 2;
    }
}
