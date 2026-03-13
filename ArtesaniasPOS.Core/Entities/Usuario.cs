namespace ArtesaniasPOS.Core.Entities
{
    public class Usuario
    {
        public int Id { get; set; }
        public int PerfilId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string NombreUsuario { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public bool Activo { get; set; } = true;
        public DateTime FechaCreacion { get; set; }
        public string NombrePerfil { get; set; } = string.Empty;
    }
}