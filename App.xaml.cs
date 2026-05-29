namespace TastyMealPlanner;

/// <summary>Application root. Initialises the main window with Shell-based navigation.</summary>
public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell());
    }
}
