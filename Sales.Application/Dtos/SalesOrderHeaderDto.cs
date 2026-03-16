namespace Sales.Application.Dtos;

public record SalesOrderHeaderDto
{
    public int Id { get; init; }
    public byte RevisionNumber { get; init; }
    public DateTime OrderDate { get; init; }
    public DateTime DueDate { get; init; }
    public DateTime? ShipDate { get; init; }
    public byte Status { get; init; }
    public bool OnlineOrderFlag { get; init; }
    public string? SalesOrderNumber { get; init; }
    public string? PurchaseOrderNumber { get; init; }
    public string? AccountNumber { get; init; }
    public int CustomerId { get; init; }
    public int? ShipToAddressId { get; init; }
    public int? BillToAddressId { get; init; }
    public string ShipMethod { get; init; } = string.Empty;
    public string? CreditCardApprovalCode { get; init; }
    public decimal SubTotal { get; init; }
    public decimal TaxAmt { get; init; }
    public decimal Freight { get; init; }
    public decimal? TotalDue { get; init; }
    public string? Comment { get; init; }
    public Guid RowGuid { get; init; }
    public DateTime ModifiedDate { get; init; }

    public ICollection<SalesOrderDetailDto> SalesOrderDetails { get; init; } = new List<SalesOrderDetailDto>();
}