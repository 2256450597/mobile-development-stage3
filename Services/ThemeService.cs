namespace TastyMealPlanner.Services;

public enum AppThemeOption { Light, Dark }

public enum FontSizeOption { Small, Medium, Large }

public class ThemeService
{
    private const string ThemeKey = "app_theme";
    private const string FontSizeKey = "font_size";

    public event Action? ThemeChanged;
    public event Action? FontSizeChanged;

    public AppThemeOption CurrentTheme { get; private set; } = AppThemeOption.Light;
    public FontSizeOption CurrentFontSize { get; private set; } = FontSizeOption.Medium;

    public double FontScale => CurrentFontSize switch
    {
        FontSizeOption.Small => 0.85,
        FontSizeOption.Large => 1.3,
        _ => 1.0
    };

    public ThemeService()
    {
        LoadPreferences();
    }

    public void SetTheme(AppThemeOption theme)
    {
        if (CurrentTheme == theme) return;
        CurrentTheme = theme;
        ApplySystemTheme();
        SavePreference(ThemeKey, theme.ToString());
        ThemeChanged?.Invoke();
    }

    public void ToggleTheme()
    {
        SetTheme(CurrentTheme == AppThemeOption.Light
            ? AppThemeOption.Dark
            : AppThemeOption.Light);
    }

    public void SetFontSize(FontSizeOption size)
    {
        if (CurrentFontSize == size) return;
        CurrentFontSize = size;
        SavePreference(FontSizeKey, size.ToString());
        FontSizeChanged?.Invoke();
    }

    public void ApplySystemTheme()
    {
        if (Application.Current != null)
        {
            Application.Current.UserAppTheme = CurrentTheme switch
            {
                AppThemeOption.Dark => AppTheme.Dark,
                _ => AppTheme.Light
            };
        }
    }

    private void LoadPreferences()
    {
        var themeStr = Preferences.Default.Get(ThemeKey, "Light");
        CurrentTheme = Enum.TryParse<AppThemeOption>(themeStr, out var t) ? t : AppThemeOption.Light;

        var fontSizeStr = Preferences.Default.Get(FontSizeKey, "Medium");
        CurrentFontSize = Enum.TryParse<FontSizeOption>(fontSizeStr, out var f) ? f : FontSizeOption.Medium;

        ApplySystemTheme();
    }

    private static void SavePreference(string key, string value)
        => Preferences.Default.Set(key, value);

    public double GetScaledFontSize(double baseSize)
        => baseSize * FontScale;

    public double TitleSize => GetScaledFontSize(24);
    public double SubtitleSize => GetScaledFontSize(18);
    public double BodySize => GetScaledFontSize(14);
    public double CaptionSize => GetScaledFontSize(12);
    public double SectionTitleSize => GetScaledFontSize(20);
}
