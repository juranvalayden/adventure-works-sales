using System.ComponentModel.DataAnnotations;

namespace Sales.Application.Dtos;
public class SalesOrderHeaderForCreationDto
{
    // Required fields
    [Required]
    [Range(0, 255)]
    public byte RevisionNumber { get; set; }

    [Required]
    [Range(0, 255)]
    public byte Status { get; set; }

    [Required]
    public bool OnlineOrderFlag { get; set; }

    [Required]
    public int CustomerId { get; set; }

    [Required]
    [StringLength(100)]
    public string ShipMethod { get; set; } = string.Empty;

    [Required]
    [Range(0, double.MaxValue)]
    public decimal SubTotal { get; set; }

    [Required]
    [Range(0, double.MaxValue)]
    public decimal TaxAmount { get; set; }

    [Required]
    [Range(0, double.MaxValue)]
    public decimal Freight { get; set; }

    // Optional fields
    public DateTime? ShipDate { get; set; }

    [StringLength(50)]
    public string? PurchaseOrderNumber { get; set; }

    [StringLength(30)]
    public string? AccountNumber { get; set; }

    public int? ShipToAddressId { get; set; }

    public int? BillToAddressId { get; set; }

    [StringLength(15)]
    public string? CreditCardApprovalCode { get; set; }

    public string? Comment { get; set; }
}