using System;
using System.Text.Json.Serialization;

namespace SkiaScope;

/// <summary>
/// Specifies the window function to apply before FFT computation.
/// </summary>
public enum FftWindow
{
    /// <summary>
    /// No windowing (rectangular window) - preserves all signal energy but may have high spectral leakage.
    /// </summary>
    None = 0,

    /// <summary>
    /// Hann window - good general-purpose window with moderate spectral leakage reduction.
    /// </summary>
    Hann = 1,

    /// <summary>
    /// Hamming window - similar to Hann but with different coefficients for better side-lobe suppression.
    /// </summary>
    Hamming = 2
}

/// <summary>
/// Computes the frequency spectrum of real-valued audio buffers using an
/// iterative radix-2 Cooley-Tukey Fast Fourier Transform.
/// </summary>
public sealed class Fft
{
    private readonly float[] _realBuffer;
    private readonly float[] _imaginaryBuffer;
    private readonly float[] _windowCoefficients;
    private readonly float[] _twiddleReal;
    private readonly float[] _twiddleImag;
    private readonly FftWindow _windowType;

    /// <summary>
    /// Initializes a new instance of the <see cref="Fft"/> class.
    /// </summary>
    /// <param name="size">
    /// The number of samples processed per transform. Must be a positive power of two.
    /// </param>
    /// <param name="window">
    /// The window function to apply before FFT computation. Defaults to <see cref="FftWindow.Hann"/>.
    /// </param>
    /// <exception cref="ArgumentException"><paramref name="size"/> is not a positive power of two.</exception>
    [JsonConstructor]
    public Fft(int size = 1024, FftWindow window = FftWindow.Hann)
    {
        if (size <= 0 || (size & (size - 1)) != 0)
        {
            throw new ArgumentException("Size must be a positive power of two", nameof(size));
        }

        Size = size;
        _windowType = window;

        // Pre-allocate reusable buffers to avoid allocations per call
        _realBuffer = new float[size];
        _imaginaryBuffer = new float[size];
        _windowCoefficients = new float[size];

        // Precompute window coefficients once
        PrecomputeWindowCoefficients();

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
    /// <returns>
    /// An array of length <c>Size / 2 + 1</c> containing the magnitude of each
    /// non-negative frequency bin (bin 0 is DC, the last bin is Nyquist).
    /// </returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="samples"/> is empty (length 0).
    /// </exception>
    public float[] ComputeMagnitudeSpectrum(ReadOnlySpan<float> samples)
    {
        if (samples.Length == 0)
        {
            throw new ArgumentException("Input samples cannot be empty", nameof(samples));
        }

        var magnitudes = new float[Size / 2 + 1];
        ComputeMagnitudeSpectrum(samples, magnitudes);

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
    /// <exception cref="ArgumentException">
    /// <paramref name="samples"/> is empty (length 0) or <paramref name="magnitudes"/> is too small.
    /// </exception>
    public void ComputeMagnitudeSpectrum(ReadOnlySpan<float> samples, Span<float> magnitudes)
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
        ApplyWindow(samples, _realBuffer.AsSpan(0, Size));

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
    /// Applies the window function to the input samples.
    /// </summary>
    /// <param name="input">The input samples to window.</param>
    /// <param name="output">The span to write windowed samples into.</param>
    /// <exception cref="ArgumentException">
    /// <paramref name="input"/> is empty or <paramref name="output"/> is too small.
    /// </exception>
    private void ApplyWindow(ReadOnlySpan<float> input, Span<float> output)
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

        // Apply precomputed window coefficients
        for (int i = 0; i < copyCount; i++)
        {
            output[i] = input[i] * _windowCoefficients[i];
        }

        // Zero-pad the remainder if input is shorter than Size
        if (copyCount < Size)
        {
            output[copyCount..Size].Clear();
        }
    }

    /// <summary>
    /// Precomputes window coefficients for the configured window type.
    /// </summary>
    private void PrecomputeWindowCoefficients()
    {
        switch (_windowType)
        {
            case FftWindow.None:
                // Rectangular window - all coefficients are 1.0
                for (int i = 0; i < Size; i++)
                {
                    _windowCoefficients[i] = 1.0f;
                }
                break;

            case FftWindow.Hann:
                // Hann window: w(n) = 0.5 * (1 - cos(2πn/(N-1)) for n = 0 to N-1
                for (int i = 0; i < Size; i++)
                {
                    float normalizedIndex = i / (float)Math.Max(Size - 1, 1);
                    _windowCoefficients[i] = 0.5f - 0.5f * MathF.Cos(2f * MathF.PI * normalizedIndex);
                }
                break;

            case FftWindow.Hamming:
                // Hamming window: w(n) = 0.54 - 0.46 * cos(2πn/(N-1))
                for (int i = 0; i < Size; i++)
                {
                    float normalizedIndex = i / (float)Math.Max(Size - 1, 1);
                    _windowCoefficients[i] = 0.54f - 0.46f * MathF.Cos(2f * MathF.PI * normalizedIndex);
                }
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(_windowType), _windowType, "Unsupported window function");
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
    /// Thrown if <paramref name="window"/> is not a valid <see cref="FftWindow"/> value.
    /// </exception>
    public float[] GetWindowCoefficients(FftWindow window)
    {
        var coefficients = new float[Size];

        switch (window)
        {
            case FftWindow.None:
                for (int i = 0; i < Size; i++)
                {
                    coefficients[i] = 1.0f;
                }
                break;

            case FftWindow.Hann:
                for (int i = 0; i < Size; i++)
                {
                    float normalizedIndex = i / (float)Math.Max(Size - 1, 1);
                    coefficients[i] = 0.5f - 0.5f * MathF.Cos(2f * MathF.PI * normalizedIndex);
                }
                break;

            case FftWindow.Hamming:
                for (int i = 0; i < Size; i++)
                {
                    float normalizedIndex = i / (float)Math.Max(Size - 1, 1);
                    coefficients[i] = 0.54f - 0.46f * MathF.Cos(2f * MathF.PI * normalizedIndex);
                }
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(window), window, "Unsupported window function");
        }

        return coefficients;
    }
}