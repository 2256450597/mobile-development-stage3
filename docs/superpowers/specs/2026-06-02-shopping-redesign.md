# Shopping List Page Redesign

**Date:** 2026-06-02
**Status:** Approved

## Overview

Redesign the Shopping List page with a compact add bar at top, progress indicator, two-section grouped list (Today's Ingredients auto / My Items manual), and a clear-purchased button with count badge.

## Target Design

### Top: Compact Add Bar
- Single-row: Item name input + Qty input + Add button
- Name input takes remaining space (flex)
- Qty input fixed width (~64px)
- Add button: Primary color, rounded, bold

### Progress Section
- "Shopping List" label + "N of M remaining" counter
- Linear progress bar showing completion ratio
- Auto-updates when items are checked/unchecked

### Section 1: Today's Ingredients (Auto)
- Section header: accent bar + "Today's Ingredients" + "Auto" badge
- Subtitle: "Auto-generated from today's meal plan. Tap to check off."
- Items displayed as **rounded pill chips** (horizontal wrap)
- Each chip: checkbox + name + quantity
- Checked chips: gray background, strikethrough, green check

### Section 2: My Items (Manual)
- Section header: accent bar + "My Items" + count badge
- Items displayed as **rounded cards** (vertical list)
- Each card: checkbox + name (bold) + quantity (below) + ✕ delete
- Checked cards: light green background, strikethrough, gray text

### Bottom: Clear Purchased
- Full-width outlined red button
- Shows count of checked items: "Clear Purchased (N)"
- Hidden when count is 0
- Confirm dialog before clearing

## Interaction (unchanged from current)
- Add: validate name required, trim whitespace, clear inputs on success
- Toggle: click checkbox to mark complete/incomplete
- Delete: click ✕ removes single item
- Clear Purchased: confirm dialog, then remove all checked
- Pull to refresh: regenerate auto items from today's meal plan
- Validation error: display inline message below add bar

## ViewModel Changes (minimal)
- Add `RemainingCount` property: count of unchecked items
- Add `TotalCount` property: total items
- Add `ProgressText` property: "N of M remaining"
- Add `ProgressFraction` property: double 0.0–1.0 for progress bar
- Add `CheckedCount` property: for Clear Purchased button text
- Add `ShowClearButton` property: true when CheckedCount > 0

## XAML Layout
```
Grid:
  Row 0: HeaderLogo
  Row 1: Add Bar (Border with Entry+Entry+Button)
  Row 2: Progress (Label + ProgressBar)
  Row 3: Validation error (Label, conditional)
  Row 4: RefreshView+ScrollView:
    - Today's Ingredients section (BindableLayout, chip style)
    - My Items section (BindableLayout or CollectionView, card style)
  Row 5: Clear Purchased button (conditional)
```

## Scope
- Visual redesign only
- All existing ViewModel commands and service calls remain unchanged
- ShoppingListService and ShoppingItem model unchanged
