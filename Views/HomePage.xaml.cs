using System.ComponentModel;
using TastyMealPlanner.Services;
using TastyMealPlanner.ViewModels;

namespace TastyMealPlanner.Views;

/// <summary>
/// The main landing page of the application. Displays the home feed,
/// shake-to-random feature, and provides navigation to other sections.
/// </summary>
public partial class HomePage : ContentPage
{
    private readonly HomeViewModel _viewModel;
    private readonly ThemeService _theme;
    private readonly IHapticService _haptic;

    /// <summary>
    /// Initializes a new instance of the <see cref="HomePage"/> class.
    /// </summary>
    /// <param name="viewModel">The view model driving the home page logic.</param>
    /// <param name="theme">Service for applying theme and font scaling.</param>
    /// <param name="haptic">Service for triggering haptic feedback.</param>
    public HomePage(HomeViewModel viewModel, ThemeService theme, IHapticService haptic)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
        _theme = theme;
        _haptic = haptic;
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    /// <summary>
    /// Handles property changes on the view model. When a shake result appears,
    /// triggers vibration and haptic long-press feedback to alert the user.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">Property change event arguments.</param>
    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(HomeViewModel.HasShakeResult) && _viewModel.HasShakeResult)
        {
            // Shake card just appeared — trigger vibration/haptic (runs on UI binding thread)
            try { Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(450)); } catch { }
            try { HapticFeedback.Default.Perform(HapticFeedbackType.LongPress); } catch { }
        }
    }

    /// <summary>
    /// Called when the page appears. Reloads the view model data and
    /// applies the current font scale to the page.
    /// </summary>
    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.Reload();
        _theme.ApplyFontScaleToPage(this);
    }

    /// <summary>
    /// Handles the tap gesture on the logo. Triggers haptic click feedback
    /// and navigates to the home shell route.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">Event arguments.</param>
    private async void OnLogoTapped(object? sender, EventArgs e)
    {
        _haptic.PerformClick();
        await Shell.Current.GoToAsync("//home");
    }
}
