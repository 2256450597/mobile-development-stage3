using System.Collections.ObjectModel;
using System.Windows.Input;
using TastyMealPlanner.Services;

namespace TastyMealPlanner.ViewModels;

public class NearbyViewModel : BaseViewModel
{
    private readonly ILocationService _locationService;
    private readonly IHapticService _haptic;

    public ObservableCollection<NearbyPlace> NearbyPlaces { get; } = new();

    private string _locationInfo = "Fetching location...";
    public string LocationInfo
    {
        get => _locationInfo;
        set => SetProperty(ref _locationInfo, value);
    }

    private string _address = string.Empty;
    public string Address
    {
        get => _address;
        set => SetProperty(ref _address, value);
    }

    private string _coordinates = string.Empty;
    public string Coordinates
    {
        get => _coordinates;
        set => SetProperty(ref _coordinates, value);
    }

    private bool _isLocating;
    public bool IsLocating
    {
        get => _isLocating;
        set => SetProperty(ref _isLocating, value);
    }

    private bool _hasLocation;
    public bool HasLocation
    {
        get => _hasLocation;
        set => SetProperty(ref _hasLocation, value);
    }

    private HtmlWebViewSource? _mapSource;
    public HtmlWebViewSource? MapSource
    {
        get => _mapSource;
        set => SetProperty(ref _mapSource, value);
    }

    private bool _hasMap;
    public bool HasMap
    {
        get => _hasMap;
        set => SetProperty(ref _hasMap, value);
    }

    public ICommand GoBackCommand { get; }
    public ICommand RefreshLocationCommand { get; }

    public NearbyViewModel(ILocationService locationService, IHapticService haptic)
    {
        _locationService = locationService;
        _haptic = haptic;
        Title = "Nearby Stores";

        GoBackCommand = new Command(async () =>
        {
            _haptic.PerformClick();
            await Shell.Current.GoToAsync("..");
        });

        RefreshLocationCommand = new Command(async () =>
        {
            _haptic.PerformClick();
            await GetLocationAsync();
        });

        _ = GetLocationAsync();
    }

    private async Task GetLocationAsync()
    {
        try
        {
            IsLocating = true;
            LocationInfo = "Requesting location permission...";

            var hasPermission = await _locationService.RequestLocationPermissionAsync();
            if (!hasPermission)
            {
                LocationInfo = "Location permission denied. Please enable in settings.";
                return;
            }

            LocationInfo = "Fetching your location...";
            var location = await _locationService.GetCurrentLocationAsync();

            if (location != null)
            {
                HasLocation = true;
                Coordinates = $"Lat: {location.Latitude:F5}, Lon: {location.Longitude:F5}";

                Address = "Looking up address...";
                Address = await _locationService.GetAddressFromLocationAsync(
                    location.Latitude, location.Longitude);

                LocationInfo = "Finding nearby grocery stores...";
                var places = await _locationService.GetNearbyGroceryStoresAsync(
                    location.Latitude, location.Longitude);

                NearbyPlaces.Clear();
                foreach (var place in places)
                    NearbyPlaces.Add(place);

                LocationInfo = $"Found {places.Count} stores nearby.";

                // Generate OpenStreetMap via Leaflet
                GenerateMap(location.Latitude, location.Longitude, places);
            }
            else
            {
                LocationInfo = "Unable to get current location. Try again.";
            }
        }
        catch (Exception ex)
        {
            LocationInfo = $"Error: {ex.Message}";
        }
        finally
        {
            IsLocating = false;
        }
    }

    private void GenerateMap(double lat, double lon, List<NearbyPlace> places)
    {
        var markersJs = $"var you = L.marker([{lat}, {lon}]).addTo(map).bindPopup('You are here').openPopup();\n";
        foreach (var place in places)
        {
            markersJs += $"L.marker([{place.Latitude}, {place.Longitude}]).addTo(map).bindPopup('<b>{place.Name}</b><br/>{place.Address}<br/>{place.DistanceKm:F1} km');\n";
        }

        var html = $@"<!DOCTYPE html>
<html>
<head>
<meta charset='utf-8'/>
<meta name='viewport' content='width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no'/>
<link rel='stylesheet' href='https://unpkg.com/leaflet@1.9.4/dist/leaflet.css'/>
<script src='https://unpkg.com/leaflet@1.9.4/dist/leaflet.js'></script>
<style>
  html,body,#map{{margin:0;padding:0;height:100%;width:100%}}
</style>
</head>
<body>
<div id='map'></div>
<script>
  var map = L.map('map').setView([{lat}, {lon}], 16);
  L.tileLayer('https://{{s}}.tile.openstreetmap.org/{{z}}/{{x}}/{{y}}.png', {{
    attribution: '&copy; OpenStreetMap contributors',
    maxZoom: 19
  }}).addTo(map);
  {markersJs}
</script>
</body>
</html>";

        MapSource = new HtmlWebViewSource { Html = html };
        HasMap = true;
    }
}
