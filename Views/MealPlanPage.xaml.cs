using TastyMealPlanner.Services;
using TastyMealPlanner.ViewModels;

namespace TastyMealPlanner.Views;

/// <summary>
/// Page that displays the user's weekly meal plan, allowing them to view,
/// assign, and manage meals for each day of the week.
/// </summary>
public partial class MealPlanPage : ContentPage
{
    private readonly MealPlanViewModel _viewModel;
    private readonly ThemeService _theme;

    /// <summary>
    /// Initializes a new instance of the <see cref="MealPlanPage"/> class.
    /// </summary>
    /// <param name="viewModel">The view model that manages the weekly meal plan data.</param>
    /// <param name="theme">Service for applying theme and font scaling.</param>
    public MealPlanPage(MealPlanViewModel viewModel, ThemeService theme)
    {
        InitializeComponent();
        _theme = theme;
        BindingContext = _viewModel = viewModel;
    }

    /// <summary>
    /// Called when the page appears. Applies the current font scale and
    /// loads the week plan from the view model.
    /// </summary>
    protected override void OnAppearing()
    {
        base.OnAppearing();
        _theme.ApplyFontScaleToPage(this);
        _viewModel.LoadWeekPlan();
    }
}
