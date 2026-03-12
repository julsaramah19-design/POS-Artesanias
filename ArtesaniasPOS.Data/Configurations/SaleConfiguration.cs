using ArtesaniasPOS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArtesaniasPOS.Data.Configurations;

public class SaleConfiguration : IEntityTypeConfiguration<Sale>
{
    public void Configure(EntityTypeBuilder<Sale> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Total)
               .HasPrecision(18, 2);

        builder.HasMany(s => s.Details)
               .WithOne(d => d.Sale)
               .HasForeignKey(d => d.SaleId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
