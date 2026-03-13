using System.Text.Json;

namespace Sales.Application.Common.Helpers;

public class JsonSerializerOptionsWrapper
{
    public JsonSerializerOptions Options { get; } = new(JsonSerializerDefaults.Web)
    {
        DefaultBufferSize = 10
    };
}