using TastyMealPlanner.Views;

namespace TastyMealPlanner;

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
