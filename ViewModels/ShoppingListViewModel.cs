using System.Collections.ObjectModel;
using System.Windows.Input;
using TastyMealPlanner.Helpers;
using TastyMealPlanner.Models;
using TastyMealPlanner.Services;

namespace TastyMealPlanner.ViewModels;

public class ShoppingListViewModel : BaseViewModel
{
    private readonly IDataService _dataService;
    private readonly IHapticService _haptic;

    public ObservableCollection<ShoppingItem> Items { get; } = new();

    private string _newItemName = string.Empty;
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
    public string? ValidationError
    {
        get => _validationError;
        set => SetProperty(ref _validationError, value);
    }

    public ICommand AddItemCommand { get; }
    public ICommand ToggleItemCommand { get; }
    public ICommand ClearCheckedCommand { get; }

    public ShoppingListViewModel(IDataService dataService, IHapticService haptic)
    {
        _dataService = dataService;
        _haptic = haptic;
        Title = "Shopping List";

        AddItemCommand = new Command(OnAddItem);
        ToggleItemCommand = new Command<ShoppingItem>((item) =>
        {
            if (item != null)
            {
                _haptic.PerformClick();
                _dataService.ToggleShoppingItem(item.Id);
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
                _dataService.ClearCheckedShoppingItems();
                LoadItems();
                ValidationError = null;
            }
        });

        LoadItems();
    }

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
        _dataService.AddShoppingItem(new ShoppingItem
        {
            Name = NewItemName.Trim(),
            Quantity = NewItemQuantity.Trim()
        });

        NewItemName = string.Empty;
        NewItemQuantity = string.Empty;
        ValidationError = null;
        LoadItems();
    }

    public void LoadItems()
    {
        Items.Clear();
        foreach (var item in _dataService.GetShoppingList().OrderBy(i => i.IsChecked))
            Items.Add(item);
    }
}
