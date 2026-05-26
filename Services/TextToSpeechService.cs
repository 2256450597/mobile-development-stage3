namespace TastyMealPlanner.Services;

public class TextToSpeechService : ITextToSpeechService
{
    public float Speed { get; set; } = 1.0f;
    public float Pitch { get; set; } = 1.0f;

    public bool IsSpeaking { get; private set; }

    public async Task SpeakAsync(string text, float speed = 1.0f, float pitch = 1.0f)
    {
        try
        {
            IsSpeaking = true;
            var options = new SpeechOptions
            {
                Pitch = pitch,
                Volume = 1.0f,
            };

            Speed = speed;
            Pitch = pitch;

            await TextToSpeech.Default.SpeakAsync(text, options);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Text-to-speech failed: {ex.Message}", ex);
        }
        finally
        {
            IsSpeaking = false;
        }
    }

    public Task StopAsync()
    {
        IsSpeaking = false;
        return Task.CompletedTask;
    }
}
