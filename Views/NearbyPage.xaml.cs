using TastyMealPlanner.ViewModels;

namespace TastyMealPlanner.Views;

public partial class NearbyPage : ContentPage
{
    public NearbyPage(NearbyViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
