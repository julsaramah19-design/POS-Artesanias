namespace ArtesaniasPOS.Core.Entities
{
    public class Moneda
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Simbolo { get; set; } = string.Empty;
        public decimal TasaCambio { get; set; } = 1;
        public bool EsMonedaBase { get; set; }
        public bool Activo { get; set; } = true;
    }
}