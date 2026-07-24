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
    /// Validates numeric ranges in an Fft instance to prevent NaN/Infinity or out-of-range values.
    /// </summary>
    /// <param name="fft">The FFT instance to validate.</param>
    /// <exception cref="ArgumentException">Thrown if numeric values are invalid (NaN, Infinity, or out of range).</exception>
    private static void ValidateFftNumericValues(Fft fft)
    {
        // Size is already validated in FromJson/TryFromJson
        // No other numeric properties to validate in Fft class
    }

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
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentException">
    /// The deserialized object failed validation or the Size parameter is out of valid range.
    /// </exception>
    /// <exception cref="JsonException">The JSON is invalid.</exception>
    public static Fft? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        var result = JsonSerializer.Deserialize<Fft>(json, _jsonOptions);

        if (result != null)
        {
            const int MaxFftSize = 1 << 20; // 1 MB worth of float samples (1M samples * 4 bytes)
            if (result.Size <= 0 || result.Size > MaxFftSize)
            {
                throw new ArgumentException(
                    $"FFT size must be a positive power of two between 1 and {MaxFftSize}. Actual: {result.Size}",
                    nameof(result));
            }

            // Validate that Size is actually a power of two
            if ((result.Size & (result.Size - 1)) != 0)
            {
                throw new ArgumentException(
                    $"FFT size must be a power of two. Actual: {result.Size}",
                    nameof(result));
            }

            JsonRendererExtensions.Validate(result);
        }

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
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/></exception>
    public static bool TryFromJson(string json, out Fft? value)
    {
        ArgumentNullException.ThrowIfNull(json);

        try
        {
            value = JsonSerializer.Deserialize<Fft>(json, _jsonOptions);

            if (value != null)
            {
                const int MaxFftSize = 1 << 20; // 1 MB worth of float samples (1M samples * 4 bytes)
                if (value.Size <= 0 || value.Size > MaxFftSize)
                {
                    value = null;
                    return false;
                }

                // Validate that Size is actually a power of two
                if ((value.Size & (value.Size - 1)) != 0)
                {
                    value = null;
                    return false;
                }

                JsonRendererExtensions.Validate(value);
            }

            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
        catch (ArgumentException)
        {
            value = null;
            return false;
        }
    }
}
