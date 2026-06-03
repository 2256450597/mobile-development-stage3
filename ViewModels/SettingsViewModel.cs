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
    /// <summary>Gets or sets whether dark mode is enabled and applies the theme immediately on change.</summary>
    public bool IsDarkMode
    {
        get => _isDarkMode;
        set
        {
            if (SetProperty(ref _isDarkMode, value))
            {
                _themeService.SetTheme(value ? AppThemeOption.Dark : AppThemeOption.Light);
                OnPropertyChanged(nameof(CurrentModeText));
                OnPropertyChanged(nameof(DarkModeLabel));
                OnPropertyChanged(nameof(DarkModeIcon));
            }
        }
    }

    private string _selectedFontSize = "Medium";
    /// <summary>Gets or sets the selected font size option and applies it immediately on change.</summary>
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
                OnPropertyChanged(nameof(CurrentFontLabel));
                UpdateFontButtonColors();
            }
        }
    }

    private float _ttsPitch = 1.1f;
    /// <summary>Gets or sets the text-to-speech pitch value, updating the TTS service immediately.</summary>
    public float TtsPitch
    {
        get => _ttsPitch;
        set
        {
            if (SetProperty(ref _ttsPitch, value))
                _tts.Pitch = value;
        }
    }

    /// <summary>Gets the list of available font size display options.</summary>
    public List<string> FontSizeOptions { get; } = new() { "Small", "Medium", "Large" };

    public string CurrentModeText => IsDarkMode ? "Dark" : "Light";
    public string DarkModeLabel => IsDarkMode ? "Turn Off" : "Turn On";
    public string DarkModeIcon => IsDarkMode ? "" : "";
    public string CurrentFontLabel => SelectedFontSize;
    public string TtsButtonText => _tts.IsSpeaking ? "Stop" : "Test Speech Sample";

    // Font button backgrounds (updated by SelectFontCommand)
    private Color _fontSmallBg = Color.FromArgb("#A0603F");
    private Color _fontMediumBg = Color.FromArgb("#F5EBE4");
    private Color _fontLargeBg = Color.FromArgb("#F5EBE4");
    private Color _fontSmallText = Color.FromArgb("#FFFFFF");
    private Color _fontMediumText = Color.FromArgb("#A0603F");
    private Color _fontLargeText = Color.FromArgb("#A0603F");

    public Color FontSmallBg { get => _fontSmallBg; set => SetProperty(ref _fontSmallBg, value); }
    public Color FontMediumBg { get => _fontMediumBg; set => SetProperty(ref _fontMediumBg, value); }
    public Color FontLargeBg { get => _fontLargeBg; set => SetProperty(ref _fontLargeBg, value); }
    public Color FontSmallText { get => _fontSmallText; set => SetProperty(ref _fontSmallText, value); }
    public Color FontMediumText { get => _fontMediumText; set => SetProperty(ref _fontMediumText, value); }
    public Color FontLargeText { get => _fontLargeText; set => SetProperty(ref _fontLargeText, value); }

    private void UpdateFontButtonColors()
    {
        var activeBg = Color.FromArgb("#A0603F");
        var inactiveBg = Color.FromArgb("#F5EBE4");
        var activeText = Color.FromArgb("#FFFFFF");
        var inactiveText = Color.FromArgb("#A0603F");

        FontSmallBg = SelectedFontSize == "Small" ? activeBg : inactiveBg;
        FontSmallText = SelectedFontSize == "Small" ? activeText : inactiveText;
        FontMediumBg = SelectedFontSize == "Medium" ? activeBg : inactiveBg;
        FontMediumText = SelectedFontSize == "Medium" ? activeText : inactiveText;
        FontLargeBg = SelectedFontSize == "Large" ? activeBg : inactiveBg;
        FontLargeText = SelectedFontSize == "Large" ? activeText : inactiveText;
    }

    public ICommand ToggleDarkModeCommand { get; }
    public ICommand SelectFontCommand { get; }
    public ICommand AboutCommand { get; }
    public ICommand TestTtsCommand { get; }

    /// <summary>Initialises a new instance of the <see cref="SettingsViewModel"/> class, loading saved preferences and subscribing to external theme changes.</summary>
    public SettingsViewModel(ThemeService themeService, IHapticService haptic, ITextToSpeechService tts)
    {
        _themeService = themeService;
        _haptic = haptic;
        _tts = tts;
        Title = "Settings";

        // Load saved preferences
        _isDarkMode = _themeService.CurrentTheme == AppThemeOption.Dark;
        _selectedFontSize = _themeService.CurrentFontSize.ToString();
        UpdateFontButtonColors();
        _ttsPitch = _tts.Pitch;

        ToggleDarkModeCommand = new Command(() =>
        {
            _haptic.PerformClick();
            IsDarkMode = !IsDarkMode;
        });

        SelectFontCommand = new Command<string>(size =>
        {
            _haptic.PerformClick();
            SelectedFontSize = size ?? "Medium";
            UpdateFontButtonColors();
        });

        AboutCommand = new Command(async () =>
        {
            _haptic.PerformClick();
            await Shell.Current.DisplayAlert(
                "About Nosh Your Nosh",
                "Version 1.0\n\nA meal planning app to help you organise your weekly meals, " +
                "discover new recipes, and manage your shopping list.\n\n" +
                "Built with .NET MAUI for 6G6Z0014 Mobile Computing.\n\n" +
                "Accessibility:\n" +
                "• WCAG 2.1 AA compliant colour contrast\n" +
                "• Dark mode support\n" +
                "• Adjustable font sizes\n" +
                "• Screen reader semantic descriptions\n" +
                "• Touch targets ≥ 44pt\n\n" +
                "Hardware Features (7):\n" +
                "• Camera + Flash - capture food photos with torch control\n" +
                "• GPS Location - find nearby stores\n" +
                "• Compass - device heading direction\n" +
                "• Text-to-Speech - read recipes aloud\n" +
                "• Accelerometer - shake for random recipes\n" +
                "• Haptic Feedback + Vibration - tactile responses on every tap\n" +
                "• Native Click Sound - Android system touch sound effect",
                "OK");
        });

        TestTtsCommand = new Command(async () =>
        {
            _haptic.PerformClick();
            try
            {
                if (_tts.IsSpeaking)
                {
                    await _tts.StopAsync();
                    OnPropertyChanged(nameof(TtsButtonText));
                }
                else
                {
                    await _tts.SpeakAsync(
                        "This is a test of the text-to-speech feature. " +
                        "You can adjust the pitch to change the voice tone.",
                        1.0f, TtsPitch);
                    OnPropertyChanged(nameof(TtsButtonText));
                }
            }
            catch
            {
                await Shell.Current.DisplayAlert("TTS Error",
                    "Text-to-speech is currently unavailable. Please check your device's speech settings.", "OK");
            }
        });

        // Listen for external theme changes
        _themeService.ThemeChanged += OnExternalThemeChanged;
    }

    /// <summary>Synchronises the dark mode toggle state when the theme is changed externally.</summary>
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
