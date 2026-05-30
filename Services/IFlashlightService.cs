namespace TastyMealPlanner.Services;

/// <summary>Controls the device flashlight/torch for use as camera flash or illumination.</summary>
public interface IFlashlightService
{
    bool IsSupported { get; }
    bool IsOn { get; }
    void Toggle();
    void TurnOn();
    void TurnOff();
}
