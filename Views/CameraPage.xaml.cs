using TastyMealPlanner.ViewModels;

namespace TastyMealPlanner.Views;

public partial class CameraPage : ContentPage
{
    public CameraPage(CameraViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
