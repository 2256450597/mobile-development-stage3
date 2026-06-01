namespace TastyMealPlanner.Models;

/// <summary>Defines food categories by cooking method.</summary>
public enum FoodCategory
{
    /// <summary>No heat required — assemble, blend, or mix fresh ingredients.</summary>
    Fresh,

    /// <summary>Cooked on the stovetop — pan, grill, wok, or pot.</summary>
    Stovetop,

    /// <summary>Cooked in the oven — baked, roasted, or set.</summary>
    Baked
}
