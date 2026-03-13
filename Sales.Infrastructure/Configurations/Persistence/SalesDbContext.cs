using Microsoft.EntityFrameworkCore;
using Sales.Domain.Entities;

namespace Sales.Infrastructure.Configurations.Persistence;

public class SalesDbContext(DbContextOptions<SalesDbContext> options) : DbContext(options)
{
    public DbSet<SalesOrderHeader> SalesOrderHeaders { get; set; }
    public DbSet<SalesOrderDetail> SalesOrderDetails { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("SalesLT");

        modelBuilder.ApplyConfiguration(new SalesOrderHeaderConfiguration());
        modelBuilder.ApplyConfiguration(new SalesOrderDetailConfiguration());

        base.OnModelCreating(modelBuilder);
    }
}