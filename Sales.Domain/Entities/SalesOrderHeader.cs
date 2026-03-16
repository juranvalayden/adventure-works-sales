namespace Sales.Domain.Entities;

public class SalesOrderHeader
{
    // Primary Key
    public int Id { get; set; }

    // Required fields
    public byte RevisionNumber { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime DueDate { get; set; }
    public byte Status { get; set; }
    public bool OnlineOrderFlag { get; set; }
    
    public string ShipMethod { get; set; } = string.Empty;
    public decimal SubTotal { get; set; }
    public decimal TaxAmt { get; set; }
    public decimal Freight { get; set; }
    public Guid RowGuid { get; set; }
    public DateTime ModifiedDate { get; set; }

    // Optional fields
    public DateTime? ShipDate { get; set; }
    public string? PurchaseOrderNumber { get; set; }
    public string? AccountNumber { get; set; }
    public string? CreditCardApprovalCode { get; set; }
    public string? Comment { get; set; }

    // Computed fields (read-only, database generated)
    public string SalesOrderNumber { get; private set; } = string.Empty;
    public decimal TotalDue { get; private set; }

    // foreign keys properties
    public int CustomerId { get; set; }
    public virtual Customer? Customer { get; set; }

    public int? ShipToAddressId { get; set; }
    public virtual Address ShippingAddress { get; set; }
    
    public int? BillToAddressId { get; set; }
    public virtual Address BillingAddress { get; set; }

    public virtual IEnumerable<SalesOrderDetail> SalesOrderDetails { get; set; } = [];
    
}