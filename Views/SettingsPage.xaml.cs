using TastyMealPlanner.Services;
using TastyMealPlanner.ViewModels;

namespace TastyMealPlanner.Views;

/// <summary>
/// Page that allows the user to configure application settings such as
/// theme, font size, and other preferences.
/// </summary>
public partial class SettingsPage : ContentPage
{
    private readonly ThemeService _theme;

    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsPage"/> class.
    /// Subscribes to font size changes so the page rescales dynamically.
    /// </summary>
    /// <param name="viewModel">The view model that manages settings state.</param>
    /// <param name="theme">Service for applying theme and font scaling.</param>
    public SettingsPage(SettingsViewModel viewModel, ThemeService theme)
    {
        InitializeComponent();
        _theme = theme;
        BindingContext = viewModel;
        _theme.FontSizeChanged += () => _theme.ApplyFontScaleToPage(this);
    }

    /// <summary>
    /// Called when the page appears. Applies the current font scale to the page.
    /// </summary>
    protected override void OnAppearing()
    {
        base.OnAppearing();
        _theme.ApplyFontScaleToPage(this);
    }
}
