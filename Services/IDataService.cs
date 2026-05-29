using TastyMealPlanner.Models;

namespace TastyMealPlanner.Services;

/// <summary>
/// Provides access to recipe data, meal plans, and shopping lists.
/// Serves as the primary data layer for the application.
/// </summary>
public interface IDataService
{
    /// <summary>Returns all available recipes.</summary>
    List<Recipe> GetAllRecipes();

    /// <summary>Returns recipes filtered by a specific category.</summary>
    List<Recipe> GetRecipesByCategory(FoodCategory category);

    /// <summary>Retrieves a single recipe by its unique ID, or null if not found.</summary>
    Recipe? GetRecipeById(string id);

    /// <summary>Gets the meal plan entries for a given week (starting Monday).</summary>
    List<MealPlanEntry> GetMealPlanForWeek(DateTime weekStart);

    /// <summary>Returns the current shopping list.</summary>
    List<ShoppingItem> GetShoppingList();

    /// <summary>Adds a recipe to the meal plan for a specific day and meal type.</summary>
    void AddToMealPlan(MealPlanEntry entry);

    /// <summary>Removes a meal plan entry by its ID.</summary>
    void RemoveFromMealPlan(string entryId);

    /// <summary>Adds a new item to the shopping list.</summary>
    void AddShoppingItem(ShoppingItem item);

    /// <summary>Toggles the checked state of a shopping item.</summary>
    void ToggleShoppingItem(string itemId);

    /// <summary>Removes all checked/purchased items from the shopping list.</summary>
    void ClearCheckedShoppingItems();

    /// <summary>Searches recipes by name, category, or ingredients.</summary>
    List<Recipe> SearchRecipes(string query);

    /// <summary>Adds a new recipe to the catalog.</summary>
    void AddRecipe(Recipe recipe);
}
