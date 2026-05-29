namespace TastyMealPlanner.Services;

public interface ILocationService
{
    Task<LocationInfo?> GetCurrentLocationAsync();

    Task<string> GetAddressFromLocationAsync(double latitude, double longitude);

    Task<List<NearbyPlace>> GetNearbyGroceryStoresAsync(double latitude, double longitude);

    Task<bool> RequestLocationPermissionAsync();
}

public class LocationInfo
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string DisplayText { get; set; } = string.Empty;
}

public class NearbyPlace
{
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public double DistanceKm { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string IconGlyph { get; set; } = "\U0001F3EA";
}
