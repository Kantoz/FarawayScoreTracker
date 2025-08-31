using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Faraway.ScoreTracker.Tests;

public static class JsonHelpers
{
    public static StringContent AsStringEnumJson<T>(T payload)
    {
        JsonSerializerOptions opts = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        opts.Converters.Add(new JsonStringEnumConverter());

        string json = JsonSerializer.Serialize(payload, opts);
        return new StringContent(json, Encoding.UTF8, "application/json");
    }
    
    public static readonly JsonSerializerOptions ReadStringEnums = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };
}