using System.Windows.Input;

namespace TastyMealPlanner.ViewModels;

public class SettingsViewModel : BaseViewModel
{
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

    public List<string> FontSizeOptions { get; } = new() { "Small", "Medium", "Large" };

    public ICommand ToggleDarkModeCommand { get; }
    public ICommand AboutCommand { get; }

    public SettingsViewModel()
    {
        Title = "Settings";

        ToggleDarkModeCommand = new Command(() => IsDarkMode = !IsDarkMode);

        AboutCommand = new Command(async () =>
        {
            await Shell.Current.DisplayAlert(
                "About Tasty Meal Planner",
                "Version 1.0\n\nA meal planning app to help you organise your weekly meals, " +
                "discover new recipes, and manage your shopping list.\n\n" +
                "Built with .NET MAUI for 6G6Z0014 Mobile Computing.",
                "OK");
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
        // Font size adjustment will be implemented in Step 3 (Themes)
        System.Diagnostics.Debug.WriteLine($"Font size changed to: {size}");
    }
}
