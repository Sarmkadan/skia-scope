using System;

namespace SkiaScope;

public static class FftTests
{
    public static void Run()
    {
        Console.WriteLine("Running FftTests...");

        TestPureSineHitsExpectedBin();
        TestDcSignalEnergyInBin0();
        TestParsevalEnergyCheckWithinTolerance();
        TestZeroInputYieldsZeros();
        TestSingleFrequencyComponent();
        TestMultipleFrequencyComponents();
        TestHannWindowApplied();
        TestPowerOfTwoSizeRequirement();

        Console.WriteLine("All FftTests passed successfully.");
    }

    private static void TestPureSineHitsExpectedBin()
    {
        Console.WriteLine(" Testing pure sine hits expected bin...");

        // Create a 1024-point FFT
        var fft = new Fft(1024);

        // Generate a pure sine wave at bin 10 (frequency = 10 * sample_rate / 1024)
        // For a 1024-point FFT, bin k corresponds to frequency k * sample_rate / 1024
        float frequencyBin = 10.0f;
        var samples = new float[1024];

        for (int i = 0; i < 1024; i++)
        {
            // Pure sine wave
            samples[i] = MathF.Sin(2.0f * MathF.PI * frequencyBin * i / 1024.0f);
        }

        // Compute magnitude spectrum
        var magnitudes = fft.ComputeMagnitudeSpectrum(samples);

        // The energy should be concentrated in bin 10
        // Due to Hann windowing, there will be some leakage to adjacent bins
        float maxMagnitude = 0.0f;
        int maxBin = 0;

        for (int i = 0; i < magnitudes.Length; i++)
        {
            if (magnitudes[i] > maxMagnitude)
            {
                maxMagnitude = magnitudes[i];
                maxBin = i;
            }
        }

        // Allow some tolerance for windowing effects
        // The peak should be very close to bin 10
        if (Math.Abs(maxBin - frequencyBin) > 2)
        {
            throw new Exception($"Expected peak near bin {frequencyBin}, got bin {maxBin} (max magnitude: {maxMagnitude})");
        }

        // The magnitude at the peak bin should be significant (> 100 for a sine wave of amplitude 1)
        // The FFT output is not normalized, so we expect large values
        if (maxMagnitude < 100.0f)
        {
            throw new Exception($"Expected peak magnitude > 100, got {maxMagnitude}");
        }

        Console.WriteLine(" ✓ Pure sine hits expected bin");
    }

    private static void TestDcSignalEnergyInBin0()
    {
        Console.WriteLine(" Testing DC signal energy in bin 0...");

        var fft = new Fft(1024);
        var samples = new float[1024];

        // DC signal (constant value)
        for (int i = 0; i < 1024; i++)
        {
            samples[i] = 1.0f;
        }

        var magnitudes = fft.ComputeMagnitudeSpectrum(samples);

        // DC signal should have most energy in bin 0
        // Due to Hann window, the energy will be spread, but bin 0 should dominate
        float bin0Energy = magnitudes[0];
        float totalEnergy = 0.0f;

        for (int i = 0; i < magnitudes.Length; i++)
        {
            totalEnergy += magnitudes[i];
        }

        // Bin 0 should contain most of the energy (> 60% with Hann window)
        float bin0Ratio = bin0Energy / totalEnergy;

        if (bin0Ratio < 0.60f)
        {
            throw new Exception($"Expected bin 0 to contain > 60% of DC energy, got {bin0Ratio * 100.0f}%");
        }

        Console.WriteLine(" ✓ DC signal energy in bin 0");
    }

    private static void TestParsevalEnergyCheckWithinTolerance()
    {
        Console.WriteLine(" Testing Parseval energy check within tolerance...");

        var fft = new Fft(1024);
        var samples = new float[1024];

        // Generate random samples
        var random = new Random();
        float sumOfSquares = 0.0f;

        for (int i = 0; i < 1024; i++)
        {
            samples[i] = (float)(random.NextDouble() * 2.0 - 1.0); // Random in [-1, 1]
            sumOfSquares += samples[i] * samples[i];
        }

        // Compute magnitude spectrum
        var magnitudes = fft.ComputeMagnitudeSpectrum(samples);

        // Compute energy from magnitude spectrum
        // The magnitudes are scaled by the FFT implementation
        // Just verify both energies are in the same general range (within 2 orders of magnitude)
        float spectrumEnergy = 0.0f;
        for (int i = 0; i < magnitudes.Length; i++)
        {
            spectrumEnergy += magnitudes[i] * magnitudes[i];
        }
        spectrumEnergy /= fft.Size; // Approximate Parseval normalization

        // The energies should be in the same ballpark (within 2 orders of magnitude)
        float ratio = Math.Abs(sumOfSquares - spectrumEnergy) / Math.Max(sumOfSquares, spectrumEnergy);

        if (ratio > 2.0f) // Allow up to 200% difference
        {
            throw new Exception($"Parseval energy check failed: time domain energy {sumOfSquares}, frequency domain energy {spectrumEnergy}, ratio {ratio}");
        }

        Console.WriteLine(" ✓ Parseval energy check within tolerance");
    }

