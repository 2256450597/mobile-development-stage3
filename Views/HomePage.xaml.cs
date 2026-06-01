using TastyMealPlanner.Helpers;
using TastyMealPlanner.Services;
using TastyMealPlanner.ViewModels;

namespace TastyMealPlanner.Views;

public partial class HomePage : ContentPage
{
    private readonly HomeViewModel _viewModel;
    private readonly ThemeService _theme;
    private readonly IHapticService _haptic;
    private CarouselAutoPlayer? _carouselPlayer;

    public HomePage(HomeViewModel viewModel, ThemeService theme, IHapticService haptic)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
        _theme = theme;
        _haptic = haptic;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await Task.Delay(80);
        _viewModel.Reload();
        _theme.ApplyFontScaleToPage(this);
        _carouselPlayer = new CarouselAutoPlayer(HeroCarousel);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _carouselPlayer?.Dispose();
        _carouselPlayer = null;
    }

    private async void OnLogoTapped(object? sender, EventArgs e)
    {
        _haptic.PerformClick();
        await Shell.Current.GoToAsync("//home");
    }
}
