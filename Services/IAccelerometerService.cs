namespace TastyMealPlanner.Services;

/// <summary>
/// Detects device shake gestures using the accelerometer.
/// Includes debouncing to prevent duplicate triggers.
/// </summary>
public interface IAccelerometerService
{
    /// <summary>Raised when a shake gesture is detected (debounced to ~500ms).</summary>
    event EventHandler? ShakeDetected;

    /// <summary>Starts monitoring for shake gestures.</summary>
    void StartShakeDetection();

    /// <summary>Stops monitoring for shake gestures and releases sensor resources.</summary>
    void StopShakeDetection();

    /// <summary>Indicates whether shake detection is currently active.</summary>
    bool IsMonitoring { get; }
}
