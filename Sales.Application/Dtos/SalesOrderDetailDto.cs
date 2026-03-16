namespace Sales.Application.Dtos;

public record SalesOrderDetailDto
{
    public int Id { get; init; }
    public short OrderQty { get; init; }
    public int ProductId { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal UnitPriceDiscount { get; init; }
    public decimal? LineTotal { get; init; }
    public Guid RowGuid { get; init; }
    public DateTime ModifiedDate { get; init; }

    public int SalesOrderHeaderId { get; init; }
    public ICollection<SalesOrderDetailDto> SalesOrderDetails { get; init; } = new List<SalesOrderDetailDto>();
}