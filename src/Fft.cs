using System;
using System.Text.Json.Serialization;

namespace SkiaScope;

/// <summary>
/// Specifies the window function to apply before FFT computation.
/// </summary>
public enum WindowFunction
{
    /// <summary>
    /// Rectangular (no windowing) - preserves all signal energy but may have high spectral leakage.
    /// </summary>
    Rectangular = 0,

    /// <summary>
    /// Hann window - good general-purpose window with moderate spectral leakage reduction.
    /// </summary>
    Hann = 1,

    /// <summary>
    /// 4-term Blackman-Harris window - excellent spectral leakage reduction for spectrograms.
    /// </summary>
    BlackmanHarris = 2
}

/// <summary>
/// Computes the frequency spectrum of real-valued audio buffers using an
/// iterative radix-2 Cooley-Tukey Fast Fourier Transform.
/// </summary>
public sealed class Fft
{
    private readonly float[] _realBuffer;
    private readonly float[] _imaginaryBuffer;
    private readonly float[] _windowBuffer;
    private readonly float[] _twiddleReal;
    private readonly float[] _twiddleImag;

    /// <summary>
    /// Initializes a new instance of the <see cref="Fft"/> class.
    /// </summary>
    /// <param name="size">
    /// The number of samples processed per transform. Must be a positive power of two.
    /// </param>
    /// <exception cref="ArgumentException"><paramref name="size"/> is not a positive power of two.</exception>
    [JsonConstructor]
    public Fft(int size = 1024)
    {
        if (size <= 0 || (size & (size - 1)) != 0)
        {
            throw new ArgumentException("Size must be a positive power of two", nameof(size));
        }

        Size = size;

        // Pre-allocate reusable buffers to avoid allocations per call
        _realBuffer = new float[size];
        _imaginaryBuffer = new float[size];
        _windowBuffer = new float[size];

        // Precompute twiddle factors for all butterfly stages
        _twiddleReal = new float[size];
        _twiddleImag = new float[size];
        PrecomputeTwiddleFactors();
    }

    /// <summary>
    /// Gets the number of samples processed per transform.
    /// </summary>
    public int Size { get; }

    /// <summary>
    /// Computes the magnitude spectrum for the given audio samples.
    /// </summary>
    /// <param name="samples">
    /// The input samples. If shorter than <see cref="Size"/> the remainder is zero-padded;
    /// if longer, only the first <see cref="Size"/> samples are used.
    /// </param>
    /// <param name="window">
    /// The window function to apply before transformation. Defaults to <see cref="WindowFunction.Hann"/>
    /// </param>
    /// <returns>
    /// An array of length <c>Size / 2 + 1</c> containing the magnitude of each
    /// non-negative frequency bin (bin 0 is DC, the last bin is Nyquist).
    /// </returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="samples"/> is empty (length 0).
    /// </exception>
    public float[] ComputeMagnitudeSpectrum(ReadOnlySpan<float> samples, WindowFunction window = WindowFunction.Hann)
    {
        if (samples.Length == 0)
        {
            throw new ArgumentException("Input samples cannot be empty", nameof(samples));
        }

        var magnitudes = new float[Size / 2 + 1];
        ComputeMagnitudeSpectrum(samples, magnitudes, window);

        return magnitudes;
    }

