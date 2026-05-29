using TastyMealPlanner.Views;

namespace TastyMealPlanner;

/// <summary>Shell navigation configuration. Registers routes for detail pages (not in the tab bar).</summary>
public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Register routes for detail pages (not in tabs)
        Routing.RegisterRoute("recipedetail", typeof(RecipeDetailPage));
        Routing.RegisterRoute("camera", typeof(CameraPage));
        Routing.RegisterRoute("nearby", typeof(NearbyPage));
        Routing.RegisterRoute("quickadd", typeof(QuickAddPage));
    }
}
