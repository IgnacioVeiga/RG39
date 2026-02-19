# Development

## Requirements
- Windows 10/11 (recommended) for WPF build and runtime.
- .NET SDK 8.x.

## Local commands
From repository root:

```bash
dotnet restore RandomGameLauncher.sln
```

Core-only validation (works on Linux/macOS/Windows):

```bash
dotnet build RandomGameLauncher.Core/RandomGameLauncher.Core.csproj
dotnet test RandomGameLauncher.Core.Tests/RandomGameLauncher.Core.Tests.csproj
```

Full app validation (Windows only):

```powershell
dotnet build RandomGameLauncher/RandomGameLauncher.csproj -c Release -r win-x64
```

## Coding guidelines
- Keep pure logic in `RandomGameLauncher.Core`.
- Keep Windows-specific code in `RandomGameLauncher/Services`.
- Avoid new static service classes.
- Prefer interface-driven dependencies and constructor injection.
- Preserve behavior for manual-vs-library game ownership.

## Testing strategy
- Add/extend unit tests in `RandomGameLauncher.Core.Tests` for any change in core logic.
- Run WPF build on Windows CI before merge.

## Commit style
- Use short English commit messages.
- Keep commits focused (logic, docs, CI).
