using TastyMealPlanner.ViewModels;

namespace TastyMealPlanner.Views;

/// <summary>
/// Page that shows nearby places or restaurants, allowing the user to
/// discover meal options in their vicinity.
/// </summary>
public partial class NearbyPage : ContentPage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NearbyPage"/> class.
    /// </summary>
    /// <param name="viewModel">The view model that manages nearby places data.</param>
    public NearbyPage(NearbyViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
