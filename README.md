# RG39
![RG39Icon](/RG39/Assets/Images/RG39.svg)
&nbsp;&nbsp;&nbsp;&nbsp;
[![Download](https://img.shields.io/static/v1?style=flat-square&logo=windows&label=Download&message=windows-x64&color=000099&labelColor=009900)](https://github.com/IgnacioVeiga/RG39/releases/latest/download/RG39.exe)
![GitHub last commit](https://img.shields.io/github/last-commit/IgnacioVeiga/RG39?color=009900&style=flat-square)
![GitHub release (latest by date)](https://img.shields.io/github/v/release/IgnacioVeiga/RG39?color=darkblue&label=Latest%20release&style=flat-square)
![GitHub Workflow Status](https://img.shields.io/github/actions/workflow/status/IgnacioVeiga/RG39/deploy-project.yml?color=009900&logo=github&style=flat-square)
![GitHub license](https://img.shields.io/github/license/IgnacioVeiga/RG39?style=flat-square)
</br>
Permite realizar un listado de los videojuegos que tengas instalados y ejecutar uno al azar.

## Capturas:
![Screenshot_0000](/RG39/Assets/Images/Screenshot_0000.png "Games")
![Screenshot_0001](/RG39/Assets/Images/Screenshot_0001.png "Settings")
![Screenshot_0002](/RG39/Assets/Images/Screenshot_0002.png "Collapsed")

## Funcionalidades:
- [x] Busca y carga la libreria de Steam.
- [x] Carga ejecutables de forma manual.
- [x] Elimina individualmente a elementos de la lista.
- [x] Limpia la lista.
- [x] Detecta la ubicación de `steam.exe` de froma automatica.
- [x] Guarda la lista en un archivo `list.json` (solo los añadidos manualmente).
- [x] Muestra los iconos de los ejecutables (solo los añadidos manualmente).
- [x] Marcar cuales elementos quiero que participen sin quitarlos de la lista.
- [x] Impide que se repitan los juegos añadidos.
- [x] Al iniciar y leer el listado filtra los juegos no encontrados.
- [x] Idioma español e inglés.

## Incompleto:
- [ ] Permitir ordenar la lista.
- [ ] Reconocer la biblioteca de Epic Games Store, Origin y de GOG Galaxy.
- [ ] Mostrar iconos en la UI en vez de texto.
- [ ] Enseñar una portada/caratula.
- [ ] Arreglar los tamaños de los elementos.
- [ ] Usar temas personalizados.

## Requerido:
- Windows 7 o superior (Recomendado Windows 10/11)
- SDK .NET 6.0 (LTS) solo para compilar

## Dependencias:
### Frameworks
- Microsoft.NETCore.App **v6.0.10**
- Microsoft.WindowsDesktop.App.WPF **v6.0.10**

### Paquetes
- GameFinder.StoreHandlers.Steam **v2.2.1**
- WinCopies.WindowsAPICodePack.Shell **v2.12.0.2**

## Compilar:
Ejecuta el comando `dotnet build` en la raíz del repositorio. La otra opción es compilar a través de Visual Studio 2022.
