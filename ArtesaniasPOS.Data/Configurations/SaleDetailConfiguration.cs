using ArtesaniasPOS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArtesaniasPOS.Data.Configurations;

public class SaleDetailConfiguration : IEntityTypeConfiguration<SaleDetail>
{
    public void Configure(EntityTypeBuilder<SaleDetail> builder)
    {
        builder.HasKey(d => d.Id);

        builder.Property(d => d.UnitPrice)
               .HasPrecision(18, 2);

        builder.Property(d => d.Subtotal)
               .HasPrecision(18, 2);
    }
}
