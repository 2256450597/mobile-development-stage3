namespace TastyMealPlanner.Services;

/// <summary>Detects device shake gestures using the accelerometer with 500ms debounce.</summary>
public class AccelerometerService : IAccelerometerService
{
    private const double ShakeThreshold = 1.5;
    private const int ShakeWindowMs = 500;

    private DateTime _lastShakeTime = DateTime.MinValue;
    private bool _isMonitoring;

    public event EventHandler? ShakeDetected;
    public bool IsMonitoring => _isMonitoring;

    public void StartShakeDetection()
    {
        if (_isMonitoring) return;

        try
        {
            if (Accelerometer.Default.IsSupported)
            {
                Accelerometer.Default.ShakeDetected += OnShakeDetected;
                Accelerometer.Default.Start(SensorSpeed.Game);
                _isMonitoring = true;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Accelerometer start failed: {ex.Message}");
        }
    }

    public void StopShakeDetection()
    {
        if (!_isMonitoring) return;

        try
        {
            Accelerometer.Default.ShakeDetected -= OnShakeDetected;
            Accelerometer.Default.Stop();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Accelerometer stop failed: {ex.Message}");
        }
        finally
        {
            _isMonitoring = false;
        }
    }

    /// <summary>Debounced shake handler — raises ShakeDetected event at most once per 500ms window.</summary>
    private void OnShakeDetected(object? sender, EventArgs e)
    {
        // Debounce shake events
        var now = DateTime.Now;
        if ((now - _lastShakeTime).TotalMilliseconds < ShakeWindowMs)
            return;

        _lastShakeTime = now;
        ShakeDetected?.Invoke(this, EventArgs.Empty);
    }
}
