namespace TastyMealPlanner.Services;

/// <summary>
/// Provides text-to-speech capabilities for reading recipes aloud.
/// Speed and pitch can be adjusted before or during playback.
/// </summary>
public interface ITextToSpeechService
{
    /// <summary>Speaks the given text with specified speed and pitch multipliers.</summary>
    Task SpeakAsync(string text, float speed = 1.0f, float pitch = 1.0f);

    /// <summary>Stops any ongoing speech playback.</summary>
    Task StopAsync();

    /// <summary>Gets or sets the speech speed multiplier (0.5 to 2.0).</summary>
    float Speed { get; set; }

    /// <summary>Gets or sets the speech pitch multiplier (0.5 to 2.0).</summary>
    float Pitch { get; set; }

    /// <summary>Indicates whether speech is currently playing.</summary>
    bool IsSpeaking { get; }
}
