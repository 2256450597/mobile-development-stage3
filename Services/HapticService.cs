namespace TastyMealPlanner.Services;

/// <summary>Provides click and long-press haptic feedback via MAUI HapticFeedback API.</summary>
public class HapticService : IHapticService
{
    public void PerformClick()
    {
        try
        {
            HapticFeedback.Default.Perform(HapticFeedbackType.Click);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Haptic click failed: {ex.Message}");
        }
    }

    public void PerformLongPress()
    {
        try
        {
            HapticFeedback.Default.Perform(HapticFeedbackType.LongPress);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Haptic long press failed: {ex.Message}");
        }
    }
}
