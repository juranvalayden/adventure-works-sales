using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sales.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "SalesLT");

            migrationBuilder.CreateTable(
                name: "SalesOrderHeader",
                schema: "SalesLT",
                columns: table => new
                {
                    SalesOrderID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RevisionNumber = table.Column<byte>(type: "tinyint", nullable: false),
                    OrderDate = table.Column<DateTime>(type: "datetime(7)", precision: 7, nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime(7)", precision: 7, nullable: false),
                    ShipDate = table.Column<DateTime>(type: "datetime(7)", precision: 7, nullable: true),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    OnlineOrderFlag = table.Column<bool>(type: "bit", nullable: false),
                    SalesOrderNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PurchaseOrderNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AccountNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    CustomerID = table.Column<int>(type: "int", nullable: false),
                    ShipToAddressID = table.Column<int>(type: "int", nullable: true),
                    BillToAddressID = table.Column<int>(type: "int", nullable: true),
                    ShipMethod = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreditCardApprovalCode = table.Column<string>(type: "varchar(15)", unicode: false, maxLength: 15, nullable: true),
                    SubTotal = table.Column<decimal>(type: "money", nullable: false),
                    TaxAmt = table.Column<decimal>(type: "money", nullable: false),
                    Freight = table.Column<decimal>(type: "money", nullable: false),
                    TotalDue = table.Column<decimal>(type: "money", nullable: true),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    rowguid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime(7)", precision: 7, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesOrderHeader", x => x.SalesOrderID);
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
                name: "SalesOrderDetail",
                schema: "SalesLT",
                columns: table => new
                {
                    SalesOrderDetailID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderQty = table.Column<short>(type: "smallint", nullable: false),
                    ProductID = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "money", nullable: false),
                    UnitPriceDiscount = table.Column<decimal>(type: "money", nullable: false),
                    LineTotal = table.Column<decimal>(type: "numeric(38,6)", nullable: true),
                    rowguid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime(7)", precision: 7, nullable: false),
                    SalesOrderID = table.Column<int>(type: "int", nullable: false)
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
        }
    }
}
