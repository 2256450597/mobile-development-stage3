namespace TastyMealPlanner.Services;

public class TextToSpeechService : ITextToSpeechService
{
    private CancellationTokenSource? _cts;

    public float Speed { get; set; } = 1.0f;

    public float Pitch { get; set; } = 1.1f;

    public bool IsSpeaking { get; private set; }

    public async Task SpeakAsync(string text, float speed = 1.0f, float pitch = 1.1f)
    {
        // Cancel any existing speech before starting new
        await StopAsync();

        _cts = new CancellationTokenSource();
        var options = new SpeechOptions
        {
            Pitch = pitch,
            Volume = 1.0f,
            Locale = await GetEnglishLocaleAsync()
        };

        Speed = speed;
        Pitch = pitch;

        try
        {
            IsSpeaking = true;
            await TextToSpeech.Default.SpeakAsync(text, options, _cts.Token);
        }
        catch (OperationCanceledException)
        {
            // Speech was cancelled intentionally — this is not an error
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Text-to-speech failed: {ex.Message}", ex);
        }
        finally
        {
            IsSpeaking = false;
            _cts?.Dispose();
            _cts = null;
        }
    }

    private static async Task<Locale?> GetEnglishLocaleAsync()
    {
        try
        {
            var locales = await TextToSpeech.Default.GetLocalesAsync();
            // Prefer UK or US English over other English variants
            return locales.FirstOrDefault(l => l.Language == "en" && l.Country == "GB")
                ?? locales.FirstOrDefault(l => l.Language == "en" && l.Country == "US")
                ?? locales.FirstOrDefault(l => l.Language.StartsWith("en", StringComparison.OrdinalIgnoreCase))
                ?? null;
        }
        catch
        {
            return null;
        }
    }

    public Task StopAsync()
    {
        if (_cts != null && !_cts.IsCancellationRequested)
        {
            _cts.Cancel();
        }
        IsSpeaking = false;
        return Task.CompletedTask;
    }
}
