using System.Collections.ObjectModel;
using System.Windows.Input;
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
        set => SetProperty(ref _newItemName, value);
    }

    private string _newItemQuantity = string.Empty;
    public string NewItemQuantity
    {
        get => _newItemQuantity;
        set => SetProperty(ref _newItemQuantity, value);
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

        ClearCheckedCommand = new Command(() =>
        {
            _haptic.PerformLongPress();
            _dataService.ClearCheckedShoppingItems();
            LoadItems();
        });

        LoadItems();
    }

    private void OnAddItem()
    {
        if (string.IsNullOrWhiteSpace(NewItemName)) return;

        _haptic.PerformClick();
        _dataService.AddShoppingItem(new ShoppingItem
        {
            Name = NewItemName.Trim(),
            Quantity = NewItemQuantity.Trim()
        });

        NewItemName = string.Empty;
        NewItemQuantity = string.Empty;
        LoadItems();
    }

    public void LoadItems()
    {
        Items.Clear();
        foreach (var item in _dataService.GetShoppingList().OrderBy(i => i.IsChecked))
            Items.Add(item);
    }
}
