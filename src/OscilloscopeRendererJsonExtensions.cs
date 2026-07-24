using System;
using System.Text.Json;

namespace SkiaScope;

/// <summary>
/// Provides JSON serialization and deserialization extensions for <see cref="OscilloscopeRenderer"/>.
/// </summary>
/// <remarks>
/// This class uses the shared <see cref="JsonRendererExtensions.JsonOptions"/> contract
/// for consistent serialization behavior across all renderer types.
/// </remarks>
public static class OscilloscopeRendererJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = JsonRendererExtensions.JsonOptions;

    /// <summary>
    /// Validates numeric ranges in an OscilloscopeRenderer instance to prevent NaN/Infinity or out-of-range values.
    /// </summary>
    /// <param name="renderer">The renderer to validate.</param>
    /// <exception cref="ArgumentException">Thrown if numeric values are invalid (NaN, Infinity, or out of range).</exception>
    private static void ValidateOscilloscopeRendererNumericValues(OscilloscopeRenderer renderer)
    {
        // Validate point count
        if (renderer.PointCount < 64 || renderer.PointCount > 8192)
        {
            throw new ArgumentException(
                $"PointCount is out of valid range [64, 8192]. Actual: {renderer.PointCount}",
                nameof(renderer));
        }

        // Validate line width
        if (float.IsNaN(renderer.LineWidth) || float.IsInfinity(renderer.LineWidth) || renderer.LineWidth < 0.5f || renderer.LineWidth > 10.0f)
        {
            throw new ArgumentException(
                $"LineWidth is invalid. Must be between 0.5 and 10.0. Actual: {renderer.LineWidth}",
                nameof(renderer));
        }

        // Validate alpha falloff
        if (float.IsNaN(renderer.AlphaFalloff) || float.IsInfinity(renderer.AlphaFalloff) || renderer.AlphaFalloff < 0.9f || renderer.AlphaFalloff > 0.999f)
        {
            throw new ArgumentException(
                $"AlphaFalloff is out of valid range [0.9, 0.999]. Actual: {renderer.AlphaFalloff}",
                nameof(renderer));
        }

        // Validate persistence amount
        if (float.IsNaN(renderer.PersistenceAmount) || float.IsInfinity(renderer.PersistenceAmount) || renderer.PersistenceAmount < 0.0f || renderer.PersistenceAmount > 1.0f)
        {
            throw new ArgumentException(
                $"PersistenceAmount is out of valid range [0.0, 1.0]. Actual: {renderer.PersistenceAmount}",
                nameof(renderer));
        }

        // Validate edge threshold
        if (float.IsNaN(renderer.EdgeThreshold) || float.IsInfinity(renderer.EdgeThreshold) || renderer.EdgeThreshold < -1.0f || renderer.EdgeThreshold > 1.0f)
        {
            throw new ArgumentException(
                $"EdgeThreshold is out of valid range [-1.0, 1.0]. Actual: {renderer.EdgeThreshold}",
                nameof(renderer));
        }

        // Validate edge hysteresis
        if (float.IsNaN(renderer.EdgeHysteresis) || float.IsInfinity(renderer.EdgeHysteresis) || renderer.EdgeHysteresis < 0.0f || renderer.EdgeHysteresis > 0.5f)
        {
            throw new ArgumentException(
                $"EdgeHysteresis is out of valid range [0.0, 0.5]. Actual: {renderer.EdgeHysteresis}",
                nameof(renderer));
        }

        // Validate edge holdoff samples
        if (renderer.EdgeHoldoffSamples < 0)
        {
            throw new ArgumentException(
                $"EdgeHoldoffSamples cannot be negative. Actual: {renderer.EdgeHoldoffSamples}",
                nameof(renderer));
        }

        // Validate sample rate
        if (float.IsNaN(renderer.SampleRate) || float.IsInfinity(renderer.SampleRate))
        {
            throw new ArgumentException(
                $"SampleRate contains invalid numeric value: {renderer.SampleRate}. NaN and Infinity are not allowed.",
                nameof(renderer));
        }

        // Validate theme numeric values
        if (renderer.Theme != null)
        {
            if (float.IsNaN(renderer.Theme.GridThickness) || float.IsInfinity(renderer.Theme.GridThickness))
            {
                throw new ArgumentException(
                    $"Theme.GridThickness contains invalid numeric value: {renderer.Theme.GridThickness}",
                    nameof(renderer));
            }

            if (float.IsNaN(renderer.Theme.FontSize) || float.IsInfinity(renderer.Theme.FontSize))
            {
                throw new ArgumentException(
                    $"Theme.FontSize contains invalid numeric value: {renderer.Theme.FontSize}",
                    nameof(renderer));
            }
        }

        // Validate ring buffer capacities
        if (renderer._xBuffer != null && (renderer._xBuffer.Capacity < 64 || renderer._xBuffer.Capacity > 8192))
        {
            throw new ArgumentException(
                $"RingBuffer capacity is out of valid range [64, 8192]. Actual: {renderer._xBuffer.Capacity}",
                nameof(renderer));
        }
    }

    /// <summary>
    /// Serializes the <see cref="OscilloscopeRenderer"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The oscilloscope renderer to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the renderer.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/></exception>
    public static string ToJson(this OscilloscopeRenderer value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? JsonRendererExtensions.CreateOptions(true)
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to an <see cref="OscilloscopeRenderer"/> instance and validates the result.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized and validated <see cref="OscilloscopeRenderer"/> instance, or <see langword="null"/> if deserialization fails.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentException">The deserialized object failed validation.</exception>
    /// <exception cref="JsonException">The JSON is invalid.</exception>
    public static OscilloscopeRenderer? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        try
        {
            var result = JsonSerializer.Deserialize<OscilloscopeRenderer>(json, _jsonOptions);
            if (result != null)
            {
                ValidateOscilloscopeRendererNumericValues(result);
            }
            JsonRendererExtensions.Validate(result);
            return result;
        }
        catch (JsonException)
        {
            return null;
        }
        catch (ArgumentException)
        {
            throw;
        }
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to an <see cref="OscilloscopeRenderer"/> instance and validates the result.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">
    /// Receives the deserialized and validated <see cref="OscilloscopeRenderer"/> instance if successful; otherwise, <see langword="null"/>.
    /// </param>
    /// <returns><see langword="true"/> if deserialization and validation succeed; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/></exception>
    public static bool TryFromJson(string json, out OscilloscopeRenderer? value)
    {
        ArgumentNullException.ThrowIfNull(json);

        try
        {
            value = JsonSerializer.Deserialize<OscilloscopeRenderer>(json, _jsonOptions);
            if (value != null)
            {
                ValidateOscilloscopeRendererNumericValues(value);
            }
            JsonRendererExtensions.Validate(value);
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
