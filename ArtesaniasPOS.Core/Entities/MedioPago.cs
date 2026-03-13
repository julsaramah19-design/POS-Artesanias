namespace ArtesaniasPOS.Core.Entities
{
    public class MedioPago
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public bool Activo { get; set; } = true;
    }
}