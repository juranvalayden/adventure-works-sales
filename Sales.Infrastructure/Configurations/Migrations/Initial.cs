#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace Sales.Infrastructure.Configurations.Migrations;

/// <inheritdoc />
public partial class Initial : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "SalesLT");

        migrationBuilder.CreateTable(
            name: "Address",
            schema: "SalesLT",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Address", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Customer",
            schema: "SalesLT",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Customer", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "SalesOrderHeaderTaken",
            schema: "SalesLT",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                SalesOrderId = table.Column<int>(type: "int", nullable: false),
                DateTaken = table.Column<DateTimeOffset>(type: "datetimeoffset(7)", precision: 7, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SalesOrderHeaderTaken", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "SalesOrderHeader",
            schema: "SalesLT",
            columns: table => new
            {
                SalesOrderID = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                RevisionNumber = table.Column<byte>(type: "tinyint", nullable: false, defaultValue: (byte)0),
                OrderDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()"),
                DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                Status = table.Column<byte>(type: "tinyint", nullable: false, defaultValue: (byte)1),
                OnlineOrderFlag = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                ShipMethod = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                SubTotal = table.Column<decimal>(type: "money", nullable: false, defaultValue: 0.00m),
                TaxAmt = table.Column<decimal>(type: "money", nullable: false, defaultValue: 0.00m),
                Freight = table.Column<decimal>(type: "money", nullable: false, defaultValue: 0.00m),
                rowguid = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "newid()"),
                ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()"),
                ShipDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                PurchaseOrderNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                AccountNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                CreditCardApprovalCode = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                SalesOrderNumber = table.Column<string>(type: "nvarchar(450)", nullable: false),
                TotalDue = table.Column<decimal>(type: "money", nullable: false),
                CustomerID = table.Column<int>(type: "int", nullable: false),
                ShipToAddressID = table.Column<int>(type: "int", nullable: true),
                BillToAddressID = table.Column<int>(type: "int", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SalesOrderHeader_SalesOrderID", x => x.SalesOrderID);
                table.ForeignKey(
                    name: "FK_SalesOrderHeader_Address_BillTo_AddressID",
                    column: x => x.BillToAddressID,
                    principalSchema: "SalesLT",
                    principalTable: "Address",
                    principalColumn: "Id");
                table.ForeignKey(
                    name: "FK_SalesOrderHeader_Address_ShipTo_AddressID",
                    column: x => x.ShipToAddressID,
                    principalSchema: "SalesLT",
                    principalTable: "Address",
                    principalColumn: "Id");
                table.ForeignKey(
                    name: "FK_SalesOrderHeader_Customer_CustomerID",
                    column: x => x.CustomerID,
                    principalSchema: "SalesLT",
                    principalTable: "Customer",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "SalesOrderDetail",
            schema: "SalesLT",
            columns: table => new
            {
                SalesOrderDetailID = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                SalesOrderID = table.Column<int>(type: "int", nullable: false),
                OrderQty = table.Column<short>(type: "smallint", nullable: false),
                ProductID = table.Column<int>(type: "int", nullable: false),
                UnitPrice = table.Column<decimal>(type: "money", nullable: false),
                UnitPriceDiscount = table.Column<decimal>(type: "money", nullable: false),
                LineTotal = table.Column<decimal>(type: "numeric(38,6)", nullable: true),
                rowguid = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                ModifiedDate = table.Column<DateTime>(type: "datetime(7)", precision: 7, nullable: false, defaultValueSql: "getdate()")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SalesOrderDetail", x => x.SalesOrderDetailID);
                table.ForeignKey(
                    name: "FK_SalesOrderDetail_SalesOrderHeader_SalesOrderID",
                    column: x => x.SalesOrderID,
                    principalSchema: "SalesLT",
                    principalTable: "SalesOrderHeader",
                    principalColumn: "SalesOrderID",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_SalesOrderDetail_SalesOrderID",
            schema: "SalesLT",
            table: "SalesOrderDetail",
            column: "SalesOrderID");

        migrationBuilder.CreateIndex(
            name: "AK_SalesOrderHeader_rowguid",
            schema: "SalesLT",
            table: "SalesOrderHeader",
            column: "rowguid",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "AK_SalesOrderHeader_SalesOrderNumber",
            schema: "SalesLT",
            table: "SalesOrderHeader",
            column: "SalesOrderNumber",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_SalesOrderHeader_BillToAddressID",
            schema: "SalesLT",
            table: "SalesOrderHeader",
            column: "BillToAddressID");

        migrationBuilder.CreateIndex(
            name: "IX_SalesOrderHeader_CustomerID",
            schema: "SalesLT",
            table: "SalesOrderHeader",
            column: "CustomerID");

        migrationBuilder.CreateIndex(
            name: "IX_SalesOrderHeader_ShipToAddressID",
            schema: "SalesLT",
            table: "SalesOrderHeader",
            column: "ShipToAddressID");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "SalesOrderDetail",
            schema: "SalesLT");

        migrationBuilder.DropTable(
            name: "SalesOrderHeaderTaken",
            schema: "SalesLT");

        migrationBuilder.DropTable(
            name: "SalesOrderHeader",
            schema: "SalesLT");

        migrationBuilder.DropTable(
            name: "Address",
            schema: "SalesLT");

        migrationBuilder.DropTable(
            name: "Customer",
            schema: "SalesLT");
    }
}