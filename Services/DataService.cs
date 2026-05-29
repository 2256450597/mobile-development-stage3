using TastyMealPlanner.Models;

namespace TastyMealPlanner.Services;

/// <summary>In-memory data store providing recipes, meal plans, and shopping lists with search/filter capabilities.</summary>
public class DataService : IDataService
{
    private readonly List<Recipe> _recipes;
    private readonly List<MealPlanEntry> _mealPlan;
    private readonly List<ShoppingItem> _shoppingList;

    public DataService()
    {
        _recipes = InitializeRecipes();
        _mealPlan = InitializeMealPlan();
        _shoppingList = InitializeShoppingList();
    }

    public List<Recipe> GetAllRecipes() => _recipes;

    public List<Recipe> GetRecipesByCategory(FoodCategory category)
        => _recipes.Where(r => r.Category == category).ToList();

    public Recipe? GetRecipeById(string id)
        => _recipes.FirstOrDefault(r => r.Id == id);

    public List<MealPlanEntry> GetMealPlanForWeek(DateTime weekStart)
    {
        // Build set of 7 days starting from weekStart (assumed to be Monday)
        var weekDays = new HashSet<DayOfWeek>();
        for (int i = 0; i < 7; i++)
            weekDays.Add(weekStart.AddDays(i).DayOfWeek);

        return _mealPlan.Where(m => weekDays.Contains(m.Day)).ToList();
    }

    public List<ShoppingItem> GetShoppingList() => _shoppingList;

    public void AddToMealPlan(MealPlanEntry entry) => _mealPlan.Add(entry);

    public void RemoveFromMealPlan(string entryId)
    {
        var entry = _mealPlan.FirstOrDefault(e => e.Id == entryId);
        if (entry != null)
            _mealPlan.Remove(entry);
    }

    public void AddShoppingItem(ShoppingItem item) => _shoppingList.Add(item);

    public void ToggleShoppingItem(string itemId)
    {
        var item = _shoppingList.FirstOrDefault(i => i.Id == itemId);
        if (item != null)
            item.IsChecked = !item.IsChecked;
    }

    public void ClearCheckedShoppingItems()
        => _shoppingList.RemoveAll(i => i.IsChecked);

    public void AddRecipe(Recipe recipe) => _recipes.Add(recipe);

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

