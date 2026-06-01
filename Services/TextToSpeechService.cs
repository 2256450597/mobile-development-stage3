namespace TastyMealPlanner.Services;

/// <summary>Wraps MAUI TextToSpeech API with configurable speed/pitch and cancellation support.</summary>
public class TextToSpeechService : ITextToSpeechService
{
    private CancellationTokenSource? _cts;

    /// <summary>Gets or sets the speech speed rate. Default is 1.0 (normal).</summary>
    public float Speed { get; set; } = 1.0f;

    /// <summary>Gets or sets the speech pitch. Default is 1.1 (slightly higher than normal).</summary>
    public float Pitch { get; set; } = 1.1f;

    /// <summary>Indicates whether speech output is currently in progress.</summary>
    public bool IsSpeaking { get; private set; }

    /// <summary>Speaks the specified text aloud, cancelling any speech already in progress.</summary>
    /// <param name="text">The text to speak.</param>
    /// <param name="speed">The speech rate (default 1.0).</param>
    /// <param name="pitch">The speech pitch (default 1.1).</param>
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
            throw new InvalidOperationException("Speech playback could not be started. Please try again.", ex);
        }
        finally
        {
            IsSpeaking = false;
            _cts?.Dispose();
            _cts = null;
        }
    }

    /// <summary>Finds an English voice locale on the device for more natural pronunciation.</summary>
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

    /// <summary>Cancels any ongoing speech output immediately.</summary>
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
