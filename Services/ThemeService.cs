namespace TastyMealPlanner.Services;

public enum AppThemeOption { Light, Dark }

public enum FontSizeOption { Small, Medium, Large }

/// <summary>Central coordinator for app theme (light/dark) and font size preferences. Persists using MAUI Preferences API.</summary>
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

    /// <summary>Switches between light and dark theme and persists the choice.</summary>
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

    /// <summary>Sets the font size level and notifies listeners of the change.</summary>
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

    /// <summary>Returns a font size multiplied by the current scale factor.</summary>
    public double GetScaledFontSize(double baseSize)
        => baseSize * FontScale;

    public double TitleSize => GetScaledFontSize(24);
    public double SubtitleSize => GetScaledFontSize(18);
    public double BodySize => GetScaledFontSize(14);
    public double CaptionSize => GetScaledFontSize(12);
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
