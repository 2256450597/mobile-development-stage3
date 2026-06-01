using System.Text.Json;
using System.Text.Json.Serialization;
using TastyMealPlanner.Models;

namespace TastyMealPlanner.Services;

/// <summary>In-memory recipe store loaded from embedded JSON, with search and filter support.</summary>
public class RecipeService : IRecipeService
{
    private readonly List<Recipe> _recipes;

    public RecipeService()
    {
        _recipes = LoadFromJson();
    }

    public List<Recipe> GetAllRecipes() => _recipes;

    public List<Recipe> GetRecipesByCategory(FoodCategory category)
        => _recipes.Where(r => r.Category == category).ToList();

    public Recipe? GetRecipeById(string id)
        => _recipes.FirstOrDefault(r => r.Id == id);

    public List<Recipe> SearchRecipes(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return _recipes;

        var lower = query.ToLowerInvariant();
        return _recipes
            .Where(r => r.Name.ToLowerInvariant().Contains(lower)
                     || r.Category.ToString().ToLowerInvariant().Contains(lower)
                     || r.Ingredients.Any(i => i.ToLowerInvariant().Contains(lower)))
            .ToList();
    }

    public void AddRecipe(Recipe recipe) => _recipes.Add(recipe);

    private static List<Recipe> LoadFromJson()
    {
        try
        {
            using var stream = FileSystem.OpenAppPackageFileAsync("recipes.json").Result;
            using var reader = new StreamReader(stream);
            var json = reader.ReadToEnd();
            return JsonSerializer.Deserialize<List<Recipe>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter() }
            }) ?? new();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load recipes.json: {ex.Message}");
            return new();
        }
    }
}
