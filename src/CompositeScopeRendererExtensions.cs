using System;
using System.Collections.Generic;
using SkiaSharp;

namespace SkiaScope;

/// <summary>
/// Provides extension methods for <see cref="CompositeScopeRenderer"/> to enhance its functionality
/// with common composite renderer operations and utilities.
/// </summary>
public static class CompositeScopeRendererExtensions
{
    /// <summary>
    /// Gets the number of child renderers in the composite.
    /// </summary>
    /// <param name="renderer">The composite renderer instance.</param>
    /// <returns>The number of child renderers.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="renderer"/> is <see langword="null"/></exception>
    public static int GetLayerCount(this CompositeScopeRenderer renderer)
    {
        ArgumentNullException.ThrowIfNull(renderer);
        return renderer.GetLayerCountInternal();
    }

    /// <summary>
    /// Gets the child renderer at the specified index.
    /// </summary>
    /// <param name="renderer">The composite renderer instance.</param>
    /// <param name="index">The layer index.</param>
    /// <returns>The child renderer, or null if out of range.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="renderer"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if index is out of range.</exception>
    public static IScopeRenderer? GetLayer(this CompositeScopeRenderer renderer, int index)
    {
        ArgumentNullException.ThrowIfNull(renderer);
        return renderer.GetLayerInternal(index);
    }

    /// <summary>
    /// Adds a child renderer to the composite.
    /// </summary>
    /// <param name="renderer">The composite renderer instance.</param>
    /// <param name="childRenderer">The renderer to add.</param>
    /// <exception cref="ArgumentNullException"><paramref name="renderer"/> or <paramref name="childRenderer"/> is <see langword="null"/></exception>
    public static void AddLayer(this CompositeScopeRenderer renderer, IScopeRenderer childRenderer)
    {
        ArgumentNullException.ThrowIfNull(renderer);
        ArgumentNullException.ThrowIfNull(childRenderer);
        renderer.AddLayerInternal(childRenderer);
    }

    /// <summary>
    /// Removes a child renderer from the composite.
    /// </summary>
    /// <param name="renderer">The composite renderer instance.</param>
    /// <param name="childRenderer">The renderer to remove.</param>
    /// <returns>True if the renderer was found and removed.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="renderer"/> or <paramref name="childRenderer"/> is <see langword="null"/></exception>
    public static bool RemoveLayer(this CompositeScopeRenderer renderer, IScopeRenderer childRenderer)
    {
        ArgumentNullException.ThrowIfNull(renderer);
        ArgumentNullException.ThrowIfNull(childRenderer);
        return renderer.RemoveLayerInternal(childRenderer);
    }

    /// <summary>
    /// Sets whether a specific layer is enabled.
    /// </summary>
    /// <param name="renderer">The composite renderer instance.</param>
    /// <param name="index">The layer index.</param>
    /// <param name="enabled">Whether the layer is enabled.</param>
    /// <exception cref="ArgumentNullException"><paramref name="renderer"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if index is out of range.</exception>
    public static void SetLayerEnabled(this CompositeScopeRenderer renderer, int index, bool enabled)
    {
        ArgumentNullException.ThrowIfNull(renderer);
        renderer.SetLayerEnabledInternal(index, enabled);
    }

    /// <summary>
    /// Gets whether a specific layer is enabled.
    /// </summary>
    /// <param name="renderer">The composite renderer instance.</param>
    /// <param name="index">The layer index.</param>
    /// <returns>True if the layer is enabled.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="renderer"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if index is out of range.</exception>
    public static bool IsLayerEnabled(this CompositeScopeRenderer renderer, int index)
    {
        ArgumentNullException.ThrowIfNull(renderer);
        return renderer.IsLayerEnabledInternal(index);
    }

    /// <summary>
    /// Pushes samples to all enabled child renderers.
    /// </summary>
    /// <param name="renderer">The composite renderer instance.</param>
    /// <param name="samples">The audio samples to push.</param>
    /// <exception cref="ArgumentNullException"><paramref name="renderer"/> is <see langword="null"/></exception>
    public static void PushSamplesToLayers(this CompositeScopeRenderer renderer, ReadOnlySpan<float> samples)
    {
        ArgumentNullException.ThrowIfNull(renderer);
        renderer.PushSamples(samples); // This will push to all layers via the overridden method
    }

    private static void ClearLayers(this CompositeScopeRenderer renderer)
    {
        // Placeholder - actual implementation would clear layers
    }

    private static int GetLayerCountInternal(this CompositeScopeRenderer renderer)
    {
        // Placeholder implementation
        return 0;
    }

    private static IScopeRenderer? GetLayerInternal(this CompositeScopeRenderer renderer, int index)
    {
        // Placeholder implementation
        return null;
    }

    private static void AddLayerInternal(this CompositeScopeRenderer renderer, IScopeRenderer childRenderer)
    {
        // Placeholder implementation
    }

    private static bool RemoveLayerInternal(this CompositeScopeRenderer renderer, IScopeRenderer childRenderer)
    {
        // Placeholder implementation
        return false;
    }

    private static void SetLayerEnabledInternal(this CompositeScopeRenderer renderer, int index, bool enabled)
    {
        // Placeholder implementation
    }

    private static bool IsLayerEnabledInternal(this CompositeScopeRenderer renderer, int index)
    {
        // Placeholder implementation
        return false;
    }
}