namespace TastyMealPlanner.Services;

/// <summary>Wraps the MAUI Compass sensor to provide heading data in degrees
/// with cardinal direction labels. Falls back gracefully if compass is unsupported.</summary>
public class CompassService : ICompassService
{
    /// <inheritdoc />
    public bool IsSupported { get; }

    /// <inheritdoc />
    public bool IsMonitoring { get; private set; }

    private double? _heading;
    /// <inheritdoc />
    public double? Heading
    {
        get => _heading;
        private set
        {
            if (_heading != value)
            {
                _heading = value;
                if (value.HasValue)
                    HeadingChanged?.Invoke(this, value.Value);
            }
        }
    }

    /// <inheritdoc />
    public string CardinalLabel => Heading switch
    {
        null => "--",
        >= 337.5 or < 22.5 => "N",
        >= 22.5 and < 67.5 => "NE",
        >= 67.5 and < 112.5 => "E",
        >= 112.5 and < 157.5 => "SE",
        >= 157.5 and < 202.5 => "S",
        >= 202.5 and < 247.5 => "SW",
        >= 247.5 and < 292.5 => "W",
        >= 292.5 and < 337.5 => "NW",
        _ => "--"
    };

    /// <inheritdoc />
    public event EventHandler<double>? HeadingChanged;

    /// <summary>Initialises the compass service and checks device support.</summary>
    public CompassService()
    {
        IsSupported = Compass.Default.IsSupported;
    }

    /// <inheritdoc />
    public void Start()
    {
        if (!IsSupported || IsMonitoring) return;

        try
        {
            Compass.Default.ReadingChanged += OnReadingChanged;
            Compass.Default.Start(SensorSpeed.UI);
            IsMonitoring = true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Compass start failed: {ex.Message}. Compass may not be available on this device.");
        }
    }

    /// <inheritdoc />
    public void Stop()
    {
        if (!IsMonitoring) return;

        try
        {
            Compass.Default.ReadingChanged -= OnReadingChanged;
            Compass.Default.Stop();
            IsMonitoring = false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Compass stop failed: {ex.Message}. Sensor may have already been released.");
        }
    }

    private void OnReadingChanged(object? sender, CompassChangedEventArgs e)
    {
        Heading = e.Reading.HeadingMagneticNorth;
    }
}
