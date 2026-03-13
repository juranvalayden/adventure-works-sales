using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sales.Domain.Entities;

namespace Sales.Infrastructure.Configurations;

public class SalesOrderHeaderConfiguration : IEntityTypeConfiguration<SalesOrderHeader>
{
    public void Configure(EntityTypeBuilder<SalesOrderHeader> builder)
    {
        builder.ToTable("SalesOrderHeader");

        builder.HasKey(s => s.Id)
            .HasName("PK_SalesOrderHeader");

        builder.Property(s => s.Id)
            .IsRequired()
            .HasColumnType("int");

        builder.Property(s => s.RevisionNumber)
            .IsRequired()
            .HasColumnType("tinyint");

        builder.Property(s => s.OrderDate)
            .IsRequired()
            .HasColumnType("datetime")
            .HasPrecision(7);

        builder.Property(s => s.DueDate)
            .IsRequired()
            .HasColumnType("datetime")
            .HasPrecision(7);

        builder.Property(s => s.ShipDate)
            .HasColumnType("datetime")
            .HasPrecision(7)
            .IsRequired(false);

        builder.Property(s => s.Status)
            .IsRequired()
            .HasColumnType("tinyint");

        builder.Property(s => s.OnlineOrderFlag)
            .IsRequired()
            .HasColumnType("bit");

        builder.Property(s => s.SalesOrderNumber)
            .HasMaxLength(50)
            .IsUnicode()
            .IsRequired(false);

        builder.Property(s => s.PurchaseOrderNumber)
            .HasMaxLength(50)
            .IsUnicode()
            .IsRequired(false);

        builder.Property(s => s.AccountNumber)
            .HasMaxLength(30)
            .IsUnicode()
            .IsRequired(false);

        builder.Property(s => s.CustomerId)
            .IsRequired()
            .HasColumnName("CustomerID")
            .HasColumnType("int");

        builder.Property(s => s.ShipToAddressId)
            .HasColumnName("ShipToAddressID")
            .HasColumnType("int")
            .IsRequired(false);

        builder.Property(s => s.BillToAddressId)
            .HasColumnName("BillToAddressID")
            .HasColumnType("int")
            .IsRequired(false);

        builder.Property(s => s.ShipMethod)
            .IsRequired()
            .HasMaxLength(100)
            .IsUnicode();

        builder.Property(s => s.CreditCardApprovalCode)
            .HasMaxLength(15)
            .IsUnicode(false)
            .IsRequired(false);

        builder.Property(s => s.SubTotal)
            .IsRequired()
            .HasColumnType("money");

        builder.Property(s => s.TaxAmt)
            .IsRequired()
            .HasColumnType("money");

        builder.Property(s => s.Freight)
            .IsRequired()
            .HasColumnType("money");

        builder.Property(s => s.TotalDue)
            .HasColumnType("money")
            .IsRequired(false);

        builder.Property(s => s.Comment)
            .IsUnicode()
            .HasColumnType("nvarchar(max)")
            .IsRequired(false);

        builder.Property(s => s.RowGuid)
            .IsRequired()
            .HasColumnName("rowguid")
            .HasColumnType("uniqueidentifier");

        builder.Property(s => s.ModifiedDate)
            .IsRequired()
            .HasColumnType("datetime")
            .HasPrecision(7);

        // One-to-many relationship
        builder.HasMany(s => s.SalesOrderDetails)
            .WithOne(d => d.SalesOrderHeader)
            .HasForeignKey(d => d.SalesOrderHeaderId);
    }
}