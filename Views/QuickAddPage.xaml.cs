using TastyMealPlanner.ViewModels;

namespace TastyMealPlanner.Views;

/// <summary>
/// Page that provides a quick input form for adding a new meal or recipe
/// without navigating through the full creation flow.
/// </summary>
public partial class QuickAddPage : ContentPage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="QuickAddPage"/> class.
    /// </summary>
    /// <param name="viewModel">The view model that handles quick-add logic.</param>
    public QuickAddPage(QuickAddViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
