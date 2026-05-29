namespace TastyMealPlanner.Services;

/// <summary>
/// Provides haptic (tactile) feedback for user interactions.
/// Used throughout the app to confirm button presses and gestures.
/// </summary>
public interface IHapticService
{
    /// <summary>Triggers a short click vibration for standard button presses.</summary>
    void PerformClick();

    /// <summary>Triggers a longer vibration for significant actions (e.g. confirmations).</summary>
    void PerformLongPress();
}
