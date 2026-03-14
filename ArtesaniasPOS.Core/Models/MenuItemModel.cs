namespace ArtesaniasPOS.Core.Models
{
    public class MenuItemModel
    {
        public string Titulo { get; set; } = string.Empty;
        public string Icono { get; set; } = string.Empty;
        public string Modulo { get; set; } = string.Empty;

        public List<int> PerfilesPermitidos { get; set; } = new();
    }
}