    /// <summary>
    /// Computes the magnitude spectrum and writes results into the provided output span.
    /// </summary>
    /// <param name="samples">
    /// The input samples. If shorter than <see cref="Size"/> the remainder is zero-padded;
    /// if longer, only the first <see cref="Size"/> samples are used.
    /// </param>
    /// <param name="magnitudes">
    /// The span to write magnitude results into. Must have length of at least <c>Size / 2 + 1</c>.
    /// </param>
    /// <param name="window">
    /// The window function to apply before transformation. Defaults to <see cref="WindowFunction.Hann"/>
    /// </param>
    /// <exception cref="ArgumentException">
    /// <paramref name="samples"/> is empty (length 0) or <paramref name="magnitudes"/> is too small.
    /// </exception>
    public void ComputeMagnitudeSpectrum(ReadOnlySpan<float> samples, Span<float> magnitudes, WindowFunction window = WindowFunction.Hann)
    {
        if (samples.Length == 0)
        {
            throw new ArgumentException("Input samples cannot be empty", nameof(samples));
        }

        if (magnitudes.Length < Size / 2 + 1)
        {
            throw new ArgumentOutOfRangeException(nameof(magnitudes), $"Span must have length at least {Size / 2 + 1}");
        }

        // Apply window function to input samples
        ApplyWindow(samples, _realBuffer.AsSpan(0, Size), window);

        // Perform FFT in-place on the real buffer (imaginary starts as zero)
        Transform(_realBuffer, _imaginaryBuffer);

        // Compute magnitudes
        for (int i = 0; i <= Size / 2; i++)
        {
            magnitudes[i] = MathF.Sqrt(_realBuffer[i] * _realBuffer[i] + _imaginaryBuffer[i] * _imaginaryBuffer[i]);
        }
    }

    /// <summary>
    /// Performs an in-place iterative radix-2 Cooley-Tukey FFT on the given real and
    /// imaginary components.
    /// </summary>
    /// <param name="real">The real components; overwritten with the transform's real part.</param>
    /// <param name="imaginary">The imaginary components; overwritten with the transform's imaginary part.</param>
    /// <exception cref="ArgumentException">
    /// <paramref name="real"/> or <paramref name="imaginary"/> does not have length equal to <see cref="Size"/>.
    /// </exception>
    public void Transform(Span<float> real, Span<float> imaginary)
    {
        if (real.Length != Size)
        {
            throw new ArgumentException($"Span must have length {Size}", nameof(real));
        }

        if (imaginary.Length != Size)
        {
            throw new ArgumentException($"Span must have length {Size}", nameof(imaginary));
        }

        int n = Size;

        // Bit-reversal permutation.
        for (int i = 1, j = 0; i < n; i++)
        {
            int bit = n >> 1;
            for (; (j & bit) != 0; bit >>= 1)
            {
                j ^= bit;
            }

            j ^= bit;

            if (i < j)
            {
                (real[i], real[j]) = (real[j], real[i]);
                (imaginary[i], imaginary[j]) = (imaginary[j], imaginary[i]);
            }
        }

        // Iterative Cooley-Tukey butterfly using precomputed twiddle factors.
        for (int len = 2; len <= n; len <<= 1)
        {
            int twiddleStep = n / len;

            for (int i = 0; i < n; i += len)
            {
                for (int k = 0; k < len / 2; k++)
                {
                    int evenIdx = i + k;
                    int oddIdx = i + k + (len / 2);

                    float evenReal = real[evenIdx];
                    float evenImag = imaginary[evenIdx];

                    // Use precomputed twiddle factors
                    int twiddleIndex = k * twiddleStep;
                    float wReal = _twiddleReal[twiddleIndex];
                    float wImag = _twiddleImag[twiddleIndex];

                    float oddReal = (real[oddIdx] * wReal) - (imaginary[oddIdx] * wImag);
                    float oddImag = (real[oddIdx] * wImag) + (imaginary[oddIdx] * wReal);

                    real[evenIdx] = evenReal + oddReal;
                    imaginary[evenIdx] = evenImag + oddImag;
                    real[oddIdx] = evenReal - oddReal;
                    imaginary[oddIdx] = evenImag - oddImag;
                }
            }
        }
    }

