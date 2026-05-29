namespace TastyMealPlanner.Services;

/// <summary>Wraps MAUI TextToSpeech API with configurable speed/pitch and cancellation support.</summary>
public class TextToSpeechService : ITextToSpeechService
{
    private CancellationTokenSource? _cts;

    public float Speed { get; set; } = 1.0f;
    public float Pitch { get; set; } = 1.0f;
    public bool IsSpeaking { get; private set; }

    public async Task SpeakAsync(string text, float speed = 1.0f, float pitch = 1.0f)
    {
        // Cancel any existing speech before starting new
        await StopAsync();

        _cts = new CancellationTokenSource();
        var options = new SpeechOptions
        {
            Pitch = pitch,
            Volume = 1.0f,
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
