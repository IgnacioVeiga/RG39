# RG39
![RG39Icon](/RG39/icon.ico)<br /> <br />
Permite realizar un listado de los videojuegos que tengas instalados y ejecutar uno al azar.

## Funcionalidades:
- [x] Busca y carga la libreria de Steam y de Epic Games Store.
- [x] Carga ejecutables de forma manual.
- [x] Elimina individualmente a elementos de la lista.
- [x] Limpia la lista.
- [x] Detecta la ubicación de `steam.exe` de froma automatica.
- [x] Cargar de forma manual `steam.exe`.
- [x] Guarda la lista en un archivo `.xml` (excepto libreria de steam).
- [x] Marcar cuales elementos quiero que participen sin quitarlos de la lista.
- [x] Impide que se repitan los juegos añadidos.

## Incompleto:
- [ ] Soporte para más de un idioma.
- [ ] Permitir ordenar la lista.
- [ ] Reconocer la biblioteca de Origin
- [ ] Reconocer la biblioteca de GOG Galaxy
- [ ] Mostrar iconos de los videojuegos
- [ ] Mostrar iconos en la UI
- [ ] Enseñar una portada/caratula.
- [ ] Arreglar los tamaños de los elementos.
- [ ] Usar temas personalizados.

## Requerido
SDK .NET 6.0 (LTS)

## Dependencias 
- GameFinder.StoreHandlers.Steam **v2.2.1**
- WinCopies.WindowsAPICodePack.Shell **2.12.0.2**
