using System.Text.Json;

public static class FftJsonExtensions
{
    /// <summary>
    /// Cached JsonSerializerOptions with camelCase naming.
    /// </summary>
    private static readonly JsonSerializerOptions _options = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Serializes the <see cref="Fft"/> instance to JSON.
    /// </summary>
    /// <param name="value">The Fft instance to serialize.</param>
    /// <param name="indented">If true, the output JSON will be indented.</param>
    /// <returns>A JSON string representing the Fft instance.</returns>
    /// <remarks>
    /// If indentation is requested, a new JsonSerializerOptions instance is created with the cached options and indentation enabled.
    /// </remarks>
    public static string ToJson(this Fft value, bool indented = false)
    {
        // If indentation is requested, clone the cached options and enable indentation.
        var options = indented
            ? new JsonSerializerOptions(_options) { WriteIndented = true }
            : _options;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string into an <see cref="Fft"/> instance.
    /// </summary>
    /// <param name="json">The JSON string.</param>
    /// <returns>The deserialized <see cref="Fft"/> instance, or null if the JSON is empty.</returns>
    public static Fft? FromJson(string json)
    {
        return JsonSerializer.Deserialize<Fft>(json, _options);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string into an <see cref="Fft"/> instance.
    /// </summary>
    /// <param name="json">The JSON string.</param>
    /// <param name="value">When this method returns, contains the deserialized <see cref="Fft"/> instance if the operation succeeded; otherwise, null.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    public static bool TryFromJson(string json, out Fft? value)
    {
        try
        {
            value = JsonSerializer.Deserialize<Fft>(json, _options);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}