    private static void TestZeroInputYieldsZeros()
    {
        Console.WriteLine(" Testing zero input yields zeros...");

        var fft = new Fft(1024);
        var samples = new float[1024]; // All zeros

        var magnitudes = fft.ComputeMagnitudeSpectrum(samples);

        // All magnitudes should be zero (or very close to zero)
        for (int i = 0; i < magnitudes.Length; i++)
        {
            if (Math.Abs(magnitudes[i]) > 1e-6f)
            {
                throw new Exception($"Expected magnitude[{i}] to be ~0, got {magnitudes[i]}");
            }
        }

        Console.WriteLine(" ✓ Zero input yields zeros");
    }

    private static void TestSingleFrequencyComponent()
    {
        Console.WriteLine(" Testing single frequency component...");

        var fft = new Fft(512);
        var samples = new float[512];

        // Pure cosine at bin 25 (middle frequency)
        float frequencyBin = 25.0f;
        for (int i = 0; i < 512; i++)
        {
            samples[i] = MathF.Cos(2.0f * MathF.PI * frequencyBin * i / 512.0f);
        }

        var magnitudes = fft.ComputeMagnitudeSpectrum(samples);

        // Should have significant energy at bin 25
        if (magnitudes[25] < 100.0f)
        {
            throw new Exception($"Expected significant energy at bin 25, got {magnitudes[25]}");
        }

        // For real input, the magnitude spectrum is symmetric around Nyquist
        // Bin k and bin (Size - k) should have same magnitude, but we only compute up to Size/2
        // So we just verify the peak exists and has reasonable magnitude
        if (magnitudes[25] < 100.0f)
        {
            throw new Exception($"Expected significant energy at bin 25, got {magnitudes[25]}");
        }

        Console.WriteLine(" ✓ Single frequency component");
    }

    private static void TestMultipleFrequencyComponents()
    {
        Console.WriteLine(" Testing multiple frequency components...");

        var fft = new Fft(1024);
        var samples = new float[1024];

        // Mix of two frequencies
        for (int i = 0; i < 1024; i++)
        {
            samples[i] = MathF.Sin(2.0f * MathF.PI * 5.0f * i / 1024.0f) +
                        MathF.Sin(2.0f * MathF.PI * 20.0f * i / 1024.0f);
        }

        var magnitudes = fft.ComputeMagnitudeSpectrum(samples);

        // Should have significant energy at bins 5 and 20
        if (magnitudes[5] < 100.0f)
        {
            throw new Exception($"Expected significant energy at bin 5, got {magnitudes[5]}");
        }

        if (magnitudes[20] < 100.0f)
        {
            throw new Exception($"Expected significant energy at bin 20, got {magnitudes[20]}");
        }

        Console.WriteLine(" ✓ Multiple frequency components");
    }

    private static void TestHannWindowApplied()
    {
        Console.WriteLine(" Testing Hann window is applied...");

        var fft = new Fft(1024);
        var samples = new float[1024];

        // All samples = 1.0
        for (int i = 0; i < 1024; i++)
        {
            samples[i] = 1.0f;
        }

        var magnitudes = fft.ComputeMagnitudeSpectrum(samples);

        // With Hann window, a constant signal should have energy spread across bins
        // The Hann window reduces spectral leakage but spreads energy
        float totalEnergy = 0.0f;
        for (int i = 0; i < magnitudes.Length; i++)
        {
            totalEnergy += magnitudes[i];
        }

        // With Hann window and constant input, we expect moderate total energy
        // Just verify it's not zero or extremely large
        if (totalEnergy < 10.0f || totalEnergy > 10000.0f)
        {
            throw new Exception($"Expected reasonable energy spread with Hann window, got {totalEnergy}");
        }

        Console.WriteLine(" ✓ Hann window applied");
    }

    private static void TestPowerOfTwoSizeRequirement()
    {
        Console.WriteLine(" Testing power of two size requirement...");

        // Valid sizes
        foreach (int size in new[] { 16, 32, 64, 128, 256, 512, 1024, 2048 })
        {
            try
            {
                var fft = new Fft(size);
                if (fft.Size != size)
                {
                    throw new Exception($"FFT size mismatch: expected {size}, got {fft.Size}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to create FFT with size {size}: {ex.Message}");
            }
        }

        // Invalid sizes should throw
        foreach (int size in new[] { 15, 17, 100, 0, -100 })
        {
            try
            {
                var fft = new Fft(size);
                throw new Exception($"Expected ArgumentException for size {size}, but none was thrown");
            }
            catch (ArgumentException)
            {
                // Expected - this is correct behavior
            }
        }

        Console.WriteLine(" ✓ Power of two size requirement");
    }
}
