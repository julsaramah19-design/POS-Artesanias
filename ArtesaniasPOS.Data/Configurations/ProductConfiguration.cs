using ArtesaniasPOS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArtesaniasPOS.Data.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Barcode)
               .IsRequired()
               .HasMaxLength(50);

        builder.HasIndex(p => p.Barcode)
               .IsUnique();

        builder.Property(p => p.Name)
               .IsRequired()
               .HasMaxLength(150);

        builder.Property(p => p.Price)
               .HasPrecision(18, 2);
    }
}
