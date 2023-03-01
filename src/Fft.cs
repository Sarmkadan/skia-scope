using System;
using System.Numerics;

namespace SkiaScope;

public static class Fft
{
    public static void Forward(float[] real, float[] imag)
    {
        if (real.Length != imag.Length)
        {
            throw new ArgumentException("Real and imag arrays must be of equal length");
        }

        int n = real.Length;
        if ((n & (n - 1)) != 0)
        {
            throw new ArgumentException("Length of real and imag arrays must be a power of 2");
        }

        int m = (int)Math.Log2(n);
        for (int s = 1; s <= m; s++)
        {
            int n2 = 1 << s;
            float w = 1;
            float twiddle_r = (float)Math.Cos(-2 * Math.PI / n2);
            float twiddle_i = (float)Math.Sin(-2 * Math.PI / n2);

            for (int i = 0; i < n; i += n2)
            {
                for (int j = i; j < i + (n2 >> 1); j++)
                {
                    float t_r = real[j + (n2 >> 1)] * w - imag[j + (n2 >> 1)] * w * twiddle_i;
                    float t_i = real[j + (n2 >> 1)] * w * twiddle_i + imag[j + (n2 >> 1)] * w;

                    real[j + (n2 >> 1)] = real[j] - t_r;
                    imag[j + (n2 >> 1)] = imag[j] - t_i;
                    real[j] += t_r;
                    imag[j] += t_i;

                    w *= twiddle_r;
                    w = w + w * twiddle_i * twiddle_i;
                    w = w - w * twiddle_r * twiddle_r;
                }
            }
        }

        // Normalize
        float invSqrtN = 1 / (float)Math.Sqrt(n);
        for (int i = 0; i < n; i++)
        {
            real[i] *= invSqrtN;
            imag[i] *= invSqrtN;
        }
    }

    public static float[] MagnitudesDb(ReadOnlySpan<float> samples, float[] window)
    {
        if (window.Length != samples.Length)
        {
            throw new ArgumentException("Window and samples arrays must be of equal length");
        }

        float[] real = new float[samples.Length];
        float[] imag = new float[samples.Length];

        for (int i = 0; i < samples.Length; i++)
        {
            real[i] = samples[i] * window[i];
            imag[i] = 0;
        }

        Forward(real, imag);

        float[] magnitudes = new float[samples.Length / 2];
        for (int i = 0; i < samples.Length / 2; i++)
        {
            float magnitude = (float)Math.Sqrt(real[i] * real[i] + imag[i] * imag[i]);
            magnitudes[i] = 20 * (float)Math.Log10(magnitude);
        }

        return magnitudes;
    }

    public static float[] HannWindow(int size)
    {
        float[] window = new float[size];
        for (int i = 0; i < size; i++)
        {
            window[i] = 0.5f * (1 - (float)Math.Cos(2 * Math.PI * i / (size - 1)));
        }
        return window;
    }
}
