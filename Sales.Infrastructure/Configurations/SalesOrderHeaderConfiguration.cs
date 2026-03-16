using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sales.Domain.Entities;

namespace Sales.Infrastructure.Configurations;

public class SalesOrderHeaderConfiguration : IEntityTypeConfiguration<SalesOrderHeader>
{
    public void Configure(EntityTypeBuilder<SalesOrderHeader> builder)
    {
        // Table
        builder.ToTable("SalesOrderHeader", "SalesLT");

        // Primary Key
        builder.HasKey(e => e.Id)
            .HasName("PK_SalesOrderHeader_SalesOrderID");

        builder.Property(e => e.Id)
            .HasColumnName("SalesOrderID");

        // Required fields
        builder.Property(e => e.RevisionNumber)
            .HasColumnName("RevisionNumber")
            .HasDefaultValue((byte)0);

        builder.Property(e => e.OrderDate)
            .HasColumnName("OrderDate")
            .HasDefaultValueSql("getdate()");

        builder.Property(e => e.DueDate)
            .HasColumnName("DueDate");

        builder.Property(e => e.Status)
            .HasColumnName("Status")
            .HasDefaultValue((byte)1);

        builder.Property(e => e.OnlineOrderFlag)
            .HasColumnName("OnlineOrderFlag")
            .HasDefaultValue(true);

        builder.Property(e => e.CustomerId)
            .HasColumnName("CustomerID");

        builder.Property(e => e.ShipMethod)
            .HasColumnName("ShipMethod")
            .HasMaxLength(100);

        builder.Property(e => e.SubTotal)
            .HasColumnName("SubTotal")
            .HasColumnType("money")
            .HasDefaultValue(0.00m);

        builder.Property(e => e.TaxAmt)
            .HasColumnName("TaxAmt")
            .HasColumnType("money")
            .HasDefaultValue(0.00m);

        builder.Property(e => e.Freight)
            .HasColumnName("Freight")
            .HasColumnType("money")
            .HasDefaultValue(0.00m);

        builder.Property(e => e.RowGuid)
            .HasColumnName("rowguid")
            .HasDefaultValueSql("newid()");

        builder.Property(e => e.ModifiedDate)
            .HasColumnName("ModifiedDate")
            .HasDefaultValueSql("getdate()");

        // Optional fields
        builder.Property(e => e.ShipDate)
            .HasColumnName("ShipDate");

        builder.Property(e => e.PurchaseOrderNumber)
            .HasColumnName("PurchaseOrderNumber")
            .HasMaxLength(50);

        builder.Property(e => e.AccountNumber)
            .HasColumnName("AccountNumber")
            .HasMaxLength(30);

        builder.Property(e => e.ShipToAddressId)
            .HasColumnName("ShipToAddressID");

        builder.Property(e => e.BillToAddressId)
            .HasColumnName("BillToAddressID");

        builder.Property(e => e.CreditCardApprovalCode)
            .HasColumnName("CreditCardApprovalCode")
            .HasMaxLength(15);

        builder.Property(e => e.Comment)
            .HasColumnName("Comment");

        // Computed fields
        builder.Property(e => e.SalesOrderNumber)
            .HasColumnName("SalesOrderNumber")
            .ValueGeneratedOnAddOrUpdate();

        builder.Property(e => e.TotalDue)
            .HasColumnName("TotalDue")
            .HasColumnType("money")
            .ValueGeneratedOnAddOrUpdate();

        // Indexes and unique constraints
        builder.HasIndex(e => e.RowGuid)
            .IsUnique()
            .HasDatabaseName("AK_SalesOrderHeader_rowguid");

        builder.HasIndex(e => e.SalesOrderNumber)
            .IsUnique()
            .HasDatabaseName("AK_SalesOrderHeader_SalesOrderNumber");

        builder.HasIndex(e => e.CustomerId)
            .HasDatabaseName("IX_SalesOrderHeader_CustomerID");

        // Foreign keys
        builder.HasOne(e => e.ShippingAddress)
            .WithMany()
            .HasForeignKey(e => e.ShipToAddressId)
            .HasConstraintName("FK_SalesOrderHeader_Address_ShipTo_AddressID");

        builder.HasOne(e => e.BillingAddress)
            .WithMany()
            .HasForeignKey(e => e.BillToAddressId)
            .HasConstraintName("FK_SalesOrderHeader_Address_BillTo_AddressID");

        builder.HasMany(e => e.SalesOrderDetails)
            .WithOne(d => d.SalesOrderHeader)     // Each detail belongs to one order
            .HasForeignKey(d => d.SalesOrderHeaderId)
            .HasConstraintName("FK_SalesOrderDetail_SalesOrderHeader_SalesOrderID");
    }
}