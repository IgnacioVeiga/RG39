# RG39
![RG39Icon](/RG39/Images/RG39.svg)</br>
Permite realizar un listado de los videojuegos que tengas instalados y ejecutar uno al azar.

## Capturas:

## Funcionalidades:
- [x] Busca y carga la libreria de Steam.
- [x] Carga ejecutables de forma manual.
- [x] Elimina individualmente a elementos de la lista.
- [x] Limpia la lista.
- [x] Detecta la ubicación de `steam.exe` de froma automatica.
- [x] Cargar de forma manual `steam.exe`.
- [x] Guarda la lista en un archivo `.xml` (excepto libreria de steam).
- [x] Marcar cuales elementos quiero que participen sin quitarlos de la lista.
- [x] Impide que se repitan los juegos añadidos.
- [x] Mostrar iconos de los videojuegos (solo añadidos manualmente).

## Incompleto:
- [ ] Soporte para más de un idioma.
- [ ] Permitir ordenar la lista.
- [ ] Reconocer la biblioteca de Epic Games Store, Origin y de GOG Galaxy.
- [ ] Mostrar iconos en la UI.
- [ ] Enseñar una portada/caratula.
- [ ] Arreglar los tamaños de los elementos.
- [ ] Usar temas personalizados.

## Requerido:
- Windows 7 o superior (Recomendado Windows 10/11)
- SDK .NET 6.0 (LTS)

## Dependencias:
### Frameworks
- Microsoft.NETCore.App **v6.0.10**
- Microsoft.WindowsDesktop.App.WPF **v6.0.10**

### Paquetes
- GameFinder.StoreHandlers.Steam **v2.2.1**
- WinCopies.WindowsAPICodePack.Shell **v2.12.0.2**
