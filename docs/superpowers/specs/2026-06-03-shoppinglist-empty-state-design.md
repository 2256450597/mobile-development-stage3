# Shopping List Empty State Redesign

**Date:** 2026-06-03
**Status:** Approved

## Summary

Redesign the ShoppingListPage empty state to:
1. Fix the bug where empty state is always visible (add `IsVisible` binding)
2. Replace scattered multi-line text with a two-path explanation (Auto-add / Manual)

## Design

### XAML Changes (`Views/ShoppingListPage.xaml`)

Replace the existing empty state (lines 74-93) with:

- Icon: 🛒 (shopping cart, more thematic than 📋)
- Title: "Shopping List is Empty" (bold, 17px)
- Two explanation paths:
  - **🥘 Auto-add:** Choose recipes in Today's Meal — their ingredients show up here automatically.
  - **✏️ Manual:** Type an item name and quantity above, then tap Add.
- `IsVisible="{Binding IsListEmpty}"` to show only when list is empty

Each path uses `FormattedString` with bold labels (`Auto-add:` / `Manual:`) in Primary color, followed by descriptive text in TextSecondary.

### ViewModel

No changes needed. `ShoppingListViewModel.IsListEmpty` already exists and fires `PropertyChanged` via `UpdateProgress()`.

## Rationale

- **Approach B (Two-path explanation)** chosen: clear visual separation between auto-generated ingredients and manual entry
- No CTA button: user requested simplicity
- `IsListEmpty` binding fixes the bug where empty state overlapped with item list
