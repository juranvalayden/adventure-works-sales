using Sales.Application.Dtos;
using Sales.Domain.Entities;

namespace Sales.Application.Mappers;

public static class Mapper
{
    public static SalesOrderHeader MapCreationDtoToEntity(SalesOrderHeaderForCreationDto dto)
    {
        // business logic applied
        var currentDate = DateTime.UtcNow;
        var dueDate = currentDate.AddDays(14);
        var rowGuid = Guid.NewGuid();

        return new SalesOrderHeader
        {
            CustomerId = dto.CustomerId,

            RevisionNumber = dto.RevisionNumber,
            Status = dto.Status,
            OnlineOrderFlag = dto.OnlineOrderFlag,
            ShipMethod = dto.ShipMethod,
            SubTotal = dto.SubTotal,
            TaxAmt = dto.TaxAmount,
            Freight = dto.Freight,

            // System-managed fields
            DueDate = dueDate,
            RowGuid = rowGuid,
            OrderDate = currentDate,
            ModifiedDate = currentDate,

            // optional/nullable fields
            AccountNumber = dto?.AccountNumber ?? null,
            PurchaseOrderNumber = dto?.PurchaseOrderNumber ?? null,
            CreditCardApprovalCode = dto?.CreditCardApprovalCode ?? null,
            ShipToAddressId = dto?.ShipToAddressId ?? null,
            BillToAddressId = dto?.BillToAddressId ?? null,
            Comment = dto?.Comment ?? null,
            ShipDate = dto?.ShipDate ?? null
        };
    }

    public static SalesOrderHeaderDto MapFromEntityToDto(SalesOrderHeader entity)
    {
        return new SalesOrderHeaderDto
        {
            Id = entity.Id,
            RevisionNumber = entity.RevisionNumber,
            OrderDate = entity.OrderDate,
            DueDate = entity.DueDate,
            ShipDate = entity.ShipDate,
            Status = entity.Status,
            OnlineOrderFlag = entity.OnlineOrderFlag,
            SalesOrderNumber = entity.SalesOrderNumber,
            PurchaseOrderNumber = entity.PurchaseOrderNumber,
            AccountNumber = entity.AccountNumber,
            CustomerId = entity.CustomerId,
            ShipToAddressId = entity.ShipToAddressId,
            BillToAddressId = entity.BillToAddressId,
            ShipMethod = entity.ShipMethod,
            CreditCardApprovalCode = entity.CreditCardApprovalCode,
            SubTotal = entity.SubTotal,
            TaxAmt = entity.TaxAmt,
            Freight = entity.Freight,
            TotalDue = entity.TotalDue,
            Comment = entity.Comment,
            RowGuid = entity.RowGuid,
            ModifiedDate = entity.ModifiedDate,
            SalesOrderDetails = entity.SalesOrderDetails.Select(s => new SalesOrderDetailDto
            {
                Id = s.Id,
                OrderQty = s.OrderQty,
                ProductId = s.ProductId,
                UnitPrice = s.UnitPrice,
                UnitPriceDiscount = s.UnitPriceDiscount,
                LineTotal = s.LineTotal,
                RowGuid = s.RowGuid,
                ModifiedDate = s.ModifiedDate,
                SalesOrderHeaderId = s.SalesOrderHeaderId
            }).ToList()
        };
    }

    public static IEnumerable<SalesOrderHeaderDto> MapFromEntitiesToDtos(IEnumerable<SalesOrderHeader> entities)
    {
        return entities.Select(entity => new SalesOrderHeaderDto
        {
            Id = entity.Id,
            RevisionNumber = entity.RevisionNumber,
            OrderDate = entity.OrderDate,
            DueDate = entity.DueDate,
            ShipDate = entity.ShipDate,
            Status = entity.Status,
            OnlineOrderFlag = entity.OnlineOrderFlag,
            SalesOrderNumber = entity.SalesOrderNumber,
            PurchaseOrderNumber = entity.PurchaseOrderNumber,
            AccountNumber = entity.AccountNumber,
            CustomerId = entity.CustomerId,
            ShipToAddressId = entity.ShipToAddressId,
            BillToAddressId = entity.BillToAddressId,
            ShipMethod = entity.ShipMethod,
            CreditCardApprovalCode = entity.CreditCardApprovalCode,
            SubTotal = entity.SubTotal,
            TaxAmt = entity.TaxAmt,
            Freight = entity.Freight,
            TotalDue = entity.TotalDue,
            Comment = entity.Comment,
            RowGuid = entity.RowGuid,
            ModifiedDate = entity.ModifiedDate,
            SalesOrderDetails = entity.SalesOrderDetails.Select(s => new SalesOrderDetailDto
            {
                Id = s.Id,
                OrderQty = s.OrderQty,
                ProductId = s.ProductId,
                UnitPrice = s.UnitPrice,
                UnitPriceDiscount = s.UnitPriceDiscount,
                LineTotal = s.LineTotal,
                RowGuid = s.RowGuid,
                ModifiedDate = s.ModifiedDate,
                SalesOrderHeaderId = s.SalesOrderHeaderId
            }).ToList()
        }).ToList();
    }

    public static SalesOrderHeader UpdateEntityWithDto(SalesOrderHeader entityToBeUpdated, SalesOrderHeaderForUpdateDto updatedDto)
    {
        entityToBeUpdated.CreditCardApprovalCode = updatedDto.CreditCardApprovalCode;
        entityToBeUpdated.Comment = updatedDto.Comment;

        return entityToBeUpdated;
    }
}
