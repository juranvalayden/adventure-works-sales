namespace Sales.Application.Configurations;

public class SalesCachingOptions
{
    public const string CacheSettingOptions = "CacheSettingOptions";
    public bool Enabled { get; set; }
    public TimeSpan SlidingExpiration { get; set; }
    public TimeSpan AbsoluteExpiration { get; set; }
    public TimeSpan RefreshCacheInterval { get; set; }
}