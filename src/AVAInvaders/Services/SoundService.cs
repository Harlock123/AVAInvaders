using System.Runtime.InteropServices;

namespace AVAInvaders.Services;

public class SoundService : IDisposable
{
    private readonly Dictionary<string, byte[]> _soundBuffers = new();
    private bool _initialized;
    private bool _disposed;

    public bool IsMuted { get; set; }

    public void Initialize()
    {
        if (_initialized) return;

        try
        {
            GenerateSounds();
            _initialized = true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Sound initialization failed: {ex.Message}");
        }
    }

    private void GenerateSounds()
    {
        _soundBuffers["shoot"] = GenerateShootSound();
        _soundBuffers["explosion"] = GenerateExplosionSound();
        _soundBuffers["invaderKilled"] = GenerateInvaderKilledSound();
        _soundBuffers["playerDeath"] = GeneratePlayerDeathSound();
        _soundBuffers["ufo"] = GenerateUfoSound();
        _soundBuffers["march1"] = GenerateMarchSound(180);
        _soundBuffers["march2"] = GenerateMarchSound(220);
        _soundBuffers["march3"] = GenerateMarchSound(260);
        _soundBuffers["march4"] = GenerateMarchSound(300);
    }

    private byte[] GenerateShootSound()
    {
        const int sampleRate = 44100;
        const double duration = 0.15;
        int samples = (int)(sampleRate * duration);
        byte[] buffer = new byte[samples * 2];

        for (int i = 0; i < samples; i++)
        {
            double t = (double)i / sampleRate;
            double envelope = 1.0 - (t / duration);
            double frequency = 1800 - (t / duration) * 1200;
            double sample = Math.Sin(2 * Math.PI * frequency * t) * envelope * 0.5;
            short value = (short)(sample * short.MaxValue);
            buffer[i * 2] = (byte)(value & 0xFF);
            buffer[i * 2 + 1] = (byte)((value >> 8) & 0xFF);
        }

        return CreateWavBuffer(buffer, sampleRate, 1);
    }

    private byte[] GenerateExplosionSound()
    {
        const int sampleRate = 44100;
        const double duration = 0.3;
        int samples = (int)(sampleRate * duration);
        byte[] buffer = new byte[samples * 2];
        var random = new Random(42);

        for (int i = 0; i < samples; i++)
        {
            double t = (double)i / sampleRate;
            double envelope = Math.Exp(-t * 8);
            double noise = (random.NextDouble() * 2 - 1);
            double sample = noise * envelope * 0.6;
            short value = (short)(sample * short.MaxValue);
            buffer[i * 2] = (byte)(value & 0xFF);
            buffer[i * 2 + 1] = (byte)((value >> 8) & 0xFF);
        }

        return CreateWavBuffer(buffer, sampleRate, 1);
    }

    private byte[] GenerateInvaderKilledSound()
    {
        const int sampleRate = 44100;
        const double duration = 0.2;
        int samples = (int)(sampleRate * duration);
        byte[] buffer = new byte[samples * 2];

        for (int i = 0; i < samples; i++)
        {
            double t = (double)i / sampleRate;
            double envelope = 1.0 - (t / duration);
            double freq1 = 400 - t * 800;
            double freq2 = 800 - t * 1600;
            double sample = (Math.Sin(2 * Math.PI * freq1 * t) + Math.Sin(2 * Math.PI * freq2 * t)) * envelope * 0.3;
            short value = (short)(sample * short.MaxValue);
            buffer[i * 2] = (byte)(value & 0xFF);
            buffer[i * 2 + 1] = (byte)((value >> 8) & 0xFF);
        }

        return CreateWavBuffer(buffer, sampleRate, 1);
    }

    private byte[] GeneratePlayerDeathSound()
    {
        const int sampleRate = 44100;
        const double duration = 0.8;
        int samples = (int)(sampleRate * duration);
        byte[] buffer = new byte[samples * 2];
        var random = new Random(42);

        for (int i = 0; i < samples; i++)
        {
            double t = (double)i / sampleRate;
            double envelope = Math.Exp(-t * 3);
            double frequency = 200 + Math.Sin(t * 20) * 100;
            double noise = (random.NextDouble() * 2 - 1) * 0.3;
            double sample = (Math.Sin(2 * Math.PI * frequency * t) + noise) * envelope * 0.5;
            short value = (short)(sample * short.MaxValue);
            buffer[i * 2] = (byte)(value & 0xFF);
            buffer[i * 2 + 1] = (byte)((value >> 8) & 0xFF);
        }

        return CreateWavBuffer(buffer, sampleRate, 1);
    }

    private byte[] GenerateUfoSound()
    {
        const int sampleRate = 44100;
        const double duration = 0.5;
        int samples = (int)(sampleRate * duration);
        byte[] buffer = new byte[samples * 2];

        for (int i = 0; i < samples; i++)
        {
            double t = (double)i / sampleRate;
            double frequency = 400 + Math.Sin(t * 15) * 100;
            double sample = Math.Sin(2 * Math.PI * frequency * t) * 0.3;
            short value = (short)(sample * short.MaxValue);
            buffer[i * 2] = (byte)(value & 0xFF);
            buffer[i * 2 + 1] = (byte)((value >> 8) & 0xFF);
        }

        return CreateWavBuffer(buffer, sampleRate, 1);
    }

    private byte[] GenerateMarchSound(double frequency)
    {
        const int sampleRate = 44100;
        const double duration = 0.08;
        int samples = (int)(sampleRate * duration);
        byte[] buffer = new byte[samples * 2];

        for (int i = 0; i < samples; i++)
        {
            double t = (double)i / sampleRate;
            double envelope = 1.0 - (t / duration);
            double sample = Math.Sign(Math.Sin(2 * Math.PI * frequency * t)) * envelope * 0.25;
            short value = (short)(sample * short.MaxValue);
            buffer[i * 2] = (byte)(value & 0xFF);
            buffer[i * 2 + 1] = (byte)((value >> 8) & 0xFF);
        }

        return CreateWavBuffer(buffer, sampleRate, 1);
    }

    private byte[] CreateWavBuffer(byte[] audioData, int sampleRate, int channels)
    {
        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);

        int bitsPerSample = 16;
        int byteRate = sampleRate * channels * bitsPerSample / 8;
        int blockAlign = channels * bitsPerSample / 8;

        // RIFF header
        writer.Write("RIFF"u8);
        writer.Write(36 + audioData.Length);
        writer.Write("WAVE"u8);

        // fmt chunk
        writer.Write("fmt "u8);
        writer.Write(16);
        writer.Write((short)1); // PCM
        writer.Write((short)channels);
        writer.Write(sampleRate);
        writer.Write(byteRate);
        writer.Write((short)blockAlign);
        writer.Write((short)bitsPerSample);

        // data chunk
        writer.Write("data"u8);
        writer.Write(audioData.Length);
        writer.Write(audioData);

        return ms.ToArray();
    }

    public void PlaySound(string soundName)
    {
        if (IsMuted || !_initialized || _disposed)
            return;

        if (!_soundBuffers.TryGetValue(soundName, out var buffer))
            return;

        Task.Run(() => PlayWavBuffer(buffer));
    }

    private void PlayWavBuffer(byte[] buffer)
    {
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                PlayWavWindows(buffer);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                PlayWavLinux(buffer);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                PlayWavMacOS(buffer);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Sound playback error: {ex.Message}");
        }
    }

    private void PlayWavWindows(byte[] buffer)
    {
        // Use NAudio on Windows
        try
        {
            using var ms = new MemoryStream(buffer);
            using var waveOut = new NAudio.Wave.WaveOutEvent();
            using var reader = new NAudio.Wave.WaveFileReader(ms);
            waveOut.Init(reader);
            waveOut.Play();
            while (waveOut.PlaybackState == NAudio.Wave.PlaybackState.Playing)
            {
                Thread.Sleep(10);
            }
        }
        catch { }
    }

    private void PlayWavLinux(byte[] buffer)
    {
        try
        {
            var tempFile = Path.GetTempFileName() + ".wav";
            File.WriteAllBytes(tempFile, buffer);

            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "aplay",
                    Arguments = $"-q \"{tempFile}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            process.WaitForExit(1000);

            try { File.Delete(tempFile); } catch { }
        }
        catch { }
    }

    private void PlayWavMacOS(byte[] buffer)
    {
        try
        {
            var tempFile = Path.GetTempFileName() + ".wav";
            File.WriteAllBytes(tempFile, buffer);

            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "afplay",
                    Arguments = $"\"{tempFile}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            process.WaitForExit(2000);

            try { File.Delete(tempFile); } catch { }
        }
        catch { }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _soundBuffers.Clear();
    }
}
