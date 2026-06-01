# Tasty Meal Planner

A cross-platform mobile meal planning application built with **.NET MAUI (.NET 8)** for the 6G6Z0014 Mobile Computing module.

## App Overview

Tasty Meal Planner helps users organise their weekly meals, discover new recipes, manage shopping lists, and find nearby grocery stores. The app follows the **Food and Drink** theme with a focus on meal planning and recipe management.

### Main Features

- **Recipe Browser** — Browse 15 recipes across 6 categories (Breakfast, Lunch, Dinner, Dessert, Snack, Drink) with search and category filtering
- **Weekly Meal Planner** — Plan meals for each day of the week (Breakfast/Lunch/Dinner), view and remove planned meals
- **Shopping List** — Add items with quantities, check off purchased items, clear completed items with validation
- **Dark Mode** — Full light/dark theme switching with persisted preferences
- **Accessibility** — WCAG 2.1 AA compliant with screen reader support, adjustable font sizes, and 44pt+ touch targets

### Hardware Features (7 implemented)

| Feature | Description |
|---------|-------------|
| **Camera** | Capture food photos or pick from gallery with preview |
| **Flash / Torch** | Control device flashlight via CameraManager (Android) / AVCaptureDevice (iOS) |
| **GPS Location** | Get current location and discover nearby grocery stores (5 mock stores with distances) |
| **Text-to-Speech** | Read recipes aloud with adjustable pitch controls |
| **Accelerometer / Shake** | Shake the device to get a random recipe suggestion (debounced at 500ms) |
| **Compass** | Show current device heading direction (N/NE/E/SE/S/SW/W/NW) on Nearby page |
| **Haptic Feedback / Vibration** | Tactile click and long-press feedback + short vibration on all interactive elements |

## Screenshots

> Add app screenshots here before final submission. Recommended: home screen, recipe detail, meal planner, shopping list, nearby stores, and settings page. Show both light and dark mode.

## Getting Started

### Prerequisites

- **Visual Studio 2022** (Windows) or **Visual Studio for Mac 2022**
- **.NET 8 SDK** with the .NET MAUI workload
- For Android: Android SDK with API 21+
- For iOS (macOS only): Xcode 15+

### Install .NET MAUI Workload

```bash
dotnet workload install maui
```

### Build and Run

```bash
# Clone the repository
git clone https://github.com/2256450597/mobile-development-stage3.git
cd mobile-development-stage3

# Build the project
dotnet build

# Run on Android emulator
dotnet build -t:Run -f net8.0-android

# Run on Windows
dotnet build -t:Run -f net8.0-windows10.0.19041.0
```

Or open `TastyMealPlanner.sln` (if generated) or the project folder in Visual Studio and press **F5**.

## Architecture

### Pattern: MVVM (Model-View-ViewModel)

```
TastyMealPlanner/
├── Models/               # Data models (Recipe, MealPlanEntry, ShoppingItem, etc.)
├── ViewModels/           # MVVM ViewModels with data binding and commands
│   └── BaseViewModel.cs  # Base class with INotifyPropertyChanged
├── Views/                # XAML pages with code-behind
├── Services/             # Business logic and hardware abstraction layer
│   ├── IDataService.cs   # Data layer interface
│   ├── ICameraService.cs # Camera abstraction
│   ├── ILocationService.cs    # GPS/location abstraction
│   ├── ITextToSpeechService.cs # TTS abstraction
│   ├── IAccelerometerService.cs # Shake detection
│   ├── ICompassService.cs    # Compass/magnetometer abstraction
│   ├── CompassService.cs     # MAUI Compass sensor wrapper
│   └── IHapticService.cs      # Haptic feedback abstraction
├── Converters/           # XAML value converters
├── Helpers/              # Validation and error handling utilities
├── Resources/            # Colors, Styles, AppIcon, Splash screen
└── Platforms/            # Android, iOS, Windows entry points
```

### Navigation

Shell-based tab navigation with 5 tabs:
- **Home** — Today's meals overview, quick actions
- **Recipes** — Browseable/searchable recipe collection
- **Meal Plan** — 7-day weekly planner
- **Shopping** — Shopping list manager
- **Settings** — Theme, font size, TTS, accessibility info

Detail pages (Camera, Nearby Stores, Recipe Detail) are pushed onto the navigation stack.

### Dependency Injection

All services and ViewModels are registered in `MauiProgram.cs` via the built-in .NET DI container. Hardware services use interface-based abstraction for testability and separation of concerns.

## Accessibility

This app follows **WCAG 2.1** accessibility guidelines with specific principle references:

**Principle 1 — Perceivable**
- **1.1.1 Non-text Content** (`SemanticProperties.Description` on all images, buttons, and interactive elements)
- **1.3.1 Info and Relationships** (`SemanticProperties.HeadingLevel` Level1/Level2 on section headers)
- **1.4.3 Contrast (Minimum)** — AA compliance: 4.5:1 normal text, 3:1 large text (verified via AppThemeBinding)
- **1.4.4 Resize Text** — Three font sizes (Small 0.85×, Medium 1.0×, Large 1.3×) via Settings page

**Principle 2 — Operable**
- **2.4.2 Page Titled** — Every page has a descriptive `Title`
- **2.5.5 Target Size** — All buttons, entries, and controls ≥44pt (verified via `MinimumHeightRequest`)
- **2.2.2 Pause, Stop, Hide** — Stop button available for TTS speech playback

**Principle 3 — Understandable**
- **3.2.3 Consistent Navigation** — Shell tab bar present on all main pages
- **3.3.1 Error Identification** — Validation errors displayed with clear messages (`ValidationError` label)
- **3.3.2 Labels or Instructions** — Contextual user guidance labels on every page

**Principle 4 — Robust**
- **4.1.2 Name, Role, Value** — `SemanticProperties.Description` and `SemanticProperties.Hint` on all controls
- **Dark Mode** — Full dark/light theme via `AppThemeBinding` for reduced eye strain
- **Text-to-Speech** — Recipe instructions read aloud with adjustable pitch via `SemanticScreenReader`

## Development Plan

| Step | Focus | Status |
|------|-------|--------|
| Step 1 | Main Interface — Project scaffold, MVVM, 5 tabs, 15 recipes, Shell navigation | Done |
| Step 2 | Hardware — Camera, Flash, GPS, TTS, Accelerometer, Haptic, Compass (7 features) | Done |
| Step 3 | Themes & Accessibility — Dark mode, font scaling, WCAG, AppThemeBinding | Done |
| Step 4 | Polish — Validation, error handling, XML docs, README, code quality | Done |

## Deployment

The app targets:
- **Android** (API 21+) — phone and tablet emulators/physical devices
- **iOS** (15.0+) — requires macOS with Xcode
- **Windows** (10.0.17763.0+) — desktop deployment

Required permissions:
- Camera (`CAMERA`)
- Location (`ACCESS_FINE_LOCATION`, `ACCESS_COARSE_LOCATION`)
- Storage (`READ/WRITE_EXTERNAL_STORAGE`)
- Vibration (`VIBRATE`)

## Author

RongXiao Liu — Built for 6G6Z0014 Mobile Computing, Manchester Metropolitan University (2024/25).

## Technologies

- **.NET 8** / **.NET MAUI**
- **C# 12** / **XAML**
- **MVVM Architecture**
- **Shell Navigation**
- **Dependency Injection**

## License

This project is created for educational purposes as part of a university coursework assignment.
