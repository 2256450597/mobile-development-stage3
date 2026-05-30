using Android.Views;
using TastyMealPlanner.Services;

namespace TastyMealPlanner;

/// <summary>Android-specific feedback service. Adds native system click sound
/// (SoundEffectConstants.Click) alongside vibration, matching the native
/// Button behavior that TapGestureRecognizer misses on Android.</summary>
public class AndroidHapticService : IHapticService
{
    public void PerformClick()
    {
        try
        {
            // Short vibration for tactile feedback
            Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(25));
            HapticFeedback.Default.Perform(HapticFeedbackType.Click);

            // Play the Android system "touch sound" click effect.
            // This is the same sound native Button clicks produce.
            var rootView = Platform.CurrentActivity?.Window?.DecorView;
            rootView?.PlaySoundEffect(SoundEffects.Click);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Click feedback failed: {ex.Message}");
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
            System.Diagnostics.Debug.WriteLine($"Long press feedback failed: {ex.Message}");
        }
    }
}
