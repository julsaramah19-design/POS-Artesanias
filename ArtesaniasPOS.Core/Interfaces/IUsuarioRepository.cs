using ArtesaniasPOS.Core.Entities;

namespace ArtesaniasPOS.Core.Interfaces
{
    public interface IUsuarioRepository
    {
        Task<Usuario?> ObtenerPorNombreUsuarioAsync(string nombreUsuario);
        Task<IEnumerable<Usuario>> ObtenerTodosAsync();
        Task<int> CrearAsync(Usuario usuario);
        Task ActualizarAsync(Usuario usuario);
        Task DesactivarAsync(int id);
    }
}