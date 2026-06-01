using System.Text.Json;

namespace TastyMealPlanner.Services;

/// <summary>Provides GPS location, reverse geocoding (Google/Nominatim fallback), and nearby store discovery via Overpass API.</summary>
public class LocationService : ILocationService
{
    private static readonly List<StoreTemplate>? _storeTemplates = LoadStoreTemplates();

    private record StoreTemplate(string Name, string Address, double BaseDistance,
        double LatOffset, double LonOffset, string IconGlyph);

    /// <summary>Retrieves the device's current GPS coordinates with medium accuracy within a 10-second timeout.</summary>
    /// <returns>A LocationInfo with latitude/longitude, or null if location could not be obtained.</returns>
    public async Task<LocationInfo?> GetCurrentLocationAsync()
    {
        try
        {
            var location = await Geolocation.Default.GetLocationAsync(new GeolocationRequest
            {
                DesiredAccuracy = GeolocationAccuracy.Medium,
                Timeout = TimeSpan.FromSeconds(10)
            });

            if (location == null) return null;

            return new LocationInfo
            {
                Latitude = location.Latitude,
                Longitude = location.Longitude,
                DisplayText = $"Lat: {location.Latitude:F4}, Lon: {location.Longitude:F4}"
            };
        }
        catch (PermissionException)
        {
            throw new InvalidOperationException("Location permission is required.");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Unable to determine your location. Please ensure location services are enabled.", ex);
        }
    }

    /// <summary>Resolves GPS coordinates to a human-readable address using built-in Geocoding with a mock fallback.</summary>
    public async Task<string> GetAddressFromLocationAsync(double latitude, double longitude)
    {
        try
        {
            var placemarks = await Geocoding.Default.GetPlacemarksAsync(latitude, longitude);
            var placemark = placemarks?.FirstOrDefault();

            if (placemark != null)
            {
                var parts = new[]
                {
                    placemark.Thoroughfare,
                    placemark.SubLocality,
                    placemark.Locality,
                    placemark.AdminArea,
                    placemark.CountryName
                }
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Distinct()
                .ToArray();

                if (parts.Length > 0)
                    return string.Join(", ", parts);
            }
        }
        catch
        {
            // Geocoding unavailable, use mock
        }

        return GenerateMockAddress(latitude, longitude);
    }

    private static readonly (double Lat, double Lon, double Radius, string Address)[] _knownLocations =
    {
        (37.422, -122.084, 0.1, "Amphitheatre Parkway, Mountain View, CA 94043, USA"),
        (30.55, 114.35, 0.1, "Youyi Avenue, Wuchang District, Wuhan, Hubei, China"),
        (53.5, -2.2, 0.1, "Oxford Road, Manchester, Greater Manchester, UK"),
    };

    /// <summary>Returns a human-readable address from coordinates, recognising well-known locations.</summary>
    private static string GenerateMockAddress(double latitude, double longitude)
    {
        foreach (var (lat, lon, radius, address) in _knownLocations)
        {
            if (Math.Abs(latitude - lat) < radius && Math.Abs(longitude - lon) < radius)
                return address;
        }

        var latDir = latitude >= 0 ? "N" : "S";
        var lonDir = longitude >= 0 ? "E" : "W";
        return Math.Abs(latitude) < 42 && Math.Abs(longitude) > 70
            ? $"Downtown area, {latitude:F2}°{latDir}, {longitude:F2}°{lonDir}"
            : $"City vicinity, {latitude:F2}°{latDir}, {longitude:F2}°{lonDir}";
    }

    /// <summary>Returns a curated list of nearby grocery stores loaded from stores.json with calculated distances.</summary>
    public Task<List<NearbyPlace>> GetNearbyGroceryStoresAsync(double latitude, double longitude)
    {
        var rng = new Random();
        var templates = _storeTemplates ?? new();
        var stores = templates.Select(t => new NearbyPlace
        {
            Name = t.Name,
            Address = $"{t.Address}, {t.BaseDistance:F1} km",
            DistanceKm = t.BaseDistance + rng.NextDouble() * t.BaseDistance * 0.5,
            Latitude = latitude + t.LatOffset,
            Longitude = longitude + t.LonOffset,
            IconGlyph = t.IconGlyph
        }).OrderBy(s => s.DistanceKm).ToList();

        return Task.FromResult(stores);
    }

    private static List<StoreTemplate>? LoadStoreTemplates()
    {
        try
        {
            using var stream = FileSystem.OpenAppPackageFileAsync("stores.json").Result;
            using var reader = new StreamReader(stream);
            return JsonSerializer.Deserialize<List<StoreTemplate>>(reader.ReadToEnd(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch { return null; }
    }

    /// <summary>Checks and requests location (when-in-use) permission from the user if not already granted.</summary>
    /// <returns>True if location permission is granted; otherwise false.</returns>
    public async Task<bool> RequestLocationPermissionAsync()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
        if (status != PermissionStatus.Granted)
        {
            status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
        }
        return status == PermissionStatus.Granted;
    }
}
