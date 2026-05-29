using System.Collections.ObjectModel;
using System.Windows.Input;
using TastyMealPlanner.Services;

namespace TastyMealPlanner.ViewModels;

public class NearbyViewModel : BaseViewModel
{
    private readonly ILocationService _locationService;
    private readonly IHapticService _haptic;
    private readonly ICompassService _compass;

    public ObservableCollection<NearbyPlace> NearbyPlaces { get; } = new();

    public bool HasCompass => _compass.IsSupported;
    public string CompassLabel => _compass.CardinalLabel;
    public string HeadingDisplay => _compass.Heading.HasValue
        ? $"{_compass.Heading.Value:F0}°"
        : "--";

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

    public ICommand GoBackCommand { get; }
    public ICommand RefreshLocationCommand { get; }

    public NearbyViewModel(ILocationService locationService, IHapticService haptic, ICompassService compass)
    {
        _locationService = locationService;
        _haptic = haptic;
        _compass = compass;
        Title = "Nearby Stores";

        // Listen for compass heading changes
        _compass.HeadingChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(CompassLabel));
            OnPropertyChanged(nameof(HeadingDisplay));
        };
        _compass.Start();

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
}
