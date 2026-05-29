using TastyMealPlanner.ViewModels;

namespace TastyMealPlanner.Views;

public partial class QuickAddPage : ContentPage
{
    public QuickAddPage(QuickAddViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
