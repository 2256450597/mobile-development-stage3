using TastyMealPlanner.Views;

namespace TastyMealPlanner;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Register routes for detail pages (not in tabs)
        Routing.RegisterRoute("recipedetail", typeof(RecipeDetailPage));
        Routing.RegisterRoute("camera", typeof(CameraPage));
        Routing.RegisterRoute("nearby", typeof(NearbyPage));
    }
}
