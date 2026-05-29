using TastyMealPlanner.ViewModels;

namespace TastyMealPlanner.Views;

/// <summary>
/// Page that provides a camera interface for capturing food photos
/// and automatically identifying meals via the associated view model.
/// </summary>
public partial class CameraPage : ContentPage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CameraPage"/> class.
    /// </summary>
    /// <param name="viewModel">The view model that handles camera and recognition logic.</param>
    public CameraPage(CameraViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
