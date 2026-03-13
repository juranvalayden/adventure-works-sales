namespace Sales.Domain.Entities;

public class SalesOrderHeaderTaken
{
    public int Id { get; set; }

    public int SalesOrderId { get; set; }

    public DateTimeOffset DateTaken = DateTimeOffset.Now;
}