namespace TastyMealPlanner.Views;

public partial class HeaderLogo : ContentView
{
    public HeaderLogo()
    {
        InitializeComponent();
    }

    private async void OnLogoTapped(object? sender, EventArgs e)
    {
        try { HapticFeedback.Default.Perform(HapticFeedbackType.Click); } catch { }
        await Shell.Current.GoToAsync("//home");
    }
}
