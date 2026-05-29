using Microsoft.Extensions.Logging;
using TastyMealPlanner.Services;
using TastyMealPlanner.ViewModels;
using TastyMealPlanner.Views;

namespace TastyMealPlanner;

/// <summary>Application entry point. Registers all services, ViewModels, and pages in the DI container.</summary>
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
                fonts.AddFont("Pacifico-Regular.ttf", "Pacifico");
            });

        // Services
        builder.Services.AddSingleton<IDataService, DataService>();
        builder.Services.AddSingleton<ICameraService, CameraService>();
        builder.Services.AddSingleton<ILocationService, LocationService>();
        builder.Services.AddSingleton<ITextToSpeechService, TextToSpeechService>();
        builder.Services.AddSingleton<IAccelerometerService, AccelerometerService>();
        builder.Services.AddSingleton<IHapticService, HapticService>();
        builder.Services.AddSingleton<ThemeService>();

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

        var app = builder.Build();

        // Force ThemeService initialization on startup to apply saved theme
        app.Services.GetRequiredService<ThemeService>();

        return app;
    }
}
