# Architecture

## Overview
Random Game Launcher is a Windows WPF application split into two layers:

- `RandomGameLauncher` (`net8.0-windows`): UI, Windows-specific integrations, app bootstrap.
- `RandomGameLauncher.Core` (`net8.0`): domain contracts and pure logic (random selection, path normalization, game identity).
- `RandomGameLauncher.Tests` (`net8.0-windows`): unit tests for app-layer orchestration (ViewModels/services).

## Project structure
- `RandomGameLauncher.Core/Abstractions`: interfaces (`IGameRepository`, `IGameLibraryProvider`, `IGameLauncher`, `IRandomGameSelector`, `IPathNormalizer`).
- `RandomGameLauncher.Core/Models`: domain transport models (`GameEntry`, `StoredGame`, `GameSource`).
- `RandomGameLauncher.Core/Services`: pure implementations (`BagRandomGameSelector`, `WindowsPathNormalizer`).
- `RandomGameLauncher.Core/Utilities`: shared helpers (`GameIdentity`).
- `RandomGameLauncher/Services`: infrastructure adapters and app orchestration services (JSON repository, game catalog service, Steam/Epic providers, executable picker, launcher, dialog service).
- `RandomGameLauncher/ViewModels`: WPF view models orchestrating use cases.
- `RandomGameLauncher.Tests`: tests for `MainViewModel` behavior and app-layer catalog flows.

## Data flow
1. `App` composes dependencies using `Microsoft.Extensions.DependencyInjection`.
2. `MainViewModel` initializes asynchronously and delegates catalog loading to `IGameCatalogService`.
3. `IGameCatalogService` merges manual repository entries and discovered provider entries.
4. UI exposes the unified list.
5. Random play uses `IRandomGameSelector` with active games only.
6. Launch goes through `IGameLauncher`.

## Selection UX
- Row activity state is represented by `Game.Active`.
- Header checkbox is tri-state:
  - `Checked`: all active.
  - `Unchecked`: none active.
  - `Indeterminate`: mixed state.
- Header state is calculated with incremental counters to keep updates O(1), even for larger libraries.

## Persistence
- File path: `%LocalAppData%/RandomGameLauncher/list.json`
- Stored payload: manual games only (`StoredGame`)
- Migration: legacy `list.json` in old location is imported once when possible.

## Launch parameters
Manual entries support optional launch arguments:
- Captured from Add Game dialog.
- Persisted in `StoredGame.LaunchArguments`.
- Passed to `ProcessStartInfo.Arguments` by `WindowsGameLauncher`.

## Design decisions
- Store libraries (Steam/Epic) are treated as discoverable sources, not persisted user entries.
- Duplicate manual games are resolved by normalized absolute path (case-insensitive for Windows semantics).
- Random selection uses a non-repeating bag per active set and resets when active flags change.
