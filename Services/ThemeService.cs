namespace TastyMealPlanner.Services;

public enum AppThemeOption { Light, Dark }

public enum FontSizeOption { Small, Medium, Large }

/// <summary>Central coordinator for app theme (light/dark) and font size preferences. Persists using MAUI Preferences API.</summary>
public class ThemeService
{
    private const string ThemeKey = "app_theme";
    private const string FontSizeKey = "font_size";

    /// <summary>Raised when the app theme (light/dark) is changed.</summary>
    public event Action? ThemeChanged;

    /// <summary>Raised when the font size setting is changed.</summary>
    public event Action? FontSizeChanged;

    /// <summary>Gets the current theme option (Light or Dark).</summary>
    public AppThemeOption CurrentTheme { get; private set; } = AppThemeOption.Light;

    /// <summary>Gets the current font size option (Small, Medium, or Large).</summary>
    public FontSizeOption CurrentFontSize { get; private set; } = FontSizeOption.Medium;

    /// <summary>Returns the numeric scale factor for the current font size: 0.7 for Small, 1.4 for Large, and 1.0 for Medium.</summary>
    public double FontScale => CurrentFontSize switch
    {
        FontSizeOption.Small => 0.70,
        FontSizeOption.Large => 1.40,
        _ => 1.0
    };

    /// <summary>Initialises the service by loading saved theme and font size preferences.</summary>
    public ThemeService()
    {
        LoadPreferences();
    }

    /// <summary>Switches between light and dark theme and persists the choice.</summary>
    public void SetTheme(AppThemeOption theme)
    {
        if (CurrentTheme == theme) return;
        CurrentTheme = theme;
        ApplySystemTheme();
        SavePreference(ThemeKey, theme.ToString());
        ThemeChanged?.Invoke();
    }

    /// <summary>Toggles the current theme between Light and Dark.</summary>
    public void ToggleTheme()
    {
        SetTheme(CurrentTheme == AppThemeOption.Light
            ? AppThemeOption.Dark
            : AppThemeOption.Light);
    }

    /// <summary>Sets the font size level and notifies listeners of the change.</summary>
    public void SetFontSize(FontSizeOption size)
    {
        if (CurrentFontSize == size) return;
        CurrentFontSize = size;
        SavePreference(FontSizeKey, size.ToString());
        FontSizeChanged?.Invoke();
    }

    /// <summary>Applies the current theme setting to the MAUI application's UserAppTheme.</summary>
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

    /// <summary>Loads persisted theme and font size preferences from MAUI Preferences and applies the theme.</summary>
    private void LoadPreferences()
    {
        var themeStr = Preferences.Default.Get(ThemeKey, "Light");
        CurrentTheme = Enum.TryParse<AppThemeOption>(themeStr, out var t) ? t : AppThemeOption.Light;

        var fontSizeStr = Preferences.Default.Get(FontSizeKey, "Medium");
        CurrentFontSize = Enum.TryParse<FontSizeOption>(fontSizeStr, out var f) ? f : FontSizeOption.Medium;

        ApplySystemTheme();
    }

    /// <summary>Saves a string preference value to MAUI Preferences.</summary>
    /// <param name="key">The preference key.</param>
    /// <param name="value">The preference value to store.</param>
    private static void SavePreference(string key, string value)
        => Preferences.Default.Set(key, value);

    /// <summary>Returns a font size multiplied by the current scale factor.</summary>
    public double GetScaledFontSize(double baseSize)
        => baseSize * FontScale;

    /// <summary>Gets the title font size (24pt scaled by FontScale).</summary>
    public double TitleSize => GetScaledFontSize(24);

    /// <summary>Gets the subtitle font size (18pt scaled by FontScale).</summary>
    public double SubtitleSize => GetScaledFontSize(18);

    /// <summary>Gets the body text font size (14pt scaled by FontScale).</summary>
    public double BodySize => GetScaledFontSize(14);

    /// <summary>Gets the caption font size (12pt scaled by FontScale).</summary>
    public double CaptionSize => GetScaledFontSize(12);

    /// <summary>Gets the section title font size (20pt scaled by FontScale).</summary>
    public double SectionTitleSize => GetScaledFontSize(20);

    /// <summary>Recursively scales font sizes on all text elements in a page.</summary>
    public void ApplyFontScaleToPage(Element root)
    {
        var descendants = root.GetVisualTreeDescendants();
        foreach (var element in descendants)
        {
            switch (element)
            {
                case Label label:
                    label.FontSize = GetBaseFontSize(label) * FontScale;
                    break;
                case Button button:
                    button.FontSize = GetBaseFontSize(button) * FontScale;
                    break;
                case Entry entry:
                    entry.FontSize = GetBaseFontSize(entry) * FontScale;
                    break;
                case SearchBar search:
                    search.FontSize = GetBaseFontSize(search) * FontScale;
                    break;
                case Picker picker:
                    picker.FontSize = GetBaseFontSize(picker) * FontScale;
                    break;
            }
        }
    }

    private readonly Dictionary<string, double> _originalFontSizes = new();

    /// <summary>Returns the original (unscaled) font size for a visual element, caching it on first access.</summary>
    /// <param name="el">The visual element to query.</param>
    /// <returns>The base font size in device-independent pixels.</returns>
    private double GetBaseFontSize(Element el)
    {
        var key = $"{el.GetType().Name}_{el.Id}";
        if (!_originalFontSizes.ContainsKey(key))
        {
            var current = el switch
            {
                Label l => l.FontSize,
                Button b => b.FontSize,
                Entry e => e.FontSize,
                SearchBar s => s.FontSize,
                Picker p => p.FontSize,
                _ => 14.0
            };
            _originalFontSizes[key] = current > 0 ? current : 14;
        }
        return _originalFontSizes[key];
    }
}
