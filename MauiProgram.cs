using Microsoft.Extensions.Logging;
using TastyMealPlanner.Services;
using TastyMealPlanner.ViewModels;
using TastyMealPlanner.Views;

namespace TastyMealPlanner;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Services
        builder.Services.AddSingleton<IDataService, DataService>();

        // ViewModels
        builder.Services.AddSingleton<HomeViewModel>();
        builder.Services.AddSingleton<RecipesViewModel>();
        builder.Services.AddTransient<RecipeDetailViewModel>();
        builder.Services.AddSingleton<MealPlanViewModel>();
        builder.Services.AddSingleton<ShoppingListViewModel>();
        builder.Services.AddTransient<CameraViewModel>();
        builder.Services.AddTransient<NearbyViewModel>();
        builder.Services.AddSingleton<SettingsViewModel>();

        // Pages
        builder.Services.AddSingleton<HomePage>();
        builder.Services.AddSingleton<RecipesPage>();
        builder.Services.AddTransient<RecipeDetailPage>();
        builder.Services.AddSingleton<MealPlanPage>();
        builder.Services.AddSingleton<ShoppingListPage>();
        builder.Services.AddTransient<CameraPage>();
        builder.Services.AddTransient<NearbyPage>();
        builder.Services.AddSingleton<SettingsPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
