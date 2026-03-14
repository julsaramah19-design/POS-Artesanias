namespace ArtesaniasPOS.Core.Interfaces
{
    public interface IConfiguracionAdminService
    {
        // === USUARIOS ===
        Task<IEnumerable<UsuarioListaDto>> ObtenerUsuariosAsync();
        Task CrearUsuarioAsync(UsuarioCrearDto usuario);
        Task ActualizarUsuarioAsync(UsuarioEditarDto usuario);
        Task DesactivarUsuarioAsync(int id);
        Task<IEnumerable<PerfilDto>> ObtenerPerfilesAsync();

        // === CATEGORÍAS ===
        Task<IEnumerable<CategoriaAdminDto>> ObtenerCategoriasAsync();
        Task CrearCategoriaAsync(string nombre);
        Task ActualizarCategoriaAsync(int id, string nombre);
        Task DesactivarCategoriaAsync(int id);

        // === MEDIOS DE PAGO ===
        Task<IEnumerable<MedioPagoAdminDto>> ObtenerMediosPagoAsync();
        Task CrearMedioPagoAsync(string nombre);
        Task ActualizarMedioPagoAsync(int id, string nombre);
        Task DesactivarMedioPagoAsync(int id);
    }

    public class UsuarioListaDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string NombreUsuario { get; set; } = string.Empty;
        public string PerfilNombre { get; set; } = string.Empty;
        public int PerfilId { get; set; }
        public bool Activo { get; set; }
        public string FechaCreacion { get; set; } = string.Empty;
    }

    public class UsuarioCrearDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string NombreUsuario { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public int PerfilId { get; set; }
    }

    public class UsuarioEditarDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string NombreUsuario { get; set; } = string.Empty;
        public string? NuevoPassword { get; set; }
        public int PerfilId { get; set; }
    }

    public class PerfilDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
    }

    public class CategoriaAdminDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public bool Activo { get; set; }
    }

    public class MedioPagoAdminDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public bool Activo { get; set; }
    }
}
