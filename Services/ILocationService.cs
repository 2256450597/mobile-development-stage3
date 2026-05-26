namespace TastyMealPlanner.Services;

/// <summary>
/// Provides geolocation services and nearby grocery store discovery.
/// </summary>
public interface ILocationService
{
    /// <summary>Gets the current device location. Throws on permission denial or timeout.</summary>
    Task<LocationInfo?> GetCurrentLocationAsync();

    /// <summary>Returns a list of mock grocery stores near the specified coordinates.</summary>
    Task<List<NearbyPlace>> GetNearbyGroceryStoresAsync(double latitude, double longitude);

    /// <summary>Requests location permission from the user. Returns true if granted.</summary>
    Task<bool> RequestLocationPermissionAsync();
}

/// <summary>
/// Contains latitude, longitude, and a human-readable location description.
/// </summary>
public class LocationInfo
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string DisplayText { get; set; } = string.Empty;
}

/// <summary>
/// Represents a nearby grocery store with distance and location data.
/// </summary>
public class NearbyPlace
{
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public double DistanceKm { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string IconGlyph { get; set; } = "\U0001F3EA";
}
