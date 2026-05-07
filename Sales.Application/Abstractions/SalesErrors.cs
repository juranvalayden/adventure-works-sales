namespace Sales.Application.Abstractions;

public class SalesErrors
{
    public static Error NoCache(string cacheKey) =>
        new(ErrorType.NoCache, $"Attempting'{cacheKey}' threw an error.");
}
