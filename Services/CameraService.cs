namespace TastyMealPlanner.Services;

/// <summary>Wraps MAUI MediaPicker for camera capture and gallery selection with permission handling.</summary>
public class CameraService : ICameraService
{
    /// <summary>Indicates whether the device supports taking photos via the camera.</summary>
    public bool IsCaptureSupported => MediaPicker.Default.IsCaptureSupported;

    /// <summary>Checks and requests camera permission from the user if not already granted.</summary>
    /// <returns>True if camera permission is granted; otherwise false.</returns>
    public async Task<bool> RequestCameraPermissionAsync()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
        if (status != PermissionStatus.Granted)
        {
            status = await Permissions.RequestAsync<Permissions.Camera>();
        }
        return status == PermissionStatus.Granted;
    }

    /// <summary>Opens the device camera to capture a photo and returns the result saved to app cache.</summary>
    /// <returns>A PhotoResult with the captured file info, or null if the user cancelled.</returns>
    public async Task<PhotoResult?> CapturePhotoAsync()
    {
        // Request permission before attempting capture
        var hasPermission = await RequestCameraPermissionAsync();
        if (!hasPermission)
            throw new PermissionException("Camera permission was denied. Please grant it in device settings.");

        if (!MediaPicker.Default.IsCaptureSupported)
            throw new InvalidOperationException("Camera capture is not supported on this device.");

        try
        {
            var photo = await MediaPicker.Default.CapturePhotoAsync();
            if (photo == null) return null;

            // Copy to app cache for reliable access across Android versions
            var cachedPath = Path.Combine(FileSystem.CacheDirectory, photo.FileName);
            using var sourceStream = await photo.OpenReadAsync();
            using var destStream = File.Create(cachedPath);
            await sourceStream.CopyToAsync(destStream);

            return new PhotoResult
            {
                FileName = photo.FileName,
                FullPath = cachedPath,
            };
        }
        catch (PermissionException)
        {
            throw new InvalidOperationException("Camera permission was denied. Please enable it in device settings.");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Could not capture photo: {ex.Message}", ex);
        }
    }

    /// <summary>Opens the device gallery for the user to pick an existing photo and returns the result saved to app cache.</summary>
    /// <returns>A PhotoResult with the selected file info, or null if the user cancelled.</returns>
    public async Task<PhotoResult?> PickPhotoAsync()
    {
        try
        {
            var photo = await MediaPicker.Default.PickPhotoAsync();
            if (photo == null) return null;

            // Copy to app cache for reliable access
            var cachedPath = Path.Combine(FileSystem.CacheDirectory, photo.FileName);
            using var sourceStream = await photo.OpenReadAsync();
            using var destStream = File.Create(cachedPath);
            await sourceStream.CopyToAsync(destStream);

            return new PhotoResult
            {
                FileName = photo.FileName,
                FullPath = cachedPath,
            };
        }
        catch (PermissionException)
        {
            throw new InvalidOperationException("Storage permission is required to pick photos.");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Could not pick photo: {ex.Message}", ex);
        }
    }
}
