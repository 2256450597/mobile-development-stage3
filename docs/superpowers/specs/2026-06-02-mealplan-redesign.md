# Meal Plan Page Redesign

**Date:** 2026-06-02
**Status:** Approved

## Overview

Redesign the Meal Plan page from a vertical zebra-striped card list into an interactive horizontal week-strip layout with swipeable week navigation and a detail panel for the selected day.

## Current State

- 7 day cards stacked vertically with zebra-stripe backgrounds
- Each card: day name header, separator, meal slots, outlined "+ Add Meal" button
- Functional but visually blocky, hard to see the full week at a glance

## Target Design

### Top: Week Navigation Bar

```
[←]    Jun 2 – Jun 8, 2026    [→]
            Week 23
```

- Left/right circular arrow buttons to navigate between weeks
- Center: date range + week number
- When on the current week, left arrow may be disabled (no future weeks either)
- Tap arrows to advance/go back one week at a time

### Middle: Horizontal Day Strip

```
[Mon 2] [Tue 3] [Wed 4 Today] [Thu 5] [Fri 6] [Sat 7] [Sun 8]
  (+)      (+)       (+)          (+)      (+)      (+)      (+)
   2        1         3            0        1        0        0
```

- 7 day cells in a horizontal row, equal width
- Each cell shows: weekday name (short), date number, dashed-circle "+" icon, meal count
- **Today**: red background (`#D9472B`) + white text + shadow + "Today" badge in top-right corner
- **Other days**: white background, gray border
- **Selected day**: distinct visual state (e.g. slightly different border or subtle indicator)
- The strip supports horizontal swipe with snap scrolling (built into CollectionView)
- Swiping the strip switches weeks; when not on current week, a "Back to This Week" FAB appears

### Bottom: Selected Day Detail Panel

```
┌─────────────────────────────────┐
│ ▌ Wednesday, June 4   Today·3 meals │
│                                 │
│ 🥞 BREAKFAST                    │
│    Classic Pancakes        [✕]  │
│    350 cal · 25 min             │
│                                 │
│ +  LUNCH                        │
│    Tap to add a meal            │
│                                 │
│ 🐟 DINNER                       │
│    Grilled Salmon          [✕]  │
│    450 cal · 22 min             │
└─────────────────────────────────┘
```

- White rounded-top panel (20px radius) with shadow
- Header: accent bar + full date + meal count badge
- Meal cards (filled slots):
  - Emoji icon in colored square (Breakfast=orange, Lunch=green, Dinner=green)
  - Meal type label (uppercase, small)
  - Recipe name (bold)
  - Calorie + time info
  - Red ✕ button (top-right of card) to remove
- Empty slots:
  - Dashed border card
  - "+" icon + "Tap to add a meal" text
  - Tap navigates to Recipes page in selection mode

## Interaction

| Action | Behavior |
|--------|----------|
| Tap ← / → arrows | Switch to previous/next week |
| Swipe day strip | Switch to adjacent week (snap scroll) |
| Tap a day cell | Select that day, update detail panel below |
| Tap filled meal card | Navigate to recipe detail page |
| Tap ✕ on meal card | Confirm dialog, then remove meal from plan |
| Tap empty slot (+) | Navigate to Recipes page in selection mode for that day |
| Pull down | RefreshView reloads current week data |
| Tap "Back to This Week" FAB | Jump back to current week, select today |

## ViewModel Changes

Add to `MealPlanViewModel`:
- `SelectedDay` property (DayOfWeek) — tracks which day is selected in the strip
- `CurrentWeekStart` property (DateTime) — Monday of the currently displayed week
- `SelectedDayMeals` collection — meals for the selected day
- `WeekLabel` property — e.g. "Jun 2 – Jun 8, 2026"
- `IsCurrentWeek` property — whether the displayed week is the current week
- `GoToPreviousWeekCommand` / `GoToNextWeekCommand`
- `SelectDayCommand` — selects a day and updates the detail panel
- `AddMealCommand` — updated to use SelectedDay instead of DayPlanGroup parameter
- "Back to This Week" logic: shown when `!IsCurrentWeek`

## Technology

- .NET MAUI XAML (same as rest of app)
- CollectionView with horizontal LinearItemsLayout for the day strip
- BindableLayout or CollectionView for meal cards in the detail panel
- Reuse existing `app` theme colors (`Primary=#D9472B`, background, card, text colors)
- No new NuGet packages required

## Notes

- Emoji icons (🥞 🐟 etc.) are not stored in the `Recipe` model. Use a ViewModel-level dictionary to map `FoodCategory` → emoji, or use Material Icons glyphs instead (consistent with the rest of the app). Design decision to be finalized during implementation.

## Scope

- This is only the Meal Plan page UI
- The underlying `MealPlanService` and `DayPlanGroup` model are unchanged
- Recipes page selection mode (PendingSelectionDay) integration remains the same
