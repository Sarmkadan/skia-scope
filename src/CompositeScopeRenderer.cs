using System;
using System.Collections.Generic;
using System.Linq;
using SkiaSharp;

namespace SkiaScope;

/// <summary>
/// A composite renderer that manages multiple child renderers, rendering them in sequence
/// and propagating theme and sample rate changes.
/// </summary>
public sealed class CompositeScopeRenderer : IScopeRenderer
{
    private sealed class Layer
    {
        public IScopeRenderer Renderer { get; }
        public bool IsEnabled { get; set; } = true;

        public Layer(IScopeRenderer renderer)
        {
            Renderer = renderer;
        }
    }

    private readonly List<Layer> _layers = new();
    private ScopeTheme _theme;
    private int _sampleRate;

    /// <summary>
    /// Initializes a new instance of the <see cref="CompositeScopeRenderer"/> class.
    /// </summary>
    /// <param name="theme">The initial theme to apply to this renderer and all its children.</param>
    /// <exception cref="ArgumentNullException">Thrown if theme is null.</exception>
    public CompositeScopeRenderer(ScopeTheme theme)
    {
        _theme = theme ?? throw new ArgumentNullException(nameof(theme));
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException">Thrown if value is null.</exception>
    public ScopeTheme Theme
    {
        get => _theme;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            _theme = value;
            foreach (var layer in _layers)
            {
                layer.Renderer.Theme = _theme;
            }
        }
    }

    /// <inheritdoc/>
    public int SampleRate
    {
        get => _sampleRate;
        set
        {
            _sampleRate = value;
            foreach (var layer in _layers)
            {
                layer.Renderer.SampleRate = _sampleRate;
            }
        }
    }

    /// <inheritdoc/>
    public void PushSamples(ReadOnlySpan<float> samples)
    {
        foreach (var layer in _layers.Where(l => l.IsEnabled))
        {
            layer.Renderer.PushSamples(samples);
        }
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException">Thrown if canvas is null.</exception>
    public void Render(SKCanvas canvas, SKRect bounds)
    {
        ArgumentNullException.ThrowIfNull(canvas);

        foreach (var layer in _layers.Where(l => l.IsEnabled))
        {
            layer.Renderer.Render(canvas, bounds);
        }
    }

    /// <summary>
    /// Adds a renderer as a new layer.
    /// </summary>
    /// <param name="renderer">The renderer to add.</param>
    /// <param name="enabled">Whether the layer is enabled by default.</param>
    /// <exception cref="ArgumentNullException">Thrown if renderer is null.</exception>
    /// <exception cref="ArgumentException">Thrown if renderer is this composite renderer.</exception>
    public void AddRenderer(IScopeRenderer renderer, bool enabled = true)
    {
        ArgumentNullException.ThrowIfNull(renderer);
        if (ReferenceEquals(renderer, this))
        {
            throw new ArgumentException("A composite renderer cannot be added to itself.", nameof(renderer));
        }

        renderer.Theme = _theme;
        renderer.SampleRate = _sampleRate;
        _layers.Add(new Layer(renderer) { IsEnabled = enabled });
    }

    /// <summary>
    /// Sets the enabled state of a specific renderer.
    /// </summary>
    /// <param name="renderer">The renderer whose state to change.</param>
    /// <param name="enabled">The new enabled state.</param>
    /// <exception cref="ArgumentNullException">Thrown if renderer is null.</exception>
    public void SetEnabled(IScopeRenderer renderer, bool enabled)
    {
        ArgumentNullException.ThrowIfNull(renderer);
        var layer = _layers.FirstOrDefault(l => l.Renderer == renderer);
        if (layer != null)
        {
            layer.IsEnabled = enabled;
        }
    }

    /// <summary>
    /// Moves a renderer to a new index in the z-order (rendering order).
    /// </summary>
    /// <param name="renderer">The renderer to move.</param>
    /// <param name="newIndex">The target index.</param>
    /// <exception cref="ArgumentNullException">Thrown if renderer is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if newIndex is invalid.</exception>
    public void MoveRenderer(IScopeRenderer renderer, int newIndex)
    {
        ArgumentNullException.ThrowIfNull(renderer);
        if (newIndex < 0 || newIndex >= _layers.Count)
        {
            throw new ArgumentOutOfRangeException(
                nameof(newIndex),
                newIndex,
                $"Layer index must be between 0 and {_layers.Count - 1}.");
        }

        var layer = _layers.FirstOrDefault(l => l.Renderer == renderer);
        if (layer != null)
        {
            _layers.Remove(layer);
            _layers.Insert(newIndex, layer);
        }
    }
}
