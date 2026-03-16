namespace Sales.Domain.Entities;

public class SalesOrderDetail
{
    public int Id { get; private set; } // make setter private
    public int SalesOrderHeaderId { get; set; }
    public short OrderQty { get; set; }
    public int ProductId { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal UnitPriceDiscount { get; set; }
    public decimal? LineTotal { get; private set; } // computed
    public Guid RowGuid { get; private set; } // default NEWID()
    public DateTime ModifiedDate { get; private set; } // default GETDATE()

    public SalesOrderHeader SalesOrderHeader { get; set; } = null!;
}