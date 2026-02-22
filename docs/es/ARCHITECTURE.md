# Arquitectura

## Resumen
Random Game Launcher es una aplicación WPF para Windows separada en dos capas:

- `RandomGameLauncher` (`net8.0-windows`): UI, integraciones específicas de Windows y arranque de app.
- `RandomGameLauncher.Core` (`net8.0`): contratos de dominio y lógica pura (sorteo, normalización de rutas, identidad de juego).
- `RandomGameLauncher.Tests` (`net8.0-windows`): tests unitarios de orquestación en capa app (ViewModels/servicios).

## Estructura del proyecto
- `RandomGameLauncher.Core/Abstractions`: interfaces (`IGameRepository`, `IGameLibraryProvider`, `IGameLauncher`, `IRandomGameSelector`, `IPathNormalizer`).
- `RandomGameLauncher.Core/Models`: modelos de dominio (`GameEntry`, `StoredGame`, `GameSource`).
- `RandomGameLauncher.Core/Services`: implementaciones puras (`BagRandomGameSelector`, `WindowsPathNormalizer`).
- `RandomGameLauncher.Core/Utilities`: utilidades compartidas (`GameIdentity`).
- `RandomGameLauncher/Services/Catalog`: orquestación del catálogo y proveedores (`GameCatalogService`, `JsonGameRepository`, `SteamGameLibraryProvider`, `EpicGameLibraryProvider`).
- `RandomGameLauncher/Services/Dialogs`: servicios de diálogo (`AddGameDialogService`, `ExecutableFilePicker`).
- `RandomGameLauncher/Services/Stores`: diagnóstico de rutas de tiendas (`StorePathService`).
- `RandomGameLauncher/Services/Launching`: adaptadores de lanzamiento (`WindowsGameLauncher`).
- `RandomGameLauncher/ViewModels/Main`: view model principal dividido por responsabilidad usando clases parciales.
- `RandomGameLauncher/Views/AddGame`: diálogo de alta manual implementado con code-behind simple (a propósito, más fácil de leer).
- `RandomGameLauncher/Collections`: helpers de colecciones para UI (`ObservableCollectionEx`).
- `RandomGameLauncher/Infrastructure/Commands`: implementaciones reutilizables de comandos sync/async.
- `RandomGameLauncher.Tests`: tests del comportamiento de `MainViewModel` y flujos de catálogo en capa app.

## Flujo de datos
1. `App` compone dependencias manualmente en `App.xaml.cs` (composition root explícito).
2. `MainViewModel` inicializa en forma asíncrona y delega carga de catálogo en `IGameCatalogService`.
3. `IGameCatalogService` combina entradas manuales del repositorio con juegos descubiertos por proveedores.
4. La UI muestra la lista unificada.
5. El juego aleatorio usa `IRandomGameSelector` con juegos activos.
6. El lanzamiento pasa por `IGameLauncher`.

## UX de selección
- El estado de participación por fila está en `Game.Active`.
- El checkbox de cabecera es tri-state:
  - `Checked`: todos activos.
  - `Unchecked`: ninguno activo.
  - `Indeterminate`: estado mixto.
- El estado de cabecera se calcula con contadores incrementales para mantener actualizaciones O(1), incluso con bibliotecas grandes.

## Persistencia
- Ruta: `%LocalAppData%/RandomGameLauncher/list.json`
- Datos guardados: solo juegos manuales (`StoredGame`)
- Migración: si existe `list.json` legado en ubicación anterior, se importa una vez.

## Parámetros de lanzamiento
Las entradas manuales soportan parámetros opcionales:
- Se cargan en el diálogo de alta.
- Se guardan en `StoredGame.LaunchArguments`.
- Se aplican en `ProcessStartInfo.Arguments` desde `WindowsGameLauncher`.

## Decisiones de diseño
- El arranque usa composición manual en vez de contenedor DI para que el bootstrap sea más fácil de leer para principiantes.
- Steam/Epic se tratan como fuentes descubribles, no como datos persistidos de usuario.
- Duplicados manuales se resuelven por ruta absoluta normalizada (case-insensitive en semántica Windows).
- El sorteo usa una bolsa sin repetición por conjunto activo y se reinicia cuando cambian los activos.