    /// <summary>Creates 15 sample recipes across 6 food categories with ingredients, instructions, and nutritional data.</summary>
    private static List<Recipe> InitializeRecipes() => new()
    {
        new Recipe
        {
            Id = "1",
            Name = "Classic Pancakes",
            Category = FoodCategory.Breakfast,
            Description = "Fluffy golden pancakes perfect for a weekend breakfast.",
            ImageUrl = "pancakes.jpg",
            Ingredients = new() { "1 cup flour", "2 tbsp sugar", "1 egg", "1 cup milk", "2 tbsp butter", "1 tsp baking powder" },
            Instructions = new()
            {
                "Mix flour, sugar, and baking powder in a bowl.",
                "Whisk egg and milk together, then combine with dry ingredients.",
                "Heat a pan and melt butter.",
                "Pour batter and cook until bubbles form, then flip.",
                "Serve with maple syrup and fresh berries."
            },
            PrepTimeMinutes = 10,
            CookTimeMinutes = 15,
            Calories = 350,
            Servings = 4,
            Difficulty = "Easy"
        },
        new Recipe
        {
            Id = "2",
            Name = "Avocado Toast",
            Category = FoodCategory.Breakfast,
            Description = "Simple and nutritious smashed avocado on sourdough toast.",
            ImageUrl = "avocado_toast.jpg",
            Ingredients = new() { "2 slices sourdough bread", "1 ripe avocado", "1 lime", "Salt", "Red pepper flakes", "Cherry tomatoes" },
            Instructions = new()
            {
                "Toast the sourdough bread until golden.",
                "Halve the avocado, remove pit, and scoop into a bowl.",
                "Mash avocado with lime juice and salt.",
                "Spread onto toast and top with sliced cherry tomatoes.",
                "Sprinkle with red pepper flakes."
            },
            PrepTimeMinutes = 5,
            CookTimeMinutes = 3,
            Calories = 280,
            Servings = 1,
            Difficulty = "Easy"
        },
        new Recipe
        {
            Id = "3",
            Name = "Caesar Salad",
            Category = FoodCategory.Lunch,
            Description = "Crisp romaine lettuce with classic Caesar dressing and croutons.",
            ImageUrl = "caesar_salad.jpg",
            Ingredients = new() { "1 head romaine lettuce", "Caesar dressing", "Croutons", "Parmesan cheese", "1 lemon", "Grilled chicken (optional)" },
            Instructions = new()
            {
                "Wash and chop romaine lettuce.",
                "Grill chicken breast if using, then slice.",
                "Toss lettuce with Caesar dressing.",
                "Top with croutons, shaved parmesan, and lemon juice.",
                "Add grilled chicken slices on top."
            },
            PrepTimeMinutes = 15,
            CookTimeMinutes = 10,
            Calories = 420,
            Servings = 2,
            Difficulty = "Easy"
        },
        new Recipe
        {
            Id = "4",
            Name = "Classic Margherita Pizza",
            Category = FoodCategory.Lunch,
            Description = "Authentic Italian pizza with fresh mozzarella and basil.",
            ImageUrl = "pizza.jpg",
            Ingredients = new() { "Pizza dough", "San Marzano tomatoes", "Fresh mozzarella", "Fresh basil", "Olive oil", "Salt" },
            Instructions = new()
            {
                "Preheat oven to 250 C / 480 F.",
                "Stretch pizza dough into a thin round.",
                "Spread crushed tomatoes over the base.",
                "Tear mozzarella and distribute evenly.",
                "Bake for 8-10 minutes until crust is golden.",
                "Top with fresh basil and a drizzle of olive oil."
            },
            PrepTimeMinutes = 20,
            CookTimeMinutes = 10,
            Calories = 680,
            Servings = 2,
            Difficulty = "Medium"
        },
        new Recipe
        {
            Id = "5",
            Name = "Grilled Salmon",
            Category = FoodCategory.Dinner,
            Description = "Perfectly grilled salmon fillet with lemon herb butter.",
            ImageUrl = "salmon.jpg",
            Ingredients = new() { "2 salmon fillets", "2 tbsp butter", "1 lemon", "Fresh dill", "Garlic cloves", "Asparagus", "Salt and pepper" },
            Instructions = new()
            {
                "Season salmon with salt, pepper, and lemon juice.",
                "Melt butter and mix with minced garlic and dill.",
                "Grill salmon skin-side down for 4-5 minutes.",
                "Flip and cook for another 3 minutes.",
                "Grill asparagus alongside with olive oil.",
                "Serve salmon topped with herb butter and asparagus."
            },
            PrepTimeMinutes = 10,
            CookTimeMinutes = 12,
            Calories = 450,
            Servings = 2,
            Difficulty = "Medium"
        },
        new Recipe
        {
            Id = "6",
            Name = "Beef Stir Fry",
            Category = FoodCategory.Dinner,
            Description = "Quick and flavourful beef stir fry with vegetables.",
            ImageUrl = "stirfry.jpg",
            Ingredients = new() { "Beef sirloin", "Broccoli", "Bell peppers", "Soy sauce", "Ginger", "Garlic", "Sesame oil", "Rice" },
            Instructions = new()
            {
                "Slice beef thinly against the grain.",
                "Chop broccoli and bell peppers.",
                "Heat wok with sesame oil until smoking.",
                "Stir fry beef for 2 minutes, then remove.",
                "Cook vegetables with ginger and garlic for 3 minutes.",
                "Return beef, add soy sauce, and serve over rice."
            },
            PrepTimeMinutes = 15,
            CookTimeMinutes = 10,
            Calories = 520,
            Servings = 2,
            Difficulty = "Medium"
        },
        new Recipe
        {
            Id = "7",
            Name = "Chocolate Lava Cake",
            Category = FoodCategory.Dessert,
            Description = "Rich molten chocolate cake with a gooey centre.",
            ImageUrl = "lava_cake.jpg",
            Ingredients = new() { "Dark chocolate 200g", "Butter 100g", "2 eggs", "2 egg yolks", "Sugar 50g", "Flour 30g" },
            Instructions = new()
            {
                "Melt chocolate and butter together in a double boiler.",
                "Whisk eggs, yolks, and sugar until thick.",
                "Fold chocolate mixture into egg mixture.",
                "Sift in flour and fold gently.",
                "Pour into greased ramekins.",
                "Bake at 200 C for exactly 12 minutes."
            },
            PrepTimeMinutes = 15,
            CookTimeMinutes = 12,
            Calories = 480,
            Servings = 4,
            Difficulty = "Hard"
        },
        new Recipe
        {
            Id = "8",
            Name = "Tiramisu",
            Category = FoodCategory.Dessert,
            Description = "Classic Italian layered coffee dessert with mascarpone cream.",
            ImageUrl = "tiramisu.jpg",
            Ingredients = new() { "Mascarpone 500g", "Espresso", "Ladyfinger biscuits", "Eggs 4", "Sugar 100g", "Cocoa powder" },
            Instructions = new()
            {
                "Separate eggs and beat yolks with sugar until creamy.",
                "Fold in mascarpone cheese.",
                "Whip egg whites to stiff peaks and fold in.",
                "Dip ladyfingers briefly in cooled espresso.",
                "Layer biscuits and cream in a dish.",
                "Refrigerate 4 hours, dust with cocoa before serving."
            },
            PrepTimeMinutes = 30,
            CookTimeMinutes = 0,
            Calories = 390,
            Servings = 6,
            Difficulty = "Medium"
        },
        new Recipe
        {
            Id = "9",
            Name = "Hummus with Pita",
            Category = FoodCategory.Snack,
            Description = "Creamy homemade hummus served with warm pita bread.",
            ImageUrl = "hummus.jpg",
            Ingredients = new() { "1 can chickpeas", "Tahini 3 tbsp", "Lemon juice", "Garlic 2 cloves", "Olive oil", "Paprika", "Pita bread" },
            Instructions = new()
            {
                "Drain and rinse chickpeas.",
                "Blend chickpeas with tahini, lemon juice, and garlic.",
                "Add ice water slowly until smooth and creamy.",
                "Season with salt and spread on a plate.",
                "Drizzle with olive oil and sprinkle paprika.",
                "Warm pita bread and cut into triangles."
            },
            PrepTimeMinutes = 10,
            CookTimeMinutes = 0,
            Calories = 250,
            Servings = 4,
            Difficulty = "Easy"
        },
        new Recipe
        {
            Id = "10",
            Name = "Fruit Smoothie Bowl",
            Category = FoodCategory.Snack,
            Description = "Thick and creamy smoothie bowl topped with fresh fruits and granola.",
            ImageUrl = "smoothie_bowl.jpg",
            Ingredients = new() { "Frozen banana", "Frozen berries", "Greek yogurt", "Honey", "Granola", "Chia seeds", "Fresh fruit" },
            Instructions = new()
            {
                "Blend frozen banana, berries, and yogurt until thick.",
                "Pour into a bowl.",
                "Top with sliced fresh fruit and granola.",
                "Drizzle with honey and sprinkle chia seeds.",
                "Serve immediately."
            },
            PrepTimeMinutes = 5,
            CookTimeMinutes = 0,
            Calories = 320,
            Servings = 1,
            Difficulty = "Easy"
        },
        new Recipe
        {
            Id = "11",
            Name = "Fresh Lemonade",
            Category = FoodCategory.Drink,
            Description = "Homemade zesty lemonade, perfectly refreshing.",
            ImageUrl = "lemonade.jpg",
            Ingredients = new() { "4 lemons", "Sugar 100g", "Water 1L", "Ice cubes", "Mint leaves" },
            Instructions = new()
            {
                "Juice the lemons.",
                "Dissolve sugar in a little warm water.",
                "Combine lemon juice, sugar syrup, and cold water.",
                "Stir well and add ice cubes.",
                "Garnish with mint leaves and lemon slices."
            },
            PrepTimeMinutes = 10,
            CookTimeMinutes = 0,
            Calories = 120,
            Servings = 4,
            Difficulty = "Easy"
        },
        new Recipe
        {
            Id = "12",
            Name = "Banana Oatmeal",
            Category = FoodCategory.Breakfast,
            Description = "Warm and comforting oatmeal with caramelised bananas.",
            ImageUrl = "oatmeal.png",
            Ingredients = new() { "Rolled oats 1 cup", "Milk 2 cups", "Banana 1", "Honey", "Cinnamon", "Walnuts" },
            Instructions = new()
            {
                "Bring milk to a gentle simmer.",
                "Add oats and cook for 5 minutes, stirring.",
                "Slice banana and lightly caramelise in a pan.",
                "Pour oatmeal into bowls.",
                "Top with banana, walnuts, honey, and cinnamon."
            },
            PrepTimeMinutes = 5,
            CookTimeMinutes = 10,
            Calories = 340,
            Servings = 2,
            Difficulty = "Easy"
        },
        new Recipe
        {
            Id = "13",
            Name = "Chicken Fajitas",
            Category = FoodCategory.Dinner,
            Description = "Sizzling chicken fajitas with peppers and onions.",
            ImageUrl = "fajitas.jpg",
            Ingredients = new() { "Chicken breast", "Bell peppers", "Onion", "Fajita seasoning", "Tortillas", "Sour cream", "Salsa" },
            Instructions = new()
            {
                "Slice chicken into strips and coat with seasoning.",
                "Slice peppers and onion.",
                "Sear chicken in a hot skillet for 5 minutes.",
                "Add vegetables and cook until charred.",
                "Warm tortillas.",
                "Serve with sour cream and salsa."
            },
            PrepTimeMinutes = 15,
            CookTimeMinutes = 12,
            Calories = 480,
            Servings = 4,
            Difficulty = "Easy"
        },
        new Recipe
        {
            Id = "14",
            Name = "Mango Lassi",
            Category = FoodCategory.Drink,
            Description = "Creamy Indian yogurt drink blended with sweet mango.",
            ImageUrl = "mango_lassi.jpg",
            Ingredients = new() { "Ripe mango 1", "Plain yogurt 1 cup", "Milk 1/2 cup", "Sugar 2 tbsp", "Cardamom powder", "Ice cubes" },
            Instructions = new()
            {
                "Peel and chop the mango.",
                "Add mango, yogurt, milk, and sugar to blender.",
                "Add a pinch of cardamom powder.",
                "Blend until smooth.",
                "Pour over ice and serve."
            },
            PrepTimeMinutes = 5,
            CookTimeMinutes = 0,
            Calories = 200,
            Servings = 2,
            Difficulty = "Easy"
        },
        new Recipe
        {
            Id = "15",
            Name = "Vegetable Curry",
            Category = FoodCategory.Dinner,
            Description = "Aromatic and hearty vegetable curry with coconut milk.",
            ImageUrl = "curry.jpg",
            Ingredients = new() { "Coconut milk", "Sweet potato", "Chickpeas", "Spinach", "Curry paste", "Onion", "Garlic", "Basmati rice" },
            Instructions = new()
            {
                "Dice sweet potato and onion, mince garlic.",
                "Saut  onion and garlic until soft.",
                "Add curry paste and cook for 1 minute.",
                "Add sweet potato, chickpeas, and coconut milk.",
                "Simmer for 20 minutes until potatoes are tender.",
                "Stir in spinach until wilted, serve over rice."
            },
            PrepTimeMinutes = 15,
            CookTimeMinutes = 25,
            Calories = 460,
            Servings = 4,
            Difficulty = "Medium"
        }
    };

