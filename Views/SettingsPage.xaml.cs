using TastyMealPlanner.Services;
using TastyMealPlanner.ViewModels;

namespace TastyMealPlanner.Views;

public partial class SettingsPage : ContentPage
{
    private readonly ThemeService _theme;

    public SettingsPage(SettingsViewModel viewModel, ThemeService theme)
    {
        InitializeComponent();
        _theme = theme;
        BindingContext = viewModel;
    }
}
