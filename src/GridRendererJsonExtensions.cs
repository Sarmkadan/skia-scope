using System.Text.Json;
using System.Text.Json.Serialization;

namespace SkiaScope;

public static class GridRendererJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static string ToJson(this GridRenderer value, bool indented = false)
    {
        var options = indented
            ? new JsonSerializerOptions(_jsonOptions)
            {
                WriteIndented = true
            }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    public static GridRenderer? FromJson(string json)
    {
        return JsonSerializer.Deserialize<GridRenderer>(json, _jsonOptions);
    }

    public static bool TryFromJson(string json, out GridRenderer? value)
    {
        try
        {
            value = JsonSerializer.Deserialize<GridRenderer>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}