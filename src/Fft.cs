using System;
using System.Text.Json.Serialization;

namespace SkiaScope;

/// <summary>
/// Computes the frequency spectrum of real-valued audio buffers using an
/// iterative radix-2 Cooley-Tukey Fast Fourier Transform.
/// </summary>
public sealed class Fft
{
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
    public float[] ComputeMagnitudeSpectrum(ReadOnlySpan<float> samples)
    {
        var real = new float[Size];
        var imaginary = new float[Size];

        int copyCount = Math.Min(samples.Length, Size);
        int windowSpan = Math.Max(copyCount - 1, 1);
        for (int i = 0; i < copyCount; i++)
        {
            // Apply a Hann window to reduce spectral leakage.
            float window = 0.5f - 0.5f * MathF.Cos(2f * MathF.PI * i / windowSpan);
            real[i] = samples[i] * window;
        }

        Transform(real, imaginary);

        var magnitudes = new float[Size / 2 + 1];
        for (int i = 0; i <= Size / 2; i++)
        {
            magnitudes[i] = MathF.Sqrt(real[i] * real[i] + imaginary[i] * imaginary[i]);
        }

        return magnitudes;
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
        if (real.Length != Size || imaginary.Length != Size)
        {
            throw new ArgumentException($"Both spans must have length {Size}");
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

        // Iterative Cooley-Tukey butterfly.
        for (int len = 2; len <= n; len <<= 1)
        {
            float angle = -2f * MathF.PI / len;
            float wReal = MathF.Cos(angle);
            float wImag = MathF.Sin(angle);

            for (int i = 0; i < n; i += len)
            {
                float curReal = 1f;
                float curImag = 0f;

                for (int k = 0; k < len / 2; k++)
                {
                    int evenIdx = i + k;
                    int oddIdx = i + k + (len / 2);

                    float evenReal = real[evenIdx];
                    float evenImag = imaginary[evenIdx];

                    float oddReal = (real[oddIdx] * curReal) - (imaginary[oddIdx] * curImag);
                    float oddImag = (real[oddIdx] * curImag) + (imaginary[oddIdx] * curReal);

                    real[evenIdx] = evenReal + oddReal;
                    imaginary[evenIdx] = evenImag + oddImag;
                    real[oddIdx] = evenReal - oddReal;
                    imaginary[oddIdx] = evenImag - oddImag;

                    float nextReal = (curReal * wReal) - (curImag * wImag);
                    float nextImag = (curReal * wImag) + (curImag * wReal);
                    curReal = nextReal;
                    curImag = nextImag;
                }
            }
        }
    }
}
