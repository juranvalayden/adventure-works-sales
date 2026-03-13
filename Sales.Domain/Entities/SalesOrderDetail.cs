namespace Sales.Domain.Entities;

public class SalesOrderDetail
{
    public int Id { get; set; }
    public short OrderQty { get; set; }
    public int ProductId { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal UnitPriceDiscount { get; set; }
    public decimal? LineTotal { get; set; }
    public Guid RowGuid { get; set; }
    public DateTime ModifiedDate { get; set; }

    // Navigation
    public int SalesOrderHeaderId { get; set; }
    public SalesOrderHeader SalesOrderHeader { get; set; } = null!;
}