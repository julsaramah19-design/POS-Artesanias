using ArtesaniasPOS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArtesaniasPOS.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Username)
               .IsRequired()
               .HasMaxLength(50);

        builder.HasIndex(u => u.Username)
               .IsUnique();

        builder.Property(u => u.PasswordHash)
               .IsRequired();
    }
}
