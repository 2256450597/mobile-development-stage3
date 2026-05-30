using TastyMealPlanner.Views;

namespace TastyMealPlanner;

/// <summary>Shell navigation configuration. Registers routes for detail pages (not in the tab bar).</summary>
public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute("recipedetail", typeof(RecipeDetailPage));
        Routing.RegisterRoute("camera", typeof(CameraPage));
        Routing.RegisterRoute("nearby", typeof(NearbyPage));
        Routing.RegisterRoute("quickadd", typeof(QuickAddPage));
    }

    /// <summary>Whenever the user switches to a different tab, pop all pushed pages so they see the root.</summary>
    protected override void OnNavigated(ShellNavigatedEventArgs args)
    {
        base.OnNavigated(args);

        // If we just navigated to a tab root URL (e.g. //recipes, //home), clear any pushed pages
        if (args.Source == ShellNavigationSource.ShellSectionChanged
            || args.Source == ShellNavigationSource.ShellItemChanged)
        {
            var currentTab = CurrentItem?.CurrentItem;
            if (currentTab != null)
            {
                var stack = currentTab.Navigation.NavigationStack;
                while (stack.Count > 1)
                {
                    currentTab.Navigation.RemovePage(stack[stack.Count - 1]);
                }
            }
        }
    }
}
