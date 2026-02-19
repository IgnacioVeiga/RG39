# Arquitectura

## Resumen
Random Game Launcher es una aplicación WPF para Windows separada en dos capas:

- `RandomGameLauncher` (`net8.0-windows`): UI, integraciones específicas de Windows y arranque de app.
- `RandomGameLauncher.Core` (`net8.0`): contratos de dominio y lógica pura (sorteo, normalización de rutas, identidad de juego).

## Estructura del proyecto
- `RandomGameLauncher.Core/Abstractions`: interfaces (`IGameRepository`, `IGameLibraryProvider`, `IGameLauncher`, `IRandomGameSelector`, `IPathNormalizer`).
- `RandomGameLauncher.Core/Models`: modelos de dominio (`GameEntry`, `StoredGame`, `GameSource`).
- `RandomGameLauncher.Core/Services`: implementaciones puras (`BagRandomGameSelector`, `WindowsPathNormalizer`).
- `RandomGameLauncher.Core/Utilities`: utilidades compartidas (`GameIdentity`).
- `RandomGameLauncher/Services`: adaptadores de infraestructura (repositorio JSON, proveedores Steam/Epic, selector de ejecutable, launcher, servicio de diálogo).
- `RandomGameLauncher/ViewModels`: view models WPF que orquestan casos de uso.

## Flujo de datos
1. `App` compone dependencias con `Microsoft.Extensions.DependencyInjection`.
2. `MainViewModel` carga juegos manuales desde `IGameRepository`.
3. Los proveedores (`Steam`, `Epic`) agregan juegos descubiertos.
4. La UI muestra la lista unificada.
5. El juego aleatorio usa `IRandomGameSelector` con juegos activos.
6. El lanzamiento pasa por `IGameLauncher`.

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
- Steam/Epic se tratan como fuentes descubribles, no como datos persistidos de usuario.
- Duplicados manuales se resuelven por ruta absoluta normalizada (case-insensitive en semántica Windows).
- El sorteo usa una bolsa sin repetición por conjunto activo y se reinicia cuando cambian los activos.
