# RG39 (Beta)
[![Download](https://img.shields.io/static/v1?style=flat-square&logo=windows&label=Descargar&message=windows-x64&color=3BBF3B&labelColor=24282F)](https://github.com/IgnacioVeiga/RG39/releases/latest/download/RG39.zip)
![GitHub last commit](https://img.shields.io/github/last-commit/IgnacioVeiga/RG39?color=3BBF3B&style=flat-square)
![GitHub release (latest by date)](https://img.shields.io/github/v/release/IgnacioVeiga/RG39?color=3BBF3B&label=Latest%20release&style=flat-square)
![GitHub Workflow Status](https://img.shields.io/github/actions/workflow/status/IgnacioVeiga/RG39/deploy-project.yml?color=3BBF3B&logo=github&style=flat-square)
![GitHub license](https://img.shields.io/github/license/IgnacioVeiga/RG39?style=flat-square)

<img src="/RG39/Resources/Icons/icon.ico" width="128" height="128">
<div>
  <a href="README.md">English</a> / <span>Español</span></a>
</div></br>

Permite realizar un listado de los videojuegos que tengas instalados y ejecutar uno al azar.

## Capturas de pantalla:
![Screenshot](/RG39/Resources/Images/Screenshot_es.png "Ventana principal")

***

## Funcionalidades:
- Busca y carga la libreria de Steam.
- Carga ejecutables de forma manual.
- Elimina individualmente a elementos de la lista.
- Limpia la lista.
- Detecta la ubicación de `steam.exe` de froma automatica.
- Guarda la lista en un archivo `list.json` (solo los añadidos manualmente).
- Muestra los iconos de los ejecutables (solo los añadidos manualmente).
- Marcar cuales elementos quiero que participen sin quitarlos de la lista.
- Impide que se repitan los juegos añadidos.
- Al iniciar y leer el listado filtra los juegos no encontrados.
- Permitir ordenar la lista.
- Idioma español e inglés.

## Para hacer:
- Reconocer la biblioteca de Epic Games Store.
- Enseñar una portada/caratula.
- Usar temas personalizados.

***

## Como usar:

***

## Requerido:
- Windows 7 o superior (Recomendado Windows 10/11) x64.
- .NET SDK 6 (LTS) para compilar y ejecutar.
- Entorno de ejecución de escritorio de .NET solo si es para ejecutar.

***

## Dependencias:
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

## Idiomas
Para añadir/modificar idiomas recomiendo ampliamente la **extensión** para **Visual Studio 2022** llamada `ResX Manager`. Hace mucho más facil manejar varios idiomas.
Los arhivos `.resx` de idioma se guardan en la carpeta `.\RG39\Language\`.

***

## Compilar:
Compilar a través de **Visual Studio 2022**. La otra forma es ejecutar el comando `dotnet build` desde el terminal (cmd/powershell) en la raíz del repositorio y luego comprobar dentro de la carpeta `\RG39\bin\`.

***

## Como contribuir:

***

## Licencia: