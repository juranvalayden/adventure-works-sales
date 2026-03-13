using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sales.Domain.Entities;

namespace Sales.Infrastructure.Configurations;

public class SalesOrderHeaderTakenConfiguration : IEntityTypeConfiguration<SalesOrderHeaderTaken>
{
    public void Configure(EntityTypeBuilder<SalesOrderHeaderTaken> builder)
    {
        builder.ToTable("SalesOrderHeaderTaken");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .IsRequired()
            .HasColumnName("Id")
            .HasColumnType("int");

        builder.Property(s => s.SalesOrderId)
            .IsRequired()
            .HasColumnName("SalesOrderId")
            .HasColumnType("int");

        builder.Property(s => s.DateTaken)
            .IsRequired()
            .HasColumnType("datetimeoffset")
            .HasPrecision(7);
    }
}