using System.Windows.Input;

namespace TastyMealPlanner.ViewModels;

public class CameraViewModel : BaseViewModel
{
    public ICommand TakePhotoCommand { get; }
    public ICommand GoBackCommand { get; }

    public CameraViewModel()
    {
        Title = "Camera";

        TakePhotoCommand = new Command(async () => await OnTakePhoto());
        GoBackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
    }

    private static async Task OnTakePhoto()
    {
        try
        {
            if (!MediaPicker.Default.IsCaptureSupported)
            {
                await Shell.Current.DisplayAlert("Error", "Camera is not supported on this device.", "OK");
                return;
            }

            var photo = await MediaPicker.Default.CapturePhotoAsync();
            if (photo != null)
            {
                await Shell.Current.DisplayAlert("Success", $"Photo saved: {photo.FileName}", "OK");
            }
        }
        catch (PermissionException)
        {
            await Shell.Current.DisplayAlert("Permission Denied", "Camera permission is required.", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Failed to take photo: {ex.Message}", "OK");
        }
    }
}
