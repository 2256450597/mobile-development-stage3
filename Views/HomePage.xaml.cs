using System.ComponentModel;
using TastyMealPlanner.Services;
using TastyMealPlanner.ViewModels;

namespace TastyMealPlanner.Views;

public partial class HomePage : ContentPage
{
    private readonly HomeViewModel _viewModel;
    private readonly ThemeService _theme;
    private readonly IHapticService _haptic;

    public HomePage(HomeViewModel viewModel, ThemeService theme, IHapticService haptic)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
        _theme = theme;
        _haptic = haptic;
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(HomeViewModel.HasShakeResult) && _viewModel.HasShakeResult)
        {
            // Shake card just appeared — trigger vibration/haptic (runs on UI binding thread)
            try { Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(450)); } catch { }
            try { HapticFeedback.Default.Perform(HapticFeedbackType.LongPress); } catch { }
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.Reload();
        _theme.ApplyFontScaleToPage(this);
    }

    private async void OnLogoTapped(object? sender, EventArgs e)
    {
        _haptic.PerformClick();
        await Shell.Current.GoToAsync("//home");
    }
}
