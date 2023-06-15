# RG39 (Beta)
[![Download](https://img.shields.io/static/v1?style=flat-square&logo=windows&label=Download&message=windows-x64&color=3BBF3B&labelColor=24282F)](https://github.com/IgnacioVeiga/RG39/releases/latest/download/RG39.exe)
![GitHub last commit](https://img.shields.io/github/last-commit/IgnacioVeiga/RG39?color=3BBF3B&style=flat-square)
![GitHub release (latest by date)](https://img.shields.io/github/v/release/IgnacioVeiga/RG39?color=3BBF3B&label=Latest%20release&style=flat-square)
![GitHub Workflow Status](https://img.shields.io/github/actions/workflow/status/IgnacioVeiga/RG39/deploy-project.yml?color=3BBF3B&logo=github&style=flat-square)
![GitHub license](https://img.shields.io/github/license/IgnacioVeiga/RG39?style=flat-square)

<img src="/RG39/Resources/Icons/icon.ico" width="128" height="128">
<div>
  <span>English</span> / <a href="README_es.md">Español</a> </a>
</div></br>

It allows you to make a list of the video games you have installed and run one at random.

## Screenshots:
![Screenshot](/RG39/Resources/Images/Screenshot.png "Main window")

***

## Functionalities:
- Search and load the Steam library.
- Load executables manually.
- Individually removes items from the list.
- Clear the list.
- Detects the location of `steam.exe` automatically.
- Save the list in a `list.json` file (only those added manually).
- Show executable icons (only those added manually).
- Mark which elements I want to participate without removing them from the list.
- Prevents the added games from being repeated.
- When starting and reading the list, it filters the games not found.
- Spanish and English language.
- Allow to sort the list.

## To do:
- Recognize the library of Epic Games Store.
- Show a cover / cover.
- Use custom themes.

***

## How to use:

***

## Required:
- Windows 7 or higher (Recommended Windows 10/11) x64.
- .NET SDK 6 (LTS) to compile and run.
- .NET Desktop Runtime 6 to run.

***

## Dependencies:
### Frameworks
- Microsoft.NETCore.App **(6.0.x)**.
- Microsoft.WindowsDesktop.App.WPF **(6.0.x)**

### Paquetes
- Microsoft.EntityFrameworkCore.Design **(7.0.7)**
- Microsoft.EntityFrameworkCore.Sqlite **(7.0.7)**
- GameFinder.StoreHandlers.Steam **(2.5.0)**
- GameFinder.StoreHandlers.EGS **(2.5.0)**
- WinCopies.WindowsAPICodePack.Shell **(2.12.0.2)**

***

## Languajes
For adding/modifying languages I highly recommend the **extension** for **Visual Studio 2022** called `ResX Manager`. It makes it much easier to manage multiple languages.
The language `.resx` files are saved in the `.\RG39\Language\` folder.

***

## Compile:
Compile via **Visual Studio 2022**. The other way is to run the `dotnet build` command from terminal (cmd/powershell) in the root of the repository and then check inside of the `\RG39\bin\` folder.

***

## How contribute:

***

## License:

