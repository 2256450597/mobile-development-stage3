namespace TastyMealPlanner.Services;

/// <summary>Detects device shake gestures by monitoring accelerometer readings directly.</summary>
public class AccelerometerService : IAccelerometerService
{
    private const double ShakeThreshold = 1.2;
    private const int ShakeCooldownMs = 800;

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
                Accelerometer.Default.ReadingChanged += OnReadingChanged;
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
            Accelerometer.Default.ReadingChanged -= OnReadingChanged;
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

    private void OnReadingChanged(object? sender, AccelerometerChangedEventArgs e)
    {
        var acc = e.Reading.Acceleration;
        // Calculate total acceleration magnitude (ignoring gravity)
        var magnitude = Math.Sqrt(acc.X * acc.X + acc.Y * acc.Y + acc.Z * acc.Z);

        if (magnitude > ShakeThreshold)
        {
            var now = DateTime.Now;
            if ((now - _lastShakeTime).TotalMilliseconds < ShakeCooldownMs)
                return;

            _lastShakeTime = now;
            ShakeDetected?.Invoke(this, EventArgs.Empty);
        }
    }
}
