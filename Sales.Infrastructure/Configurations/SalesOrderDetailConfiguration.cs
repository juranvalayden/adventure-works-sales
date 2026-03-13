using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sales.Domain.Entities;

namespace Sales.Infrastructure.Configurations;

public class SalesOrderDetailConfiguration : IEntityTypeConfiguration<SalesOrderDetail>
{
    public void Configure(EntityTypeBuilder<SalesOrderDetail> builder)
    {
        builder.ToTable("SalesOrderDetail");

        // Primary Key
        builder.HasKey(d => d.Id)
            .HasName("PK_SalesOrderDetail");

        builder.Property(d => d.Id)
            .IsRequired()
            .HasColumnName("SalesOrderDetailID")
            .HasColumnType("int");

        builder.Property(d => d.SalesOrderHeaderId)
            .IsRequired()
            .HasColumnName("SalesOrderID")
            .HasColumnType("int");

        builder.Property(d => d.OrderQty)
            .IsRequired()
            .HasColumnType("smallint");

        builder.Property(d => d.ProductId)
            .IsRequired()
            .HasColumnName("ProductID")
            .HasColumnType("int");

        builder.Property(d => d.UnitPrice)
            .IsRequired()
            .HasColumnType("money");

        builder.Property(d => d.UnitPriceDiscount)
            .IsRequired()
            .HasColumnType("money");

        builder.Property(d => d.LineTotal)
            .HasColumnType("numeric(38,6)")
            .IsRequired(false);

        builder.Property(d => d.RowGuid)
            .IsRequired()
            .HasColumnName("rowguid")
            .HasColumnType("uniqueidentifier");

        builder.Property(d => d.ModifiedDate)
            .IsRequired()
            .HasColumnType("datetime")
            .HasPrecision(7);

        // Relationship: Each detail belongs to one SalesOrderHeader
        builder.HasOne(d => d.SalesOrderHeader)
            .WithMany(h => h.SalesOrderDetails)
            .HasForeignKey(d => d.SalesOrderHeaderId);
    }
}