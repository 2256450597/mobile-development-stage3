namespace TastyMealPlanner.Services;

/// <summary>Detects device shake gestures using both platform event and manual accelerometer readings.</summary>
public class AccelerometerService : IAccelerometerService
{
    private const double ShakeThreshold = 1.3;
    private const int ShakeCooldownMs = 800;
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
                // Subscribe to both built-in shake detection AND manual readings
                Accelerometer.Default.ShakeDetected += OnShake;
                Accelerometer.Default.ReadingChanged += OnReadingChanged;
                Accelerometer.Default.Start(SensorSpeed.Game);
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
            Accelerometer.Default.ReadingChanged -= OnReadingChanged;
            Accelerometer.Default.Stop();
        }
        catch { }
        finally { _monitoring = false; }
    }

    private void OnShake(object? s, EventArgs e) => TryFire();

    private void OnReadingChanged(object? s, AccelerometerChangedEventArgs e)
    {
        var a = e.Reading.Acceleration;
        var mag = Math.Sqrt(a.X * a.X + a.Y * a.Y + a.Z * a.Z);
        if (mag > ShakeThreshold) TryFire();
    }

    private void TryFire()
    {
        var now = DateTime.Now;
        if ((now - _lastShake).TotalMilliseconds < ShakeCooldownMs) return;
        _lastShake = now;
        ShakeDetected?.Invoke(this, EventArgs.Empty);
    }
}
