# Random Game Launcher
[![Download](https://img.shields.io/static/v1?style=flat-square&logo=windows&label=Download&message=windows-x64&color=3BBF3B&labelColor=24282F)](https://github.com/IgnacioVeiga/RandomGameLauncher/releases/latest/download/RandomGameLauncher.zip)
![GitHub last commit](https://img.shields.io/github/last-commit/IgnacioVeiga/RandomGameLauncher?color=3BBF3B&style=flat-square)
![GitHub release (latest by date)](https://img.shields.io/github/v/release/IgnacioVeiga/RandomGameLauncher?color=3BBF3B&label=Latest%20release&style=flat-square)
![GitHub Workflow Status](https://img.shields.io/github/actions/workflow/status/IgnacioVeiga/RandomGameLauncher/create-release.yml?color=3BBF3B&logo=github&style=flat-square)
![GitHub license](https://img.shields.io/github/license/IgnacioVeiga/RandomGameLauncher?style=flat-square)

<img src="/RandomGameLauncher/Resources/Icons/icon.ico" width="128" height="128">
<div>
  <span>English</span> / <a href="README_es.md">Español</a> </a>
</div></br>

It allows you to make a list of the video games you have installed and run one at random.

## Screenshots
// Add here ///

***

## Functionalities
- Loads Steam and Epic Games libraries.
- Manually loads executables.
- Removes individual items from the list.
- Clear the list.
- Detects the location of `Steam` and `Epic Games Store` automatically.
- Saves manually added games to `%LocalAppData%\RandomGameLauncher\list.json`.
- Show the icons of the executables (only the manually added ones, for now).
- Mark which items I want to participate without removing them from the list.
- Supports launch parameters for manually added games.
- Prevents the added games from being repeated.
- When starting and reading the list, it filters the games not found.
- Allows to sort the list.
- English and Spanish language.

## To do
- Show a cover/cover.
- Use custom themes.
- Group marked games in different configurations.
- Check for updates to itself.
- Verify operation with various Epic Games Store games (still in experimental phase).

Translated with DeepL.com (free version)

***

## How to use
When the program starts it will automatically try to make a list of installed Steam (and soon Epic Games) games, if it doesn't find anything it loads manually added games from `%LocalAppData%\RandomGameLauncher\list.json` (migrating legacy `list.json` from the old location when present). After that it displays the list with the found games, to add manually you have to go to `Games` > `Add games` and load the executable.
***

## Required
- Windows 7 or higher (Recommended Windows 10/11) x64.
- .NET SDK 8 (LTS) to compile and run.
- .NET Desktop Runtime 8 (LTS) to run.

***

## Dependencies
### Frameworks
- Microsoft.NETCore.App
- Microsoft.WindowsDesktop.App.WPF

### Packages
- System.Drawing.Common
- GameFinder

***

## Languages
For adding/modifying languages I recommend the **extension** for **Visual Studio 2022** called `ResX Manager`.
The language `.resx` files are saved in the `.\RandomGameLauncher\Resources\Language\` folder.

***

## Documentation
- [Architecture](docs/ARCHITECTURE.md)
- [Development](docs/DEVELOPMENT.md)
- [Release process](docs/RELEASE.md)
- [Roadmap](docs/ROADMAP.md)

***

## Compile
Compile via **Visual Studio 2022**. The other way is to run the `dotnet build` command from terminal (cmd/powershell) in the root of the repository and then check inside of the `\RandomGameLauncher\bin\` folder.

***

## Contribute
Fork the repository and create a pull request with your changes.
