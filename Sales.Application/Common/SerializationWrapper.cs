using System.Text.Json;

namespace Sales.Application.Common;

public class SerializationWrapper
{
    public JsonSerializerOptions Options { get; } = new(JsonSerializerDefaults.Web)
    {
        DefaultBufferSize = 10
    };
}