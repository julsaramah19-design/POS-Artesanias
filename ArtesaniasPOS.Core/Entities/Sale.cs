using ArtesaniasPOS.Core.Enums;

namespace ArtesaniasPOS.Core.Entities;

public class Sale
{
    public int Id { get; set; }
    public DateTime SaleDate { get; set; } = DateTime.Now;

    public int UserId { get; set; }
    public User? User { get; set; }

    public decimal Total { get; set; }
    public PaymentMethod PaymentMethod { get; set; }

    public ICollection<SaleDetail> Details { get; set; } = new List<SaleDetail>();
}
