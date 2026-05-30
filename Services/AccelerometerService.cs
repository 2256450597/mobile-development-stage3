namespace TastyMealPlanner.Services;

/// <summary>Detects device shake gestures via the platform ShakeDetected event.</summary>
public class AccelerometerService : IAccelerometerService
{
    private const int ShakeCooldownMs = 1000;
    private DateTime _lastShake = DateTime.MinValue;
    private bool _monitoring;

    public event EventHandler? ShakeDetected;
    public bool IsMonitoring => _monitoring;

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
        ShakeDetected?.Invoke(this, EventArgs.Empty);
    }
}
