namespace TastyMealPlanner.Services;

/// <summary>Detects device shake gestures via the platform ShakeDetected event.</summary>
public class AccelerometerService : IAccelerometerService
{
    private const int ShakeCooldownMs = 1000;
    private DateTime _lastShake = DateTime.MinValue;
    private bool _monitoring;

    /// <summary>Raised when the device detects a shake gesture.</summary>
    public event EventHandler? ShakeDetected;

    /// <summary>Indicates whether shake monitoring is currently active.</summary>
    public bool IsMonitoring => _monitoring;

    /// <summary>Begins listening for shake gestures from the device accelerometer.</summary>
    public void StartShakeDetection()
    {
        if (_monitoring) return;
        try
        {
            if (Accelerometer.Default.IsSupported)
            {
                Accelerometer.Default.ShakeDetected += OnShake;
                Accelerometer.Default.Start(SensorSpeed.UI);
                _monitoring = true;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Accel start error: {ex.Message}");
        }
    }

    /// <summary>Stops listening for shake gestures and releases the accelerometer.</summary>
    public void StopShakeDetection()
    {
        if (!_monitoring) return;
        try
        {
            Accelerometer.Default.ShakeDetected -= OnShake;
            Accelerometer.Default.Stop();
        }
        catch { }
        finally { _monitoring = false; }
    }

    private void OnShake(object? s, EventArgs e)
    {
        var now = DateTime.Now;
        if ((now - _lastShake).TotalMilliseconds < ShakeCooldownMs) return;
        _lastShake = now;

        Device.BeginInvokeOnMainThread(() =>
            ShakeDetected?.Invoke(this, EventArgs.Empty));
    }
}
