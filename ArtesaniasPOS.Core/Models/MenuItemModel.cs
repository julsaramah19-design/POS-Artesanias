namespace ArtesaniasPOS.Core.Models
{
    /// <summary>
    /// Representa un item del menú lateral.
    /// Se crea dinámicamente según el perfil del usuario logueado.
    /// </summary>
    public class MenuItemModel
    {
        public string Titulo { get; set; } = string.Empty;
        public string Icono { get; set; } = string.Empty;
        public string Modulo { get; set; } = string.Empty;

        /// <summary>
        /// Perfiles que pueden ver este item.
        /// 1 = Admin, 2 = Vendedor.
        /// Si contiene ambos, ambos lo ven.
        /// </summary>
        public List<int> PerfilesPermitidos { get; set; } = new();
    }
}
