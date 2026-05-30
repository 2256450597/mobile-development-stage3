namespace TastyMealPlanner.Services;

/// <summary>Provides click and long-press haptic feedback. Uses both HapticFeedback and Vibration
/// for reliable feedback across iOS (Taptic Engine) and Android (vibration motor).</summary>
public class HapticService : IHapticService
{
    /// <summary>Short click feedback — ~25ms vibration ensures Android devices feel the tap.</summary>
    public void PerformClick()
    {
        try
        {
            Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(25));
            HapticFeedback.Default.Perform(HapticFeedbackType.Click);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Haptic click failed: {ex.Message}");
        }
    }

    /// <summary>Longer feedback for confirm/save/delete actions — ~80ms vibration dual with haptic.</summary>
    public void PerformLongPress()
    {
        try
        {
            Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(80));
            HapticFeedback.Default.Perform(HapticFeedbackType.LongPress);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Haptic long press failed: {ex.Message}");
        }
    }
}
