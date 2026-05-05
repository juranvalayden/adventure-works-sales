using System;
using System.Collections.Generic;
using System.Text;

namespace Sales.Application.Dtos;

public class SalesOrderHeaderWithoutSalesOrderDetailDto
{
    public DateTime OrderDate { get; init; }
    public DateTime DueDate { get; init; }
    public DateTime? ShipDate { get; init; }
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
}
