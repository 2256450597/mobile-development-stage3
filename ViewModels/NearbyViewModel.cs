using System.Collections.ObjectModel;
using System.Windows.Input;

namespace TastyMealPlanner.ViewModels;

public class NearbyViewModel : BaseViewModel
{
    public ICommand GoBackCommand { get; }
    public ICommand RefreshLocationCommand { get; }

    private string _locationInfo = "Fetching location...";
    public string LocationInfo
    {
        get => _locationInfo;
        set => SetProperty(ref _locationInfo, value);
    }

    private bool _isLocating;
    public bool IsLocating
    {
        get => _isLocating;
        set => SetProperty(ref _isLocating, value);
    }

    public NearbyViewModel()
    {
        Title = "Nearby Stores";

        GoBackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
        RefreshLocationCommand = new Command(async () => await GetLocationAsync());
    }

    private async Task GetLocationAsync()
    {
        try
        {
            IsLocating = true;
            LocationInfo = "Fetching location...";

            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                if (status != PermissionStatus.Granted)
                {
                    LocationInfo = "Location permission denied.";
                    return;
                }
            }

            var location = await Geolocation.Default.GetLocationAsync(new GeolocationRequest
            {
                DesiredAccuracy = GeolocationAccuracy.Medium,
                Timeout = TimeSpan.FromSeconds(10)
            });

            if (location != null)
            {
                LocationInfo = $"Lat: {location.Latitude:F4}, Lon: {location.Longitude:F4}\n" +
                               $"Showing grocery stores near you...";
            }
            else
            {
                LocationInfo = "Unable to get location.";
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
