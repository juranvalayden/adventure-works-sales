using System.ComponentModel.DataAnnotations;

namespace Sales.Application.Dtos;

public record SalesOrderHeaderForUpdateDto
{
    [Required]
    [StringLength(15)]
    public string CreditCardApprovalCode { get; init; } = string.Empty;
    
    [Required] 
    public string Comment { get; init; } = string.Empty;
}