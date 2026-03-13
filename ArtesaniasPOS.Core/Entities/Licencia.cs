namespace ArtesaniasPOS.Core.Entities
{
    public class Licencia
    {
        public int Id { get; set; }
        public string NombreCliente { get; set; } = string.Empty;
        public string HardwareId { get; set; } = string.Empty;
        public DateTime FechaEmision { get; set; }
        public DateTime? FechaVencimiento { get; set; }
        public bool Activa { get; set; } = true;

        // Propiedad calculada
        public bool EstaVigente => Activa &&
            (FechaVencimiento == null || FechaVencimiento > DateTime.Now);
    }
}