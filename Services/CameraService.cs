namespace TastyMealPlanner.Services;

/// <summary>Wraps MAUI MediaPicker for camera capture and gallery selection with permission handling.</summary>
public class CameraService : ICameraService
{
    public bool IsCaptureSupported => MediaPicker.Default.IsCaptureSupported;

    public async Task<PhotoResult?> CapturePhotoAsync()
    {
        if (!MediaPicker.Default.IsCaptureSupported)
            return null;

        try
        {
            var photo = await MediaPicker.Default.CapturePhotoAsync();
            if (photo == null) return null;

            return new PhotoResult
            {
                FileName = photo.FileName,
                FullPath = photo.FullPath,
            };
        }
        catch (PermissionException)
        {
            throw new InvalidOperationException("Camera permission is required to take photos.");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to capture photo: {ex.Message}", ex);
        }
    }

    public async Task<PhotoResult?> PickPhotoAsync()
    {
        try
        {
            var photo = await MediaPicker.Default.PickPhotoAsync();
            if (photo == null) return null;

            return new PhotoResult
            {
                FileName = photo.FileName,
                FullPath = photo.FullPath,
            };
        }
        catch (PermissionException)
        {
            throw new InvalidOperationException("Storage permission is required to pick photos.");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to pick photo: {ex.Message}", ex);
        }
    }
}
