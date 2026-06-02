using System.Collections.ObjectModel;
using System.Windows.Input;
using TastyMealPlanner.Services;

namespace TastyMealPlanner.ViewModels;

/// <summary>Gets GPS location, reverse-geocodes an address, and finds nearby grocery stores.</summary>
public class NearbyViewModel : BaseViewModel
{
    private readonly ILocationService _locationService;
    private readonly IHapticService _haptic;
    private readonly ICompassService _compass;

    /// <summary>Gets the collection of nearby grocery places found during the last location query.</summary>
    public ObservableCollection<NearbyPlace> NearbyPlaces { get; } = new();

    /// <summary>Whether the device supports compass heading readings.</summary>
    public bool HasCompass => _compass.IsSupported;
    /// <summary>Current compass heading in degrees (0-360). "--" if unavailable.</summary>
    public string CompassLabel => _compass.CardinalLabel;
    /// <summary>Compass heading value formatted with degree symbol.</summary>
    public string HeadingDisplay => _compass.Heading.HasValue
        ? $"{_compass.Heading.Value:F0}°"
        : _compass.IsSupported ? "--°" : "N/A";
    /// <summary>User-friendly compass status text.</summary>
    public string CompassStatus => _compass.IsSupported
        ? (_compass.Heading.HasValue ? "Active" : "Calibrating...")
        : "Not available on this device";

    private string _locationInfo = "Fetching location...";
    /// <summary>Gets or sets the current location status message displayed to the user.</summary>
    public string LocationInfo
    {
        get => _locationInfo;
        set => SetProperty(ref _locationInfo, value);
    }

    private string _address = string.Empty;
    /// <summary>Gets or sets the reverse-geocoded address string for the current location.</summary>
    public string Address
    {
        get => _address;
        set => SetProperty(ref _address, value);
    }

    private string _coordinates = string.Empty;
    /// <summary>Gets or sets the formatted GPS coordinate display text.</summary>
    public string Coordinates
    {
        get => _coordinates;
        set => SetProperty(ref _coordinates, value);
    }

    private bool _isLocating;
    /// <summary>Gets or sets whether the app is currently fetching the device location.</summary>
    public bool IsLocating
    {
        get => _isLocating;
        set => SetProperty(ref _isLocating, value);
    }

    private bool _hasLocation;
    /// <summary>Gets or sets whether a valid GPS location has been successfully obtained.</summary>
    public bool HasLocation
    {
        get => _hasLocation;
        set => SetProperty(ref _hasLocation, value);
    }

    /// <summary>Command to navigate back to the previous page.</summary>
    public ICommand GoBackCommand { get; }
    /// <summary>Command to refresh the current location and nearby store results.</summary>
    public ICommand RefreshLocationCommand { get; }

    /// <summary>Initialises a new instance of the <see cref="NearbyViewModel"/> class with the required location and haptic services.</summary>
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
            OnPropertyChanged(nameof(CompassStatus));
        };
        try { _compass.Start(); }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Compass unavailable: {ex.Message}"); }

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

    /// <summary>Requests location permission, fetches GPS coordinates, reverse-geocodes the address, and queries nearby stores.</summary>
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
            LocationInfo = "Unable to get your location. Please check that location services are enabled.";
        }
        finally
        {
            IsLocating = false;
        }
    }
}
