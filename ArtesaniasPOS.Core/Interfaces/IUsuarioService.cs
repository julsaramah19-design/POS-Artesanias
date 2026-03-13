namespace ArtesaniasPOS.Core.Interfaces
{
    public interface IUsuarioService
    {
        Task ActualizarAdminAsync(string nombre, string nombreUsuario, string password);
        Task<AdminInfoDto?> ObtenerAdminActualAsync();
        Task<bool> ExisteNombreUsuarioAsync(string nombreUsuario, int excluirId = 1);
    }

    public class AdminInfoDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string NombreUsuario { get; set; } = string.Empty;
    }
}
