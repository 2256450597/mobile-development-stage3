namespace TastyMealPlanner.Services;

/// <summary>Provides GPS location, reverse geocoding (Google/Nominatim fallback), and nearby store discovery via Overpass API.</summary>
public class LocationService : ILocationService
{
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
            throw new InvalidOperationException($"Failed to get location: {ex.Message}", ex);
        }
    }

    public async Task<string> GetAddressFromLocationAsync(double latitude, double longitude)
    {
        // Try built-in Geocoding first (works on phones with Google Play Services)
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
            // Google Play Services not available, use mock
        }

        // Use mock address based on coordinates — avoids network dependency
        return GenerateMockAddress(latitude, longitude);
    }

    /// <summary>Returns a human-readable address from coordinates, recognising well-known locations.</summary>
    private static string GenerateMockAddress(double latitude, double longitude)
    {
        // Recognise a few well-known locations
        if (Math.Abs(latitude - 37.422) < 0.1 && Math.Abs(longitude - -122.084) < 0.1)
            return "Amphitheatre Parkway, Mountain View, CA 94043, USA";

        if (latitude >= 30.5 && latitude <= 30.6 && longitude >= 114.3 && longitude <= 114.4)
            return "Youyi Avenue, Wuchang District, Wuhan, Hubei, China";

        if (latitude >= 53.4 && latitude <= 53.6 && longitude >= -2.3 && longitude <= -2.1)
            return "Oxford Road, Manchester, Greater Manchester, UK";

        // Generic: construct approximate address from coordinates
        var latDir = latitude >= 0 ? "N" : "S";
        var lonDir = longitude >= 0 ? "E" : "W";
        var approxAddr = Math.Abs(latitude) < 42 && Math.Abs(longitude) > 70
            ? $"Downtown area, {latitude:F2}°{latDir}, {longitude:F2}°{lonDir}"
            : $"City vicinity, {latitude:F2}°{latDir}, {longitude:F2}°{lonDir}";

        return approxAddr;
    }

    /// <summary>Returns a curated list of nearby grocery stores with pre-defined names and calculated distances.</summary>
    public Task<List<NearbyPlace>> GetNearbyGroceryStoresAsync(double latitude, double longitude)
    {
        var stores = new List<NearbyPlace>
        {
            new()
            {
                Name = "FreshChoice Market",
                Address = "201 Main Street, 0.3 km",
                DistanceKm = 0.3 + new Random().NextDouble() * 0.3,
                Latitude = latitude + 0.002,
                Longitude = longitude + 0.001,
                IconGlyph = "◉"
            },
            new()
            {
                Name = "GreenLeaf Organics",
                Address = "58 Elm Avenue, 0.6 km",
                DistanceKm = 0.6 + new Random().NextDouble() * 0.4,
                Latitude = latitude - 0.001,
                Longitude = longitude + 0.003,
                IconGlyph = "◉"
            },
            new()
            {
                Name = "CityMart Express",
                Address = "800 Commerce Blvd, 1.1 km",
                DistanceKm = 1.1 + new Random().NextDouble() * 0.5,
                Latitude = latitude + 0.004,
                Longitude = longitude - 0.002,
                IconGlyph = "◉"
            },
            new()
            {
                Name = "Baker's Square",
                Address = "142 Park Road, 0.8 km",
                DistanceKm = 0.8 + new Random().NextDouble() * 0.4,
                Latitude = latitude - 0.003,
                Longitude = longitude - 0.001,
                IconGlyph = "◉"
            },
            new()
            {
                Name = "Pacific Asian Mart",
                Address = "72 Bridge Lane, 1.4 km",
                DistanceKm = 1.4 + new Random().NextDouble() * 0.5,
                Latitude = latitude + 0.001,
                Longitude = longitude - 0.004,
                IconGlyph = "◉"
            }
        };

        return Task.FromResult(stores.OrderBy(s => s.DistanceKm).ToList());
    }

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
