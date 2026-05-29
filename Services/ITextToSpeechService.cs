namespace TastyMealPlanner.Services;

public interface ITextToSpeechService
{
    Task SpeakAsync(string text, float speed = 1.0f, float pitch = 1.0f);

    Task StopAsync();

    float Speed { get; set; }

    float Pitch { get; set; }

    bool IsSpeaking { get; }
}
