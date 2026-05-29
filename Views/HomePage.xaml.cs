using TastyMealPlanner.ViewModels;

namespace TastyMealPlanner.Views;

public partial class HomePage : ContentPage
{
    private readonly HomeViewModel _viewModel;

    public HomePage(HomeViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }
}
