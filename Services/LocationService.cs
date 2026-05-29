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

    public async Task<List<NearbyPlace>> GetNearbyGroceryStoresAsync(double latitude, double longitude)
    {
        try
        {
            return await GetRealStoresFromOverpassAsync(latitude, longitude);
        }
        catch
        {
            // Network unavailable, return empty list rather than fake data
            return new List<NearbyPlace>();
        }
    }

    private static async Task<List<NearbyPlace>> GetRealStoresFromOverpassAsync(double latitude, double longitude)
    {
        var query = $"[out:json];(node[shop=supermarket](around:5000,{latitude},{longitude});node[shop=convenience](around:3000,{latitude},{longitude});node[shop=grocery](around:3000,{latitude},{longitude}););out center 20;";
        var url = $"https://overpass-api.de/api/interpreter?data={Uri.EscapeDataString(query)}";

        using var http = new HttpClient();
        http.DefaultRequestHeaders.Add("User-Agent", "TastyMealPlanner/1.0");
        http.Timeout = TimeSpan.FromSeconds(15);

        var response = await http.GetStringAsync(url);
        var doc = System.Text.Json.JsonDocument.Parse(response);
        var elements = doc.RootElement.GetProperty("elements");

        var stores = new List<NearbyPlace>();
        foreach (var el in elements.EnumerateArray())
        {
            var name = "Unknown Store";
            if (el.TryGetProperty("tags", out var tags) && tags.TryGetProperty("name", out var nameProp))
                name = nameProp.GetString() ?? "Unknown Store";
            else if (el.TryGetProperty("tags", out var tags2) && tags2.TryGetProperty("shop", out var shopProp))
                name = char.ToUpper(shopProp.GetString()![0]) + shopProp.GetString()![1..] + " Shop";

            var storeLat = el.GetProperty("lat").GetDouble();
            var storeLon = el.GetProperty("lon").GetDouble();
            var distance = CalculateDistance(latitude, longitude, storeLat, storeLon);

            stores.Add(new NearbyPlace
            {
                Name = name,
                Address = $"Approx. {distance:F1} km from you",
                DistanceKm = distance,
                Latitude = storeLat,
                Longitude = storeLon,
                IconGlyph = "\U0001F3EA"
            });
        }

        return stores.OrderBy(s => s.DistanceKm).Take(10).ToList();
    }

    private static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        var r = 6371.0; // Earth radius in km
        var dLat = ToRad(lat2 - lat1);
        var dLon = ToRad(lon2 - lon1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return r * c;
    }

    private static double ToRad(double deg) => deg * Math.PI / 180.0;

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
