using Sales.Application.Dtos;

namespace Sales.Application.Factories;

public static class SalesOrderFactory
{
    public static SalesOrderHeaderForCreationDto Create()
    {
        const int subTotalMin = 500;
        const int subTotalMax = 10000;
        const int freightMin = 20;
        const int freightMax = 200;

        var subTotal = Random.Shared.NextDouble() * (subTotalMax - subTotalMin) + subTotalMin;
        var freight = Random.Shared.NextDouble() * (freightMax - freightMin) + freightMin;
        var taxAmount = (subTotal + freight) * 0.15;

        return new SalesOrderHeaderForCreationDto
        {
            RevisionNumber = 0,
            Status = 5,
            CustomerId = 29847,
            ShipMethod = "CARGO TRANSPORT 5",
            SubTotal = (decimal)Math.Round(subTotal, 2),
            TaxAmount = (decimal)Math.Round(taxAmount, 2),
            Freight = (decimal)Math.Round(freight, 2),
            ShipDate = null,
            PurchaseOrderNumber = null,
            AccountNumber = null,
            ShipToAddressId = null,
            BillToAddressId = null,
            CreditCardApprovalCode = null,
            Comment = null
        };
    }
}