using Microsoft.EntityFrameworkCore;
using Sales.Domain.Entities;

namespace Sales.Infrastructure.Configurations.Persistence;

public class SalesDbContext(DbContextOptions<SalesDbContext> options) : DbContext(options)
{
    public DbSet<SalesOrderHeader> SalesOrderHeaders { get; set; } = null!;
    public DbSet<SalesOrderDetail> SalesOrderDetails { get; set; } = null!;
    public DbSet<SalesOrderHeaderTaken> SalesOrderHeaderTaken { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.HasDefaultSchema("SalesLT");
        
        modelBuilder.ApplyConfiguration(new SalesOrderHeaderConfiguration());
        modelBuilder.ApplyConfiguration(new SalesOrderDetailConfiguration());
        modelBuilder.ApplyConfiguration(new SalesOrderHeaderTakenConfiguration());
    }
}