namespace TastyMealPlanner.Models;

/// <summary>Defines the available food categories used to classify recipes throughout the app.</summary>
public enum FoodCategory
{
    /// <summary>Morning meal recipes such as pancakes, oatmeal, and avocado toast.</summary>
    Breakfast,

    /// <summary>Midday meal recipes such as salads, sandwiches, and pizza.</summary>
    Lunch,

    /// <summary>Evening meal recipes such as grilled salmon, stir fry, and curry.</summary>
    Dinner,

    /// <summary>Sweet treats and desserts such as lava cake and tiramisu.</summary>
    Dessert,

    /// <summary>Light bites and snacks such as hummus and smoothie bowls.</summary>
    Snack,

    /// <summary>Beverages such as lemonade and mango lassi.</summary>
    Drink
}
