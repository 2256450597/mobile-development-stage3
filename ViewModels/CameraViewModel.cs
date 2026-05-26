using System.Windows.Input;
using TastyMealPlanner.Services;

namespace TastyMealPlanner.ViewModels;

public class CameraViewModel : BaseViewModel
{
    private readonly ICameraService _cameraService;
    private readonly IHapticService _haptic;

    private bool _hasCamera;
    public bool HasCamera
    {
        get => _hasCamera;
        set => SetProperty(ref _hasCamera, value);
    }

    private string _statusMessage = "Ready to capture";
    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    private string _photoPath = string.Empty;
    public string PhotoPath
    {
        get => _photoPath;
        set => SetProperty(ref _photoPath, value);
    }

    private bool _hasPhoto;
    public bool HasPhoto
    {
        get => _hasPhoto;
        set => SetProperty(ref _hasPhoto, value);
    }

    public ICommand TakePhotoCommand { get; }
    public ICommand PickPhotoCommand { get; }
    public ICommand ClearPhotoCommand { get; }
    public ICommand GoBackCommand { get; }

    public CameraViewModel(ICameraService cameraService, IHapticService haptic)
    {
        _cameraService = cameraService;
        _haptic = haptic;
        Title = "Camera";
        HasCamera = _cameraService.IsCaptureSupported;

        TakePhotoCommand = new Command(async () => await OnTakePhoto());
        PickPhotoCommand = new Command(async () => await OnPickPhoto());
        ClearPhotoCommand = new Command(() =>
        {
            PhotoPath = string.Empty;
            HasPhoto = false;
            StatusMessage = "Ready to capture";
        });

        GoBackCommand = new Command(async () =>
        {
            _haptic.PerformClick();
            await Shell.Current.GoToAsync("..");
        });
    }

    private async Task OnTakePhoto()
    {
        _haptic.PerformClick();

        if (!_cameraService.IsCaptureSupported)
        {
            StatusMessage = "Camera not available on this device.";
            return;
        }

        StatusMessage = "Opening camera...";
        try
        {
            var result = await _cameraService.CapturePhotoAsync();
            if (result != null)
            {
                PhotoPath = result.FullPath;
                HasPhoto = true;
                StatusMessage = $"Photo captured: {result.FileName}";
            }
            else
            {
                StatusMessage = "Photo capture was cancelled.";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            await Shell.Current.DisplayAlert("Camera Error", ex.Message, "OK");
        }
    }

    private async Task OnPickPhoto()
    {
        _haptic.PerformClick();
        StatusMessage = "Opening gallery...";

        try
        {
            var result = await _cameraService.PickPhotoAsync();
            if (result != null)
            {
                PhotoPath = result.FullPath;
                HasPhoto = true;
                StatusMessage = $"Photo selected: {result.FileName}";
            }
            else
            {
                StatusMessage = "No photo was selected.";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            await Shell.Current.DisplayAlert("Gallery Error", ex.Message, "OK");
        }
    }
}
