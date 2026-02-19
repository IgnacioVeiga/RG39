# Copilot Instructions

## Project context
- This repository is a Windows WPF app (`RandomGameLauncher`) with a core logic library (`RandomGameLauncher.Core`).
- Keep pure logic in `RandomGameLauncher.Core` (`net8.0`).
- Keep Windows-specific behavior in `RandomGameLauncher` (`net8.0-windows`).

## Architecture rules
- Prefer dependency injection over static helpers.
- Add interfaces for new infrastructure dependencies.
- Avoid coupling `ViewModel` classes directly to `Window` classes.
- Treat Steam/Epic entries as discoverable data, not persisted user data.

## Data and behavior invariants
- Manual game persistence file is `%LocalAppData%/RandomGameLauncher/list.json`.
- Persist only manual games.
- Duplicate detection for manual games is based on normalized absolute path (case-insensitive semantics).
- Random selection must use active games only and avoid repetition until a cycle is complete.

## Launch behavior
- Manual games can include optional launch arguments.
- Steam/Epic launches use URI schemes.

## Testing expectations
- For core logic changes, update/add tests in `RandomGameLauncher.Core.Tests`.
- Validate WPF build changes on Windows CI.

## Pull request hygiene
- Keep commit messages short and in English.
- Keep changes scoped and avoid broad unrelated edits.
- Update docs in `docs/` when architecture, workflows, or behavior changes.
