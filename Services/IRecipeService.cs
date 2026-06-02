using TastyMealPlanner.Models;

namespace TastyMealPlanner.Services;

/// <summary>Recipe data access — search, filter, and catalogue management.</summary>
public interface IRecipeService
{
    List<Recipe> GetAllRecipes();
    List<Recipe> GetRecipesByCategory(FoodCategory category);
    Recipe? GetRecipeById(string id);
    List<Recipe> SearchRecipes(string query);
    void AddRecipe(Recipe recipe);
    void DeleteRecipe(string id);
    void UpdateRecipe(Recipe recipe);
}
