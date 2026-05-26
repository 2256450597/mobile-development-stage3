using TastyMealPlanner.ViewModels;

namespace TastyMealPlanner.Views;

public partial class RecipesPage : ContentPage
{
    public RecipesPage(RecipesViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
