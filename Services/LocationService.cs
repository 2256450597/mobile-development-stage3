namespace TastyMealPlanner.Services;

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
        // Try built-in Geocoding first (requires Google Play Services on Android)
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
            // Google Play Services not available, fall through to Nominatim
        }

        // Fallback: OpenStreetMap Nominatim (free, no API key needed)
        return await GetNominatimAddressAsync(latitude, longitude);
    }

    private static async Task<string> GetNominatimAddressAsync(double latitude, double longitude)
    {
        try
        {
            using var http = new HttpClient();
            http.DefaultRequestHeaders.Add("User-Agent", "TastyMealPlanner/1.0");
            var url = $"https://nominatim.openstreetmap.org/reverse?format=jsonv2&lat={latitude:F6}&lon={longitude:F6}";
            var response = await http.GetStringAsync(url);
            var doc = System.Text.Json.JsonDocument.Parse(response);
            var displayName = doc.RootElement.GetProperty("display_name").GetString();
            return !string.IsNullOrWhiteSpace(displayName)
                ? displayName
                : $"{latitude:F5}, {longitude:F5}";
        }
        catch
        {
            return $"{latitude:F5}, {longitude:F5}";
        }
    }

    public Task<List<NearbyPlace>> GetNearbyGroceryStoresAsync(double latitude, double longitude)
    {
        var rng = new Random();
        var stores = new List<NearbyPlace>
        {
            new()
            {
                Name = "FreshMart Supermarket",
                Address = "123 High Street",
                DistanceKm = 0.3 + rng.NextDouble() * 0.5,
                Latitude = latitude + 0.002,
                Longitude = longitude + 0.001,
                IconGlyph = "\U0001F3EA"
            },
            new()
            {
                Name = "Organic Greens Market",
                Address = "45 Park Lane",
                DistanceKm = 0.6 + rng.NextDouble() * 0.5,
                Latitude = latitude - 0.001,
                Longitude = longitude + 0.003,
                IconGlyph = "\U0001F33F"
            },
            new()
            {
                Name = "City Food Wholesale",
                Address = "78 Commerce Road",
                DistanceKm = 1.2 + rng.NextDouble() * 0.5,
                Latitude = latitude + 0.004,
                Longitude = longitude - 0.002,
                IconGlyph = "\U0001F3EC"
            },
            new()
            {
                Name = "Baker's Delight",
                Address = "200 Mill Street",
                DistanceKm = 0.8 + rng.NextDouble() * 0.5,
                Latitude = latitude - 0.003,
                Longitude = longitude - 0.001,
                IconGlyph = "\U0001F35E"
            },
            new()
            {
                Name = "Asian Grocery Mart",
                Address = "56 Eastern Avenue",
                DistanceKm = 1.5 + rng.NextDouble() * 0.5,
                Latitude = latitude + 0.001,
                Longitude = longitude - 0.004,
                IconGlyph = "\U0001F3EA"
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
