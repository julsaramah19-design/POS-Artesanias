using ArtesaniasPOS.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace ArtesaniasPOS.Data.Context;
public class AppDbContext : DbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<SaleDetail> SaleDetails => Set<SaleDetail>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var databasePath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "data",
            "artesanias.db"
        );

        Directory.CreateDirectory(Path.GetDirectoryName(databasePath)!);

        optionsBuilder.UseSqlite($"Data Source={databasePath}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}