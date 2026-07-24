using System;
using System.Text.Json;

namespace SkiaScope;

/// <summary>
/// Provides JSON serialization and deserialization extension methods for <see cref="Fft"/> instances.
/// </summary>
/// <remarks>
/// This class uses the shared <see cref="JsonRendererExtensions.JsonOptions"/> contract
/// for consistent serialization behavior across all renderer types.
/// </remarks>
public static class FftJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = JsonRendererExtensions.JsonOptions;

    /// <summary>
    /// Serializes the <see cref="Fft"/> instance to JSON.
    /// </summary>
    /// <param name="value">The <see cref="Fft"/> instance to serialize.</param>
    /// <param name="indented">If <see langword="true"/>, the output JSON will be indented.</param>
    /// <returns>A JSON string representing the <see cref="Fft"/> instance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    public static string ToJson(this Fft value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? JsonRendererExtensions.CreateOptions(true)
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string into an <see cref="Fft"/> instance and validates the result.
    /// </summary>
    /// <param name="json">The JSON string.</param>
    /// <returns>The deserialized and validated <see cref="Fft"/> instance, or <c>null</c> if the JSON is empty.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The deserialized object failed validation.</exception>
    /// <exception cref="JsonException">The JSON is invalid.</exception>
    public static Fft? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        var result = JsonSerializer.Deserialize<Fft>(json, _jsonOptions);
        JsonRendererExtensions.Validate(result);
        return result;
    }

    /// <summary>
    /// Attempts to deserialize a JSON string into an <see cref="Fft"/> instance and validates the result.
    /// </summary>
    /// <param name="json">The JSON string.</param>
    /// <param name="value">
    /// When this method returns, contains the deserialized and validated <see cref="Fft"/> instance
    /// if the operation succeeded; otherwise, <c>null</c>.
    /// </param>
    /// <returns><see langword="true"/> if deserialization and validation succeeded; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/>.</exception>
    public static bool TryFromJson(string json, out Fft? value)
    {
        ArgumentNullException.ThrowIfNull(json);

        try
        {
            value = JsonSerializer.Deserialize<Fft>(json, _jsonOptions);
            JsonRendererExtensions.Validate(value);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
        catch (Exception)
        {
            value = null;
            return false;
        }
    }
}
