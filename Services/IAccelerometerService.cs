namespace TastyMealPlanner.Services;

public interface IAccelerometerService
{
    event EventHandler? ShakeDetected;

    void StartShakeDetection();

    void StopShakeDetection();

    bool IsMonitoring { get; }
}
