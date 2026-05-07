namespace Sales.Application.Common;

public static class CacheKeys
{
    public const string AllPrefix = "salesorderkey:";

    public static string BySalesOrderNumber(string salesOrderNumber)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nameof(salesOrderNumber), salesOrderNumber);

        var normalized = Normalize(salesOrderNumber);
        var normalizedParameter = Normalize(nameof(salesOrderNumber));

        return $"{AllPrefix}{normalizedParameter}:{normalized}";
    }

    public static string ById(int id)
    {
        return id <= 0
            ? throw new ArgumentException("Id should be a positive integer.", nameof(id))
            : $"{AllPrefix}id:{id}";
    }

    public static string All(string? salesOrderNumber)
    {
        var salesOrderNumberFilter = string.IsNullOrWhiteSpace(salesOrderNumber) ? "*" : Normalize(salesOrderNumber);
        return $"{AllPrefix}all:{salesOrderNumberFilter}";
    }

    private static string Normalize(string input)
    {
        var trimmed = input.Trim();
        trimmed = trimmed.ToLower();
        return trimmed;
    }
}