using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sales.Domain.Entities;

namespace Sales.Infrastructure.Configurations;

public class SalesOrderDetailConfiguration : IEntityTypeConfiguration<SalesOrderDetail>
{
    public void Configure(EntityTypeBuilder<SalesOrderDetail> builder)
    {
        builder.ToTable("SalesOrderDetail", "SalesLT", 
            tb => tb.UseSqlOutputClause(false));

        builder.HasKey(d => d.Id)
            .HasName("PK_SalesOrderDetail");

        builder.Property(d => d.Id)
            .HasColumnName("SalesOrderDetailID")
            .HasColumnType("int")
            .ValueGeneratedOnAdd();

        builder.Property(d => d.SalesOrderHeaderId)
            .HasColumnName("SalesOrderID")
            .HasColumnType("int")
            .IsRequired();

        builder.Property(d => d.OrderQty)
            .HasColumnType("smallint")
            .IsRequired();

        builder.Property(d => d.ProductId)
            .HasColumnName("ProductID")
            .HasColumnType("int")
            .IsRequired();

        builder.Property(d => d.UnitPrice)
            .HasColumnType("money")
            .IsRequired();

        builder.Property(d => d.UnitPriceDiscount)
            .HasColumnType("money")
            .IsRequired();

        // Computed column
        builder.Property(d => d.LineTotal)
            .HasColumnType("numeric(38,6)")
            .ValueGeneratedOnAddOrUpdate()
            .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

        builder.Property(d => d.RowGuid)
            .HasColumnName("rowguid")
            .HasColumnType("uniqueidentifier")
            .HasDefaultValueSql("NEWID()");

        builder.Property(d => d.ModifiedDate)
            .HasColumnType("datetime")
            .HasPrecision(7)
            .HasDefaultValueSql("getdate()");

        builder.HasOne(d => d.SalesOrderHeader)
            .WithMany(h => h.SalesOrderDetails)
            .HasForeignKey(d => d.SalesOrderHeaderId);
    }
}