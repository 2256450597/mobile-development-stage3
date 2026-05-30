using System.Windows.Input;
using TastyMealPlanner.Services;

namespace TastyMealPlanner.ViewModels;

/// <summary>Handles camera capture and gallery pick operations with permission and error handling.</summary>
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
    public ICommand SaveToGalleryCommand { get; }
    public ICommand UsePhotoCommand { get; }
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
            _haptic.PerformClick();
            PhotoPath = string.Empty;
            HasPhoto = false;
            StatusMessage = "Ready to capture";
        });

        SaveToGalleryCommand = new Command(async () => await OnSaveToGallery());

        UsePhotoCommand = new Command(async () =>
        {
            _haptic.PerformClick();
            if (!string.IsNullOrEmpty(PhotoPath))
            {
                var encoded = Uri.EscapeDataString(PhotoPath);
                await Shell.Current.GoToAsync($"quickadd?photo={encoded}");
            }
        });

        GoBackCommand = new Command(async () =>
        {
            _haptic.PerformClick();
            await Shell.Current.GoToAsync("..");
        });
    }

    /// <summary>Opens the device camera to capture a photo. Stores the full file path on success.</summary>
    private async Task OnTakePhoto()
    {
        _haptic.PerformClick();

        if (!_cameraService.IsCaptureSupported)
        {
            StatusMessage = "Camera not available. This device does not support photo capture.";
            return;
        }

        StatusMessage = "Requesting camera permission...";
        try
        {
            var result = await _cameraService.CapturePhotoAsync();
            if (result != null)
            {
                PhotoPath = result.FullPath;
                HasPhoto = true;
                StatusMessage = "Photo captured successfully.";
            }
            else
            {
                StatusMessage = "Camera was closed without capturing a photo.";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
        }
    }

    private async Task OnSaveToGallery()
    {
        _haptic.PerformClick();

        if (string.IsNullOrEmpty(PhotoPath) || !File.Exists(PhotoPath))
        {
            StatusMessage = "No photo to save.";
            return;
        }

        try
        {
            // Copy to device Pictures folder so it shows in the Gallery app
            var fileName = $"tasty_{DateTime.Now:yyyyMMdd_HHmmss}.jpg";
            var publicDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), fileName);
            File.Copy(PhotoPath, publicDir, overwrite: true);
            StatusMessage = "Photo saved to Pictures folder.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Could not save: {ex.Message}";
        }
    }

    /// <summary>Opens the device gallery to select an existing photo.</summary>
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
                StatusMessage = "Photo selected from gallery.";
            }
            else
            {
                StatusMessage = "No photo was selected.";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
        }
    }
}
