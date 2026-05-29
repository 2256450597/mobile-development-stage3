using System.Windows.Input;
using TastyMealPlanner.Services;

namespace TastyMealPlanner.ViewModels;

/// <summary>Manages app settings: dark mode toggle, font size selection, and TTS voice configuration.</summary>
public class SettingsViewModel : BaseViewModel
{
    private readonly ThemeService _themeService;
    private readonly IHapticService _haptic;
    private readonly ITextToSpeechService _tts;

    private bool _isDarkMode;
    public bool IsDarkMode
    {
        get => _isDarkMode;
        set
        {
            if (SetProperty(ref _isDarkMode, value))
            {
                _themeService.SetTheme(value ? AppThemeOption.Dark : AppThemeOption.Light);
            }
        }
    }

    private string _selectedFontSize = "Medium";
    public string SelectedFontSize
    {
        get => _selectedFontSize;
        set
        {
            if (SetProperty(ref _selectedFontSize, value))
            {
                var option = value switch
                {
                    "Small" => FontSizeOption.Small,
                    "Large" => FontSizeOption.Large,
                    _ => FontSizeOption.Medium
                };
                _themeService.SetFontSize(option);
            }
        }
    }

    private float _ttsPitch = 1.1f;
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

    public ICommand ToggleDarkModeCommand { get; }
    public ICommand AboutCommand { get; }
    public ICommand TestTtsCommand { get; }

    public SettingsViewModel(ThemeService themeService, IHapticService haptic, ITextToSpeechService tts)
    {
        _themeService = themeService;
        _haptic = haptic;
        _tts = tts;
        Title = "Settings";

        // Load saved preferences
        _isDarkMode = _themeService.CurrentTheme == AppThemeOption.Dark;
        _selectedFontSize = _themeService.CurrentFontSize.ToString();
        _ttsPitch = _tts.Pitch;

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
                "Accessibility:\n" +
                "• WCAG 2.1 AA compliant colour contrast\n" +
                "• Dark mode support\n" +
                "• Adjustable font sizes\n" +
                "• Screen reader semantic descriptions\n" +
                "• Touch targets ≥ 44pt\n\n" +
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
            if (_tts.IsSpeaking)
            {
                await _tts.StopAsync();
            }
            else
            {
                await _tts.SpeakAsync(
                    "This is a test of the text-to-speech feature. " +
                    "You can adjust the pitch to change the voice tone.",
                    1.0f, TtsPitch);
            }
        });

        // Listen for external theme changes
        _themeService.ThemeChanged += OnExternalThemeChanged;
    }

    private void OnExternalThemeChanged()
    {
        // Sync the toggle state if changed externally
        var expected = _themeService.CurrentTheme == AppThemeOption.Dark;
        if (_isDarkMode != expected)
        {
            _isDarkMode = expected;
            OnPropertyChanged(nameof(IsDarkMode));
        }
    }
}
