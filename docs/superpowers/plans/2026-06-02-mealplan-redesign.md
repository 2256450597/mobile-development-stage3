# Meal Plan Page Redesign Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace the vertical zebra-striped card list with a horizontal week-strip + detail panel layout, with week navigation arrows and swipeable day selection.

**Architecture:** Rewrite `MealPlanViewModel` to manage a `WeekStart` date and a `SelectedDay`, expose a `DayStrip` collection of 7 day cells, and a `SelectedDayMeals` collection for the detail panel. Rewrite `MealPlanPage.xaml` with three sections: week navigation bar, horizontal day strip, and selected-day detail panel.

**Tech Stack:** .NET MAUI XAML, CollectionView (horizontal LinearItemsLayout), BindableLayout, existing theme colors

---

### Task 1: Add DayStripItem model and extend MealPlanViewModel

**Files:**
- Modify: `ViewModels/MealPlanViewModel.cs` (complete rewrite)

- [ ] **Step 1: Rewrite MealPlanViewModel with week navigation, day strip, and selected day support**

Replace the entire file with:

```csharp
using System.Collections.ObjectModel;
using System.Windows.Input;
using TastyMealPlanner.Models;
using TastyMealPlanner.Services;

namespace TastyMealPlanner.ViewModels;

public class MealPlanViewModel : BaseViewModel
{
    private readonly IMealPlanService _mealPlan;
    private readonly IHapticService _haptic;

    // === Week navigation ===
    private DateTime _weekStart;
    public DateTime WeekStart
    {
        get => _weekStart;
        set
        {
            if (SetProperty(ref _weekStart, value))
            {
                OnPropertyChanged(nameof(WeekLabel));
                OnPropertyChanged(nameof(WeekNumber));
                OnPropertyChanged(nameof(IsCurrentWeek));
                OnPropertyChanged(nameof(ShowBackToCurrent));
                LoadDayStrip();
            }
        }
    }

    public string WeekLabel
    {
        get
        {
            var end = _weekStart.AddDays(6);
            return $"{_weekStart:MMM d} – {end:MMM d, yyyy}";
        }
    }

    public int WeekNumber => System.Globalization.CultureInfo
        .CurrentCulture.Calendar.GetWeekOfYear(_weekStart,
            System.Globalization.CalendarWeekRule.FirstDay,
            DayOfWeek.Monday);

    public bool IsCurrentWeek
    {
        get
        {
            var today = DateTime.Now.Date;
            var currentMonday = today.AddDays(-(int)today.DayOfWeek + 1);
            return _weekStart.Date == currentMonday.Date;
        }
    }

    public bool ShowBackToCurrent => !IsCurrentWeek;

    // === Day strip ===
    public ObservableCollection<DayStripItem> DayStrip { get; } = new();

    private DayOfWeek _selectedDay;
    public DayOfWeek SelectedDay
    {
        get => _selectedDay;
        set
        {
            if (SetProperty(ref _selectedDay, value))
            {
                LoadSelectedDayMeals();
                RefreshDayStripSelection();
            }
        }
    }

    public ObservableCollection<MealPlanEntry> SelectedDayMeals { get; } = new();

    // === Header for detail panel ===
    public string SelectedDayHeader
    {
        get
        {
            var date = _weekStart.AddDays((int)_selectedDay);
            var today = DateTime.Now.DayOfWeek == _selectedDay && IsCurrentWeek;
            var suffix = today ? " · Today" : "";
            return $"{date:dddd, MMMM d}{suffix}";
        }
    }

    public string MealCountLabel => SelectedDayMeals.Count == 1
        ? "1 meal" : $"{SelectedDayMeals.Count} meals";

    // === Commands ===
    public ICommand GoToPreviousWeekCommand { get; }
    public ICommand GoToNextWeekCommand { get; }
    public ICommand BackToCurrentWeekCommand { get; }
    public ICommand SelectDayCommand { get; }
    public ICommand RemoveMealCommand { get; }
    public ICommand NavigateToDetailCommand { get; }
    public ICommand AddMealCommand { get; }
    public ICommand RefreshCommand { get; }

    public MealPlanViewModel(IMealPlanService mealPlan, IHapticService haptic)
    {
        _mealPlan = mealPlan;
        _haptic = haptic;
        Title = "Meal Plan";

        var today = DateTime.Now.Date;
        _weekStart = today.AddDays(-(int)today.DayOfWeek + 1);
        _selectedDay = today.DayOfWeek;

        GoToPreviousWeekCommand = new Command(() =>
        {
            _haptic.PerformClick();
            WeekStart = _weekStart.AddDays(-7);
        });

        GoToNextWeekCommand = new Command(() =>
        {
            _haptic.PerformClick();
            WeekStart = _weekStart.AddDays(7);
        });

        BackToCurrentWeekCommand = new Command(() =>
        {
            _haptic.PerformClick();
            var today2 = DateTime.Now.Date;
            WeekStart = today2.AddDays(-(int)today2.DayOfWeek + 1);
            SelectedDay = today2.DayOfWeek;
        });

        SelectDayCommand = new Command<DayStripItem>(item =>
        {
            if (item == null) return;
            _haptic.PerformClick();
            SelectedDay = item.Day;
        });

        NavigateToDetailCommand = new Command<MealPlanEntry>(async (entry) =>
        {
            if (entry?.Recipe == null) return;
            _haptic.PerformClick();
            await Shell.Current.GoToAsync($"recipedetail?id={entry.Recipe.Id}");
        });

        RemoveMealCommand = new Command<MealPlanEntry>(async (entry) =>
        {
            if (entry == null) return;
            _haptic.PerformLongPress();
            bool confirm = await Shell.Current.DisplayAlert(
                "Remove Meal", $"Remove {entry.Recipe?.Name} from {entry.MealType}?", "Yes", "No");
            if (confirm)
            {
                _mealPlan.RemoveFromMealPlan(entry.Id);
                LoadSelectedDayMeals();
                LoadDayStrip();
            }
        });

        AddMealCommand = new Command(async () =>
        {
            _haptic.PerformClick();
            RecipesViewModel.PendingSelectionDay = SelectedDay;
            await Shell.Current.GoToAsync("//recipes");
        });

        RefreshCommand = new Command(async () =>
        {
            _haptic.PerformClick();
            Reload();
            await Task.Delay(400);
            IsBusy = false;
        });

        LoadDayStrip();
        LoadSelectedDayMeals();
    }

    public void Reload()
    {
        LoadDayStrip();
        LoadSelectedDayMeals();
    }

    private void LoadDayStrip()
    {
        DayStrip.Clear();
        var today = DateTime.Now.Date;
        for (int i = 0; i < 7; i++)
        {
            var date = _weekStart.AddDays(i);
            var day = date.DayOfWeek;
            var meals = _mealPlan.GetMealPlanForWeek(_weekStart)
                .Count(e => e.Day == day);

            DayStrip.Add(new DayStripItem
            {
                Day = day,
                DayAbbr = day.ToString().Substring(0, 3),
                DateNumber = date.Day,
                IsToday = date.Date == today,
                IsSelected = day == _selectedDay,
                MealCount = meals
            });
        }
    }

    private void RefreshDayStripSelection()
    {
        foreach (var item in DayStrip)
            item.IsSelected = item.Day == _selectedDay;

        OnPropertyChanged(nameof(SelectedDayHeader));
        OnPropertyChanged(nameof(MealCountLabel));
    }

    private void LoadSelectedDayMeals()
    {
        SelectedDayMeals.Clear();
        var meals = _mealPlan.GetMealPlanForWeek(_weekStart)
            .Where(e => e.Day == _selectedDay)
            .OrderBy(e => e.MealType)
            .ToList();

        foreach (var m in meals)
            SelectedDayMeals.Add(m);

        OnPropertyChanged(nameof(MealCountLabel));
        OnPropertyChanged(nameof(SelectedDayHeader));
    }
}

public class DayStripItem
{
    public DayOfWeek Day { get; set; }
    public string DayAbbr { get; set; } = string.Empty;
    public int DateNumber { get; set; }
    public bool IsToday { get; set; }
    public bool IsSelected { get; set; }
    public int MealCount { get; set; }
    public bool HasMeals => MealCount > 0;
    public string MealCountText => MealCount == 0 ? "" : MealCount.ToString();
}
```

