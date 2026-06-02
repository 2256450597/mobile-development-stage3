using System.Collections.ObjectModel;
using System.Windows.Input;
using TastyMealPlanner.Helpers;
using TastyMealPlanner.Models;
using TastyMealPlanner.Services;

namespace TastyMealPlanner.ViewModels;

/// <summary>Manages the shopping list with automatic ingredient generation from today's meal plan.</summary>
public class ShoppingListViewModel : BaseViewModel
{
    private readonly IShoppingListService _shoppingList;
    private readonly IMealPlanService _mealPlan;
    private readonly IHapticService _haptic;

    /// <summary>Gets the collection of shopping list items (auto-generated and manual).</summary>
    public ObservableCollection<ShoppingItem> Items { get; } = new();
    public ObservableCollection<ShoppingItem> AutoItems { get; } = new();
    public ObservableCollection<ShoppingItem> ManualItems { get; } = new();

    // === Progress ===
    private int _totalCount;
    public int TotalCount { get => _totalCount; set => SetProperty(ref _totalCount, value); }
    private int _checkedCount;
    public int CheckedCount { get => _checkedCount; set => SetProperty(ref _checkedCount, value); }
    private int _remainingCount;
    public int RemainingCount { get => _remainingCount; set => SetProperty(ref _remainingCount, value); }
    public string ProgressText => TotalCount == 0 ? "List is empty" : $"{RemainingCount} of {TotalCount} remaining";
    public double ProgressFraction => TotalCount == 0 ? 0 : (double)(TotalCount - RemainingCount) / TotalCount;
    public bool IsListEmpty => TotalCount == 0;
    public bool HasAutoItems => AutoItems.Count > 0;
    public bool ShowClearButton => CheckedCount > 0;
    public string ClearButtonText => CheckedCount > 1 ? $"Clear Purchased ({CheckedCount})" : "Clear Purchased";

    private string _newItemName = string.Empty;
    /// <summary>Gets or sets the name of a new item to add to the shopping list.</summary>
    public string NewItemName
    {
        get => _newItemName;
        set
        {
            if (SetProperty(ref _newItemName, value))
                ValidationError = null;
        }
    }

    private string _newItemQuantity = string.Empty;
    /// <summary>Gets or sets the quantity of a new item to add to the shopping list.</summary>
    public string NewItemQuantity
    {
        get => _newItemQuantity;
        set
        {
            if (SetProperty(ref _newItemQuantity, value))
                ValidationError = null;
        }
    }

    private string? _validationError;
    /// <summary>Gets or sets the current validation error message for the add-item form.</summary>
    public string? ValidationError
    {
        get => _validationError;
        set => SetProperty(ref _validationError, value);
    }

    /// <summary>Command to validate and add a new item to the shopping list.</summary>
    public ICommand AddItemCommand { get; }
    /// <summary>Command to toggle the checked (purchased) state of a shopping item.</summary>
    public ICommand ToggleItemCommand { get; }
    /// <summary>Command to remove all checked (purchased) items from the shopping list.</summary>
    public ICommand ClearCheckedCommand { get; }
    /// <summary>Command to refresh the shopping list with a smooth animated delay.</summary>
    public ICommand RefreshCommand { get; }

    /// <summary>Initialises a new instance of the <see cref="ShoppingListViewModel"/> class with data and haptic services, then loads the shopping list.</summary>
    public ShoppingListViewModel(IShoppingListService shoppingList, IMealPlanService mealPlan, IHapticService haptic)
    {
        _shoppingList = shoppingList;
        _mealPlan = mealPlan;
        _haptic = haptic;
        Title = "Shopping List";

        AddItemCommand = new Command(OnAddItem);
        ToggleItemCommand = new Command<ShoppingItem>((item) =>
        {
            if (item != null)
            {
                _haptic.PerformClick();
                _shoppingList.ToggleShoppingItem(item.Id);
                UpdateProgress();
            }
        });

        ClearCheckedCommand = new Command(async () =>
        {
            _haptic.PerformLongPress();
            var checkedItems = Items.Count(i => i.IsChecked);
            if (checkedItems == 0)
            {
                ValidationError = "No items are checked to clear.";
                return;
            }
            bool confirm = await Shell.Current.DisplayAlert(
                "Clear Items", $"Remove {checkedItems} purchased item(s)?", "Yes", "No");
            if (confirm)
            {
                _shoppingList.ClearCheckedShoppingItems();
                LoadItems();
                ValidationError = null;
            }
        });

        RefreshCommand = new Command(async () =>
        {
            _haptic.PerformClick();
            LoadItems();
            await Task.Delay(400);
            IsBusy = false;
        });

        LoadItems();
    }

    /// <summary>Validates and adds a new item to the shopping list.</summary>
    private void OnAddItem()
    {
        var (isValid, error) = ValidationHelper.ValidateShoppingItem(NewItemName, NewItemQuantity);
        if (!isValid)
        {
            _haptic.PerformLongPress();
            ValidationError = error;
            return;
        }

        _haptic.PerformClick();
        try
        {
            _shoppingList.AddShoppingItem(new ShoppingItem
            {
                Name = NewItemName.Trim(),
                Quantity = NewItemQuantity.Trim(),
                IsAutoGenerated = false
            });

            NewItemName = string.Empty;
            NewItemQuantity = string.Empty;
            ValidationError = null;
            LoadItems();
        }
        catch
        {
            ValidationError = "Unable to add this item. Please try again.";
        }
    }

    /// <summary>Reloads the shopping list, regenerating auto items from today's meal plan.</summary>
    public void LoadItems()
    {
        // Remove old auto-generated items, regenerate from today's meal plan
        _shoppingList.RemoveAutoGeneratedItems();

        var today = DateTime.Now.DayOfWeek;
        var monday = DateTime.Now.AddDays(-(int)today + 1);
        var weekEntries = _mealPlan.GetMealPlanForWeek(monday);
        var todayMeals = weekEntries
            .Where(e => e.Day == today && e.Recipe != null);

        foreach (var meal in todayMeals)
        {
            foreach (var ingredient in meal.Recipe!.Ingredients)
            {
                var (qty, name) = IngredientParser.Parse(ingredient);

                _shoppingList.AddShoppingItem(new ShoppingItem
                {
                    Name = string.IsNullOrEmpty(name) ? "" : char.ToUpper(name[0]) + name[1..],
                    Quantity = qty,
                    IsAutoGenerated = true
                });
            }
        }

        // Populate Items, AutoItems, ManualItems
        var all = _shoppingList.GetShoppingList()
            .OrderBy(i => i.IsAutoGenerated ? 0 : 1)
            .ThenBy(i => i.IsChecked ? 1 : 0)
            .ToList();

        Items.Clear();
        AutoItems.Clear();
        ManualItems.Clear();

        foreach (var item in all)
        {
            Items.Add(item);
            if (item.IsAutoGenerated) AutoItems.Add(item);
            else ManualItems.Add(item);
        }

        UpdateProgress();
    }

    private void UpdateProgress()
    {
        TotalCount = Items.Count;
        CheckedCount = Items.Count(i => i.IsChecked);
        RemainingCount = TotalCount - CheckedCount;
        OnPropertyChanged(nameof(ProgressText));
        OnPropertyChanged(nameof(ProgressFraction));
        OnPropertyChanged(nameof(ShowClearButton));
        OnPropertyChanged(nameof(ClearButtonText));
        OnPropertyChanged(nameof(IsListEmpty));
        OnPropertyChanged(nameof(HasAutoItems));
    }
}
