namespace TastyMealPlanner.Services;

/// <summary>
/// Abstracts camera operations including photo capture and gallery selection.
/// </summary>
public interface ICameraService
{
    /// <summary>Captures a new photo using the device camera. Throws on permission denial.</summary>
    Task<PhotoResult?> CapturePhotoAsync();

    /// <summary>Picks an existing photo from the device gallery. Throws on permission denial.</summary>
    Task<PhotoResult?> PickPhotoAsync();

    /// <summary>Indicates whether the device supports camera capture.</summary>
    bool IsCaptureSupported { get; }
}

/// <summary>
/// Represents the result of a camera capture or gallery pick operation.
/// </summary>
public class PhotoResult
{
    public string FileName { get; set; } = string.Empty;
    public string FullPath { get; set; } = string.Empty;
    public Stream? Content { get; set; }
}
