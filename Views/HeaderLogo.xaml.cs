namespace TastyMealPlanner.Views;

/// <summary>
/// A custom ContentView that displays the application logo in the header.
/// Tapping the logo navigates the user back to the home page.
/// </summary>
public partial class HeaderLogo : ContentView
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HeaderLogo"/> class.
    /// </summary>
    public HeaderLogo()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Handles the tap gesture on the logo. Triggers haptic feedback and
    /// navigates to the home page.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">Event arguments.</param>
    private async void OnLogoTapped(object? sender, EventArgs e)
    {
        try { HapticFeedback.Default.Perform(HapticFeedbackType.Click); } catch { }
        await Shell.Current.GoToAsync("//home");
    }
}
