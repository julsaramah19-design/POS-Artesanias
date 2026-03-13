namespace ArtesaniasPOS.Core.Interfaces
{
    public interface IMonedaService
    {
        Task<IEnumerable<MonedaDto>> ObtenerActivasAsync();
        Task EstablecerMonedaBaseAsync(int monedaId);
    }

    public class MonedaDto
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Simbolo { get; set; } = string.Empty;
        public double TasaCambio { get; set; }
        public bool EsMonedaBase { get; set; }

        public string DisplayText => $"{Codigo} - {Nombre} ({Simbolo})";
    }
}
