using System.Windows.Input;
using TastyMealPlanner.Services;

namespace TastyMealPlanner.ViewModels;

public class SettingsViewModel : BaseViewModel
{
    private readonly IHapticService _haptic;
    private readonly ITextToSpeechService _tts;

    private bool _isDarkMode;
    public bool IsDarkMode
    {
        get => _isDarkMode;
        set
        {
            if (SetProperty(ref _isDarkMode, value))
                ApplyTheme(value);
        }
    }

    private string _selectedFontSize = "Medium";
    public string SelectedFontSize
    {
        get => _selectedFontSize;
        set
        {
            if (SetProperty(ref _selectedFontSize, value))
                ApplyFontSize(value);
        }
    }

    private float _ttsSpeed = 1.0f;
    public float TtsSpeed
    {
        get => _ttsSpeed;
        set
        {
            if (SetProperty(ref _ttsSpeed, value))
                _tts.Speed = value;
        }
    }

    private float _ttsPitch = 1.0f;
    public float TtsPitch
    {
        get => _ttsPitch;
        set
        {
            if (SetProperty(ref _ttsPitch, value))
                _tts.Pitch = value;
        }
    }

    public List<string> FontSizeOptions { get; } = new() { "Small", "Medium", "Large" };

    public List<float> SpeedOptions { get; } = new() { 0.5f, 0.75f, 1.0f, 1.25f, 1.5f, 2.0f };

    public ICommand ToggleDarkModeCommand { get; }
    public ICommand AboutCommand { get; }
    public ICommand TestTtsCommand { get; }

    public SettingsViewModel(IHapticService haptic, ITextToSpeechService tts)
    {
        _haptic = haptic;
        _tts = tts;
        Title = "Settings";

        ToggleDarkModeCommand = new Command(() =>
        {
            _haptic.PerformClick();
            IsDarkMode = !IsDarkMode;
        });

        AboutCommand = new Command(async () =>
        {
            _haptic.PerformClick();
            await Shell.Current.DisplayAlert(
                "About Tasty Meal Planner",
                "Version 1.0\n\nA meal planning app to help you organise your weekly meals, " +
                "discover new recipes, and manage your shopping list.\n\n" +
                "Built with .NET MAUI for 6G6Z0014 Mobile Computing.\n\n" +
                "Hardware Features:\n" +
                "• Camera - capture food photos\n" +
                "• GPS Location - find nearby stores\n" +
                "• Text-to-Speech - read recipes aloud\n" +
                "• Accelerometer - shake for random recipes\n" +
                "• Haptic Feedback - tactile responses",
                "OK");
        });

        TestTtsCommand = new Command(async () =>
        {
            _haptic.PerformClick();
            await _tts.SpeakAsync("This is a test of the text-to-speech feature.", TtsSpeed, TtsPitch);
        });
    }

    private void ApplyTheme(bool isDark)
    {
        if (Application.Current != null)
        {
            Application.Current.UserAppTheme = isDark ? AppTheme.Dark : AppTheme.Light;
        }
    }

    private void ApplyFontSize(string size)
    {
        System.Diagnostics.Debug.WriteLine($"Font size changed to: {size}");
    }
}