    /// <summary>
    /// Applies the specified window function to the input samples.
    /// </summary>
    /// <param name="input">The input samples to window.</param>
    /// <param name="output">The span to write windowed samples into.</param>
    /// <param name="window">The window function to apply.</param>
    /// <exception cref="ArgumentException">
    /// <paramref name="input"/> is empty or <paramref name="output"/> is too small.
    /// </exception>
    private void ApplyWindow(ReadOnlySpan<float> input, Span<float> output, WindowFunction window)
    {
        if (input.Length == 0)
        {
            throw new ArgumentException("Input cannot be empty", nameof(input));
        }

        if (output.Length < Math.Min(input.Length, Size))
        {
            throw new ArgumentOutOfRangeException(nameof(output), $"Span must have length at least {Math.Min(input.Length, Size)}");
        }

        int copyCount = Math.Min(input.Length, Size);

        switch (window)
        {
            case WindowFunction.Rectangular:
                // No windowing - rectangular window
                input[..copyCount].CopyTo(output[..copyCount]);
                break;

            case WindowFunction.Hann:
                // Hann window: w(n) = 0.5 * (1 - cos(2πn/(N-1)) for n = 0 to N-1
                for (int i = 0; i < copyCount; i++)
                {
                    float normalizedIndex = i / (float)Math.Max(copyCount - 1, 1);
                    _windowBuffer[i] = 0.5f - 0.5f * MathF.Cos(2f * MathF.PI * normalizedIndex);
                    output[i] = input[i] * _windowBuffer[i];
                }
                break;

            case WindowFunction.BlackmanHarris:
                // 4-term Blackman-Harris window:
                // w(n) = a0 - a1*cos(2πn/(N-1)) + a2*cos(4πn/(N-1)) - a3*cos(6πn/(N-1))
                // where a0 = 0.35875, a1 = 0.48829, a2 = 0.14128, a3 = 0.01168
                const float a0 = 0.35875f;
                const float a1 = 0.48829f;
                const float a2 = 0.14128f;
                const float a3 = 0.01168f;

                for (int i = 0; i < copyCount; i++)
                {
                    float normalizedIndex = i / (float)Math.Max(copyCount - 1, 1);
                    float angle = 2f * MathF.PI * normalizedIndex;
                    _windowBuffer[i] = a0 -
                                      a1 * MathF.Cos(angle) +
                                      a2 * MathF.Cos(2f * angle) -
                                      a3 * MathF.Cos(3f * angle);
                    output[i] = input[i] * _windowBuffer[i];
                }
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(window), window, "Unsupported window function");
        }

        // Zero-pad the remainder if input is shorter than Size
        if (copyCount < Size)
        {
            output[copyCount..Size].Clear();
        }
    }

    /// <summary>
    /// Precomputes twiddle factors for all butterfly stages.
    /// </summary>
    private void PrecomputeTwiddleFactors()
    {
        int n = Size;

        for (int len = 2; len <= n; len <<= 1)
        {
            int twiddleStep = n / len;
            float angle = -2f * MathF.PI / len;

            for (int k = 0; k < len / 2; k++)
            {
                int twiddleIndex = k * twiddleStep;
                _twiddleReal[twiddleIndex] = MathF.Cos(angle * k);
                _twiddleImag[twiddleIndex] = MathF.Sin(angle * k);
            }
        }
    }

    /// <summary>
    /// Gets the window function coefficients for the specified window type.
    /// </summary>
    /// <param name="window">The window function type.</param>
    /// <returns>An array of window coefficients.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown if <paramref name="window"/> is not a valid <see cref="WindowFunction"/> value.
    /// </exception>
    public float[] GetWindowCoefficients(WindowFunction window)
    {
        var coefficients = new float[Size];

        switch (window)
        {
            case WindowFunction.Rectangular:
                for (int i = 0; i < Size; i++)
                {
                    coefficients[i] = 1.0f;
                }
                break;

            case WindowFunction.Hann:
                for (int i = 0; i < Size; i++)
                {
                    float normalizedIndex = i / (float)Math.Max(Size - 1, 1);
                    coefficients[i] = 0.5f - 0.5f * MathF.Cos(2f * MathF.PI * normalizedIndex);
                }
                break;

            case WindowFunction.BlackmanHarris:
                const float a0 = 0.35875f;
                const float a1 = 0.48829f;
                const float a2 = 0.14128f;
                const float a3 = 0.01168f;

                for (int i = 0; i < Size; i++)
                {
                    float normalizedIndex = i / (float)Math.Max(Size - 1, 1);
                    float angle = 2f * MathF.PI * normalizedIndex;
                    coefficients[i] = a0 -
                                      a1 * MathF.Cos(angle) +
                                      a2 * MathF.Cos(2f * angle) -
                                      a3 * MathF.Cos(3f * angle);
                }
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(window), window, "Unsupported window function");
        }

        return coefficients;
    }
}
