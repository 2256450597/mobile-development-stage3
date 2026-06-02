namespace TastyMealPlanner.Services;

/// <summary>Controls the device flashlight/torch using platform-specific APIs.
/// Android: CameraManager.SetTorchMode.
/// iOS/macOS: AVCaptureDevice.TorchMode.
/// Windows: not supported (returns false for IsSupported).</summary>
public class FlashlightService : IFlashlightService
{
    public bool IsSupported { get; }
    public bool IsOn { get; private set; }

    public FlashlightService()
    {
#if ANDROID
        IsSupported = AndroidFlashlight.IsAvailable();
#elif IOS || MACCATALYST
        IsSupported = AppleFlashlight.IsAvailable();
#else
        IsSupported = false;
#endif
    }

    public void Toggle()
    {
        if (!IsSupported) return;

        if (IsOn) TurnOff();
        else TurnOn();
    }

    public void TurnOn()
    {
        if (!IsSupported || IsOn) return;

#if ANDROID
        AndroidFlashlight.TurnOn();
#elif IOS || MACCATALYST
        AppleFlashlight.TurnOn();
#endif
        IsOn = true;
    }

    public void TurnOff()
    {
        if (!IsSupported || !IsOn) return;

#if ANDROID
        AndroidFlashlight.TurnOff();
#elif IOS || MACCATALYST
        AppleFlashlight.TurnOff();
#endif
        IsOn = false;
    }
}

#if ANDROID
/// <summary>Android-specific torch control via CameraManager.</summary>
internal static class AndroidFlashlight
{
    private static Android.Hardware.Camera2.CameraManager? _cameraManager;
    private static string? _cameraId;

    public static bool IsAvailable()
    {
        try
        {
            var context = Android.App.Application.Context;
            _cameraManager = (Android.Hardware.Camera2.CameraManager)
                context.GetSystemService(Android.Content.Context.CameraService)!;
            var ids = _cameraManager.GetCameraIdList();
            foreach (var id in ids)
            {
                var characteristics = _cameraManager.GetCameraCharacteristics(id);
                var facing = (int?)characteristics.Get(
                    Android.Hardware.Camera2.CameraCharacteristics.LensFacing);
                var hasFlash = (bool?)characteristics.Get(
                    Android.Hardware.Camera2.CameraCharacteristics.FlashInfoAvailable);

                // Prefer back camera with flash
                if (facing == (int)Android.Hardware.Camera2.LensFacing.Back && hasFlash == true)
                {
                    _cameraId = id;
                    return true;
                }
            }
            return false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Flashlight availability check failed: {ex.Message}");
            return false;
        }
    }

    public static void TurnOn()
    {
        if (_cameraManager == null || _cameraId == null) return;
        _cameraManager.SetTorchMode(_cameraId, true);
    }

    public static void TurnOff()
    {
        if (_cameraManager == null || _cameraId == null) return;
        _cameraManager.SetTorchMode(_cameraId, false);
    }
}
#endif

#if IOS || MACCATALYST
/// <summary>iOS/macOS torch control via AVCaptureDevice.</summary>
internal static class AppleFlashlight
{
    private static AVFoundation.AVCaptureDevice? _device;

    public static bool IsAvailable()
    {
        _device = AVFoundation.AVCaptureDevice.GetDefaultDevice(
            AVFoundation.AVMediaTypes.Video);
        return _device?.HasTorch == true;
    }

    public static void TurnOn()
    {
        if (_device == null || !_device.HasTorch) return;
        if (_device.LockForConfiguration(out _))
        {
            _device.TorchMode = AVFoundation.AVCaptureTorchMode.On;
            _device.UnlockForConfiguration();
        }
    }

    public static void TurnOff()
    {
        if (_device == null || !_device.HasTorch) return;
        if (_device.LockForConfiguration(out _))
        {
            _device.TorchMode = AVFoundation.AVCaptureTorchMode.Off;
            _device.UnlockForConfiguration();
        }
    }
}
#endif