**Step 2: Commit**

```bash
git add ViewModels/MealPlanViewModel.cs
git commit -m "feat: rewrite MealPlanViewModel with week navigation and day strip"
```

---

### Task 2: Rewrite MealPlanPage.xaml with new layout

**Files:**
- Modify: `Views/MealPlanPage.xaml` (complete rewrite)

- [ ] **Step 1: Write the new XAML**

Replace the entire file content with:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:vm="clr-namespace:TastyMealPlanner.ViewModels"
             xmlns:models="clr-namespace:TastyMealPlanner.Models"
             xmlns:views="clr-namespace:TastyMealPlanner.Views"
             x:Class="TastyMealPlanner.Views.MealPlanPage"
             x:DataType="vm:MealPlanViewModel"
             Shell.NavBarIsVisible="False" Title="Meal Plan"
             BackgroundColor="{AppThemeBinding Light={StaticResource Light_Background}, Dark={StaticResource Dark_Background}}">

    <Grid RowDefinitions="Auto,Auto,Auto,Auto,*,Auto" Padding="0">

        <!-- Row 0: Header logo -->
        <views:HeaderLogo Grid.Row="0" />

        <!-- Row 1: Week Navigation Bar -->
        <Grid Grid.Row="1" ColumnDefinitions="Auto,*,Auto" Padding="16,0,16,8">
            <!-- Previous week -->
            <Border Grid.Column="0" StrokeShape="RoundRectangle 20"
                    Stroke="{StaticResource BorderLight}" StrokeThickness="1"
                    WidthRequest="40" HeightRequest="40" Padding="0"
                    BackgroundColor="{AppThemeBinding Light={StaticResource Light_Card}, Dark={StaticResource Dark_Card}}">
                <Border.Shadow>
                    <Shadow Brush="#10000000" Offset="0,2" Radius="6" Opacity="0.3" />
                </Border.Shadow>
                <Label Text="&#xe5cb;" FontSize="20" FontFamily="MaterialIcons"
                       TextColor="{StaticResource Primary}"
                       HorizontalTextAlignment="Center" VerticalTextAlignment="Center" />
                <Border.GestureRecognizers>
                    <TapGestureRecognizer Command="{Binding GoToPreviousWeekCommand}" />
                </Border.GestureRecognizers>
            </Border>
            <!-- Week label -->
            <VerticalStackLayout Grid.Column="1" HorizontalOptions="Center" VerticalOptions="Center" Spacing="2">
                <Label Text="{Binding WeekLabel}" FontSize="16" FontAttributes="Bold"
                       TextColor="{AppThemeBinding Light={StaticResource Light_TextPrimary}, Dark={StaticResource Dark_TextPrimary_Alt}}"
                       HorizontalTextAlignment="Center" />
                <Label Text="{Binding WeekNumber, StringFormat='Week {0}'}" FontSize="12"
                       TextColor="{StaticResource TextTertiary}"
                       HorizontalTextAlignment="Center" />
            </VerticalStackLayout>
            <!-- Next week -->
            <Border Grid.Column="2" StrokeShape="RoundRectangle 20"
                    Stroke="{StaticResource BorderLight}" StrokeThickness="1"
                    WidthRequest="40" HeightRequest="40" Padding="0"
                    BackgroundColor="{AppThemeBinding Light={StaticResource Light_Card}, Dark={StaticResource Dark_Card}}">
                <Border.Shadow>
                    <Shadow Brush="#10000000" Offset="0,2" Radius="6" Opacity="0.3" />
                </Border.Shadow>
                <Label Text="&#xe5cc;" FontSize="20" FontFamily="MaterialIcons"
                       TextColor="{StaticResource Primary}"
                       HorizontalTextAlignment="Center" VerticalTextAlignment="Center" />
                <Border.GestureRecognizers>
                    <TapGestureRecognizer Command="{Binding GoToNextWeekCommand}" />
                </Border.GestureRecognizers>
            </Border>
        </Grid>

        <!-- Row 2: Swipe hint -->
        <Label Grid.Row="2" Text="Select a day to view or add meals"
               FontSize="12" HorizontalTextAlignment="Center"
               TextColor="{StaticResource TextTertiary}" Margin="0,0,0,8" />

        <!-- Row 3: Horizontal Day Strip -->
        <CollectionView Grid.Row="3" ItemsSource="{Binding DayStrip}" HeightRequest="100"
                        Margin="0,0,0,8">
            <CollectionView.ItemsLayout>
                <LinearItemsLayout Orientation="Horizontal" ItemSpacing="8" />
            </CollectionView.ItemsLayout>
            <CollectionView.Header>
                <BoxView WidthRequest="12" Color="Transparent" />
            </CollectionView.Header>
            <CollectionView.Footer>
                <BoxView WidthRequest="12" Color="Transparent" />
            </CollectionView.Footer>
            <CollectionView.ItemTemplate>
                <DataTemplate x:DataType="vm:DayStripItem">
                    <Border StrokeShape="RoundRectangle 14"
                            StrokeThickness="0" Padding="0"
                            WidthRequest="68" HeightRequest="90"
                            BackgroundColor="{AppThemeBinding Light=#FFFFFF, Dark=#1E1C18}">
                        <Border.Triggers>
                            <!-- Today: terracotta fill -->
                            <DataTrigger TargetType="Border" Binding="{Binding IsToday}" Value="True">
                                <Setter Property="BackgroundColor" Value="{StaticResource Primary}" />
                            </DataTrigger>
                            <!-- Selected (not today): subtle outline -->
                            <MultiTrigger TargetType="Border">
                                <MultiTrigger.Conditions>
                                    <BindingCondition Binding="{Binding IsSelected}" Value="True" />
                                    <BindingCondition Binding="{Binding IsToday}" Value="False" />
                                </MultiTrigger.Conditions>
                                <Setter Property="Stroke" Value="{StaticResource Primary}" />
                                <Setter Property="StrokeThickness" Value="2" />
                            </MultiTrigger>
                        </Border.Triggers>
                        <Border.Shadow>
                            <Shadow Brush="#12000000" Offset="0,2" Radius="8" Opacity="0.3" />
                        </Border.Shadow>
                        <VerticalStackLayout VerticalOptions="Center" HorizontalOptions="Center"
                                             Spacing="4" Padding="4">
                            <Label Text="{Binding DayAbbr}" FontSize="12"
                                   HorizontalTextAlignment="Center">
                                <Label.Triggers>
                                    <DataTrigger TargetType="Label" Binding="{Binding IsToday}" Value="True">
                                        <Setter Property="TextColor" Value="White" />
                                    </DataTrigger>
                                    <DataTrigger TargetType="Label" Binding="{Binding IsToday}" Value="False">
                                        <Setter Property="TextColor" Value="{StaticResource TextTertiary}" />
                                    </DataTrigger>
                                </Label.Triggers>
                            </Label>
                            <Label Text="{Binding DateNumber}" FontSize="22" FontAttributes="Bold"
                                   HorizontalTextAlignment="Center">
                                <Label.Triggers>
                                    <DataTrigger TargetType="Label" Binding="{Binding IsToday}" Value="True">
                                        <Setter Property="TextColor" Value="White" />
                                    </DataTrigger>
                                    <DataTrigger TargetType="Label" Binding="{Binding IsToday}" Value="False">
                                        <Setter Property="TextColor" Value="{AppThemeBinding Light={StaticResource Light_TextPrimary}, Dark={StaticResource Dark_TextPrimary_Alt}}" />
                                    </DataTrigger>
                                </Label.Triggers>
                            </Label>
                            <!-- Meal indicator -->
                            <Border StrokeShape="RoundRectangle 8" StrokeThickness="0"
                                    Padding="6,2" HorizontalOptions="Center">
                                <Border.Triggers>
                                    <DataTrigger TargetType="Border" Binding="{Binding HasMeals}" Value="True">
                                        <Setter Property="BackgroundColor" Value="{StaticResource PrimaryVeryLight}" />
                                    </DataTrigger>
                                    <DataTrigger TargetType="Border" Binding="{Binding HasMeals}" Value="False">
                                        <Setter Property="BackgroundColor" Value="Transparent" />
                                    </DataTrigger>
                                </Border.Triggers>
                                <Label Text="{Binding MealCountText}" FontSize="11" FontAttributes="Bold"
                                       HorizontalTextAlignment="Center">
                                    <Label.Triggers>
                                        <DataTrigger TargetType="Label" Binding="{Binding HasMeals}" Value="True">
                                            <Setter Property="TextColor" Value="{StaticResource Primary}" />
                                        </DataTrigger>
                                        <DataTrigger TargetType="Label" Binding="{Binding HasMeals}" Value="False">
                                            <Setter Property="TextColor" Value="Transparent" />
                                        </DataTrigger>
                                    </Label.Triggers>
                                </Label>
                            </Border>
                        </VerticalStackLayout>
                        <Border.GestureRecognizers>
                            <TapGestureRecognizer
                                Command="{Binding Source={RelativeSource AncestorType={x:Type vm:MealPlanViewModel}}, Path=SelectDayCommand}"
                                CommandParameter="{Binding .}" />
                        </Border.GestureRecognizers>
                    </Border>
                </DataTemplate>
            </CollectionView.ItemTemplate>
        </CollectionView>

        <!-- Row 4: Selected Day Detail Panel -->
        <RefreshView Grid.Row="4" Command="{Binding RefreshCommand}" IsRefreshing="{Binding IsBusy}">
            <ScrollView>
                <VerticalStackLayout Padding="20,16,20,20" Spacing="12"
                    BackgroundColor="{AppThemeBinding Light=#FFFFFF, Dark=#1E1C18}">
                    <!-- Rounded top corners on the white panel -->
                    <Border.StrokeShape>
                        <RoundRectangle CornerRadius="20,20,0,0" />
                    </Border.StrokeShape>

                    <!-- Detail header -->
                    <Grid ColumnDefinitions="Auto,*,Auto" ColumnSpacing="10">
                        <BoxView Grid.Column="0" WidthRequest="4" CornerRadius="2"
                                 Color="{StaticResource Primary}" VerticalOptions="Fill"
                                 HeightRequest="24" />
                        <VerticalStackLayout Grid.Column="1" Spacing="2">
                            <Label Text="{Binding SelectedDayHeader}" FontSize="17" FontAttributes="Bold"
                                   TextColor="{AppThemeBinding Light={StaticResource Light_TextPrimary}, Dark={StaticResource Dark_TextPrimary_Alt}}" />
                            <Label Text="{Binding MealCountLabel}" FontSize="13"
                                   TextColor="{StaticResource TextSecondary}" />
                        </VerticalStackLayout>
                        <Border Grid.Column="2" StrokeShape="RoundRectangle 12" StrokeThickness="0"
                                BackgroundColor="{StaticResource PrimaryVeryLight}"
                                Padding="10,6" VerticalOptions="Center">
                            <Label Text="{Binding MealCountLabel}" FontSize="12" FontAttributes="Bold"
                                   TextColor="{StaticResource Primary}" />
                        </Border>
                    </Grid>

                    <BoxView HeightRequest="1" Color="{StaticResource BorderLight}" />

                    <!-- Meal list via BindableLayout -->
                    <VerticalStackLayout Spacing="10"
                        BindableLayout.ItemsSource="{Binding SelectedDayMeals}">
                        <BindableLayout.EmptyView>
                            <VerticalStackLayout HorizontalOptions="Center" VerticalOptions="Center"
                                                 Spacing="8" Padding="0,20">
                                <Label Text="&#xe561;" FontSize="36" FontFamily="MaterialIcons"
                                       TextColor="{StaticResource TextTertiary}"
                                       HorizontalTextAlignment="Center" />
                                <Label Text="No meals planned for this day" FontSize="14"
                                       TextColor="{StaticResource TextSecondary}"
                                       HorizontalTextAlignment="Center" />
                                <Label Text="Tap + Add Meal below to get started" FontSize="12"
                                       TextColor="{StaticResource TextTertiary}"
                                       HorizontalTextAlignment="Center" />
                            </VerticalStackLayout>
                        </BindableLayout.EmptyView>
                        <BindableLayout.ItemTemplate>
                            <DataTemplate x:DataType="models:MealPlanEntry">
                                <Border StrokeShape="RoundRectangle 12"
                                        Stroke="{AppThemeBinding Light=#F0EBE4, Dark=#2A2A25}"
                                        StrokeThickness="1" Padding="14,12"
                                        BackgroundColor="{AppThemeBinding Light=#FFFBF7, Dark=#1C1B18}">
                                    <Grid ColumnDefinitions="Auto,*,Auto" ColumnSpacing="12">
                                        <!-- Meal type icon -->
                                        <Border Grid.Column="0" StrokeThickness="0"
                                                StrokeShape="RoundRectangle 10"
                                                WidthRequest="40" HeightRequest="40" Padding="0"
                                                BackgroundColor="{StaticResource PrimaryVeryLight}">
                                            <Label Text="&#xe8fe;" FontSize="20"
                                                   FontFamily="MaterialIcons"
                                                   TextColor="{StaticResource Primary}"
                                                   HorizontalTextAlignment="Center"
                                                   VerticalTextAlignment="Center" />
                                        </Border>
                                        <!-- Recipe info -->
                                        <VerticalStackLayout Grid.Column="1" VerticalOptions="Center" Spacing="2">
                                            <Label Text="{Binding Recipe.Name}" FontSize="15"
                                                   FontAttributes="Bold"
                                                   TextColor="{AppThemeBinding Light={StaticResource Light_TextPrimary}, Dark={StaticResource Dark_TextPrimary_Alt}}"
                                                   LineBreakMode="TailTruncation" MaxLines="1" />
                                            <Label FontSize="12" TextColor="{StaticResource TextSecondary}">
                                                <Label.Text>
                                                    <MultiBinding StringFormat="{}{0} cal · {1} min">
                                                        <Binding Path="Recipe.Calories" />
                                                        <Binding Path="Recipe.PrepTimeMinutes" />
                                                    </MultiBinding>
                                                </Label.Text>
                                            </Label>
                                        </VerticalStackLayout>
                                        <!-- Remove button -->
                                        <Border Grid.Column="2" StrokeThickness="0"
                                                StrokeShape="RoundRectangle 8"
                                                BackgroundColor="{StaticResource ChiliRedBg}"
                                                WidthRequest="32" HeightRequest="32" Padding="0"
                                                VerticalOptions="Center">
                                            <Label Text="&#xe5cd;" FontSize="16"
                                                   FontFamily="MaterialIcons"
                                                   TextColor="{StaticResource ChiliRed}"
                                                   HorizontalTextAlignment="Center"
                                                   VerticalTextAlignment="Center" />
                                            <Border.GestureRecognizers>
                                                <TapGestureRecognizer
                                                    Command="{Binding Source={RelativeSource AncestorType={x:Type vm:MealPlanViewModel}}, Path=RemoveMealCommand}"
                                                    CommandParameter="{Binding .}" />
                                            </Border.GestureRecognizers>
                                        </Border>
                                    </Grid>
                                    <Border.GestureRecognizers>
                                        <TapGestureRecognizer
                                            Command="{Binding Source={RelativeSource AncestorType={x:Type vm:MealPlanViewModel}}, Path=NavigateToDetailCommand}"
                                            CommandParameter="{Binding .}" />
                                    </Border.GestureRecognizers>
                                </Border>
                            </DataTemplate>
                        </BindableLayout.ItemTemplate>
                    </VerticalStackLayout>

                    <!-- Add Meal button -->
                    <Button Text="+ Add Meal" FontSize="14" FontAttributes="Bold"
                            BackgroundColor="{StaticResource Primary}"
                            TextColor="White" CornerRadius="12"
                            HeightRequest="48" BorderWidth="0"
                            Command="{Binding AddMealCommand}" />
                </VerticalStackLayout>
            </ScrollView>
        </RefreshView>

        <!-- Row 5: Back to This Week FAB -->
        <Border Grid.Row="5" StrokeShape="RoundRectangle 24" StrokeThickness="0"
                BackgroundColor="{AppThemeBinding Light=#3A2518, Dark=#1E1C18}"
                Padding="16,10" Margin="0,0,0,12"
                HorizontalOptions="Center"
                IsVisible="{Binding ShowBackToCurrent}">
            <Border.Shadow>
                <Shadow Brush="#30000000" Offset="0,4" Radius="12" Opacity="0.4" />
            </Border.Shadow>
            <HorizontalStackLayout Spacing="8">
                <Label Text="&#xe5d8;" FontSize="16" FontFamily="MaterialIcons"
                       TextColor="White" VerticalTextAlignment="Center" />
                <Label Text="Back to This Week" FontSize="14" FontAttributes="Bold"
                       TextColor="White" VerticalTextAlignment="Center" />
            </HorizontalStackLayout>
            <Border.GestureRecognizers>
                <TapGestureRecognizer Command="{Binding BackToCurrentWeekCommand}" />
            </Border.GestureRecognizers>
        </Border>

    </Grid>