    private List<MealPlanEntry> InitializeMealPlan()
    {
        var today = DateTime.Now.DayOfWeek;
        var monday = today - ((int)today - 1);
        if (monday < 0) monday += 7;

        return new()
        {
            new() { Id = "mp1", Day = DayOfWeek.Monday, MealType = MealType.Breakfast, Recipe = _recipes[0] },
            new() { Id = "mp2", Day = DayOfWeek.Monday, MealType = MealType.Lunch, Recipe = _recipes[2] },
            new() { Id = "mp3", Day = DayOfWeek.Monday, MealType = MealType.Dinner, Recipe = _recipes[4] },
            new() { Id = "mp4", Day = DayOfWeek.Tuesday, MealType = MealType.Breakfast, Recipe = _recipes[11] },
            new() { Id = "mp5", Day = DayOfWeek.Tuesday, MealType = MealType.Dinner, Recipe = _recipes[12] },
            new() { Id = "mp6", Day = DayOfWeek.Wednesday, MealType = MealType.Breakfast, Recipe = _recipes[1] },
            new() { Id = "mp7", Day = DayOfWeek.Wednesday, MealType = MealType.Lunch, Recipe = _recipes[3] },
            new() { Id = "mp8", Day = DayOfWeek.Thursday, MealType = MealType.Dinner, Recipe = _recipes[14] },
            new() { Id = "mp9", Day = DayOfWeek.Friday, MealType = MealType.Lunch, Recipe = _recipes[5] },
            new() { Id = "mp10", Day = DayOfWeek.Saturday, MealType = MealType.Breakfast, Recipe = _recipes[0] },
            new() { Id = "mp11", Day = DayOfWeek.Saturday, MealType = MealType.Dinner, Recipe = _recipes[4] },
            new() { Id = "mp12", Day = DayOfWeek.Sunday, MealType = MealType.Breakfast, Recipe = _recipes[9] },
        };
    }

    private static List<ShoppingItem> InitializeShoppingList() => new()
    {
        new() { Id = "s1", Name = "Chicken Breast", Quantity = "500g" },
        new() { Id = "s2", Name = "Avocado", Quantity = "2 pcs" },
        new() { Id = "s3", Name = "Milk", Quantity = "2L" },
        new() { Id = "s4", Name = "Eggs", Quantity = "12 pcs" },
        new() { Id = "s5", Name = "Bread", Quantity = "1 loaf" },
        new() { Id = "s6", Name = "Tomatoes", Quantity = "4 pcs" },
        new() { Id = "s7", Name = "Olive Oil", Quantity = "1 bottle" },
    };
}
