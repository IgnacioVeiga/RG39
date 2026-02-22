# Empezar Por Aquí (Guía para Principiantes)

Esta guía es para entender el proyecto rápido antes de meterte en todos los archivos.

## Qué leer primero
1. `RandomGameLauncher/App.xaml.cs`
2. `RandomGameLauncher/MainWindow.xaml`
3. `RandomGameLauncher/ViewModels/Main/MainViewModel.cs`
4. `RandomGameLauncher/ViewModels/Main/MainViewModel.GameActions.cs`
5. `RandomGameLauncher/Services/Catalog/GameCatalogService.cs`
6. `RandomGameLauncher.Core/Services/BagRandomGameSelector.cs`

## Qué puedes ignorar al principio
- `RandomGameLauncher.Tests/` (tests de UI/app)
- `RandomGameLauncher.Core.Tests/` (volver después)
- `RandomGameLauncher/Resources/Styles/` (solo personalización visual)
- `RandomGameLauncher/Properties/*.Designer.cs` (autogenerados)
- `RandomGameLauncher/Resources/Language/*.Designer.cs` (autogenerados)

## Modelo mental (simple)
- `App` crea los objetos y abre la ventana principal.
- `MainWindow.xaml` define el layout de UI.
- `MainViewModel` reacciona a botones y eventos de UI.
- `GameCatalogService` carga/mezcla juegos y guarda los manuales.
- `Core` contiene lógica reutilizable (sorteo, normalización de rutas).