</ContentPage>
```

**Step 2: Commit**

```bash
git add Views/MealPlanPage.xaml
git commit -m "feat: rewrite MealPlanPage.xaml with week strip + detail panel"
```

---

### Task 3: Update MealPlanPage.xaml.cs

**Files:**
- Modify: `Views/MealPlanPage.xaml.cs`

- [ ] **Step 1: Replace code-behind to use Reload() instead of LoadWeekPlan()**

Replace the file content with:

```csharp
using TastyMealPlanner.Services;
using TastyMealPlanner.ViewModels;

namespace TastyMealPlanner.Views;

public partial class MealPlanPage : ContentPage
{
    private readonly MealPlanViewModel _viewModel;
    private readonly ThemeService _theme;

    public MealPlanPage(MealPlanViewModel viewModel, ThemeService theme)
    {
        InitializeComponent();
        _theme = theme;
        BindingContext = _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _theme.ApplyFontScaleToPage(this);
        _viewModel.Reload();
    }
}
```

**Step 2: Commit**

```bash
git add Views/MealPlanPage.xaml.cs
git commit -m "refactor: simplify MealPlanPage code-behind to use Reload()"
```

---

### Task 4: Build and verify

**Step 1: Build the project**

```bash
dotnet build
```

Expected: Build succeeds with no errors.

**Step 2: Commit final check**

```bash
git status
```

Expected: Working tree clean.
