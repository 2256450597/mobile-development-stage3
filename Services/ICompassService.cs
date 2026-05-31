namespace TastyMealPlanner.Services;

/// <summary>Provides device compass heading data via the magnetometer sensor.
/// Exposes the current heading in degrees (0-360, where 0/360 is North).</summary>
public interface ICompassService
{
    /// <summary>Whether the device hardware supports compass readings.</summary>
    bool IsSupported { get; }

    /// <summary>Whether the compass sensor is actively monitoring.</summary>
    bool IsMonitoring { get; }

    /// <summary>Most recent heading in degrees (0-360). Null if no reading yet.</summary>
    double? Heading { get; }

    /// <summary>Human-readable cardinal direction label (e.g. "N", "NE").</summary>
    string CardinalLabel { get; }

    /// <summary>Fires when the heading changes.</summary>
    event EventHandler<double>? HeadingChanged;

    /// <summary>Starts monitoring the compass sensor.</summary>
    void Start();

    /// <summary>Stops monitoring the compass sensor.</summary>
    void Stop();
}
