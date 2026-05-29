namespace TastyMealPlanner.Services;

public class HapticService : IHapticService
{
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
