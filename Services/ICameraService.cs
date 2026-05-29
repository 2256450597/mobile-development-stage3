namespace TastyMealPlanner.Services;

public interface ICameraService
{
    Task<PhotoResult?> CapturePhotoAsync();

    Task<PhotoResult?> PickPhotoAsync();

    bool IsCaptureSupported { get; }
}

public class PhotoResult
{
    public string FileName { get; set; } = string.Empty;
    public string FullPath { get; set; } = string.Empty;
    public Stream? Content { get; set; }
}
