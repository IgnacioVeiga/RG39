# Start Here (Beginner Guide)

This guide is for understanding the project quickly before diving into all files.

## What to read first
1. `RandomGameLauncher/App.xaml.cs`
2. `RandomGameLauncher/MainWindow.xaml`
3. `RandomGameLauncher/ViewModels/Main/MainViewModel.cs`
4. `RandomGameLauncher/ViewModels/Main/MainViewModel.GameActions.cs`
5. `RandomGameLauncher/Services/Catalog/GameCatalogService.cs`
6. `RandomGameLauncher.Core/Services/BagRandomGameSelector.cs`

## What you can ignore at first
- `RandomGameLauncher.Tests/` (UI/app tests)
- `RandomGameLauncher.Core.Tests/` (come back later)
- `RandomGameLauncher/Resources/Styles/` (visual customization only)
- `RandomGameLauncher/Properties/*.Designer.cs` (auto-generated)
- `RandomGameLauncher/Resources/Language/*.Designer.cs` (auto-generated)

## Mental model (simple)
- `App` creates the objects and opens the main window.
- `MainWindow.xaml` defines the UI layout.
- `MainViewModel` reacts to button clicks and UI events.
- `GameCatalogService` loads/merges games and saves manual entries.
- `Core` contains reusable logic (random selection, path normalization).
