# Desarrollo

## Requisitos
- Windows 10/11 (recomendado) para compilar y ejecutar WPF.
- .NET SDK 8.x.

## Comandos locales
Desde la raíz del repositorio:

```bash
dotnet restore RandomGameLauncher.sln
```

Validación solo Core (funciona en Linux/macOS/Windows):

```bash
dotnet build RandomGameLauncher.Core/RandomGameLauncher.Core.csproj
dotnet test RandomGameLauncher.Core.Tests/RandomGameLauncher.Core.Tests.csproj
```

Validación completa de app (solo Windows):

```powershell
dotnet build RandomGameLauncher/RandomGameLauncher.csproj -c Release -r win-x64
dotnet test RandomGameLauncher.Tests/RandomGameLauncher.Tests.csproj -c Release
```

## Guías de código
- Mantener lógica pura en `RandomGameLauncher.Core`.
- Mantener código específico de Windows en `RandomGameLauncher/Services`.
- Evitar nuevos servicios estáticos.
- Preferir dependencias por interfaz e inyección por constructor.
- Mantener comportamiento de propiedad manual vs bibliotecas.

## Estrategia de testing
- Agregar/extender tests unitarios en `RandomGameLauncher.Core.Tests` cuando cambie lógica de core.
- Agregar/extender tests unitarios en `RandomGameLauncher.Tests` cuando cambie lógica de ViewModels o de orquestación de app.
- Ejecutar build WPF en CI de Windows antes de mergear.

## Bugs conocidos
- El checkbox de actividad en el header del DataGrid puede no propagar su acción a todas las filas en ciertos estados de UI.
  - Alcance: comportamiento del checkbox de cabecera en la lista de juegos de `MainWindow`.
  - Estado actual: bug reconocido, pendiente de fix robusto.
  - Workaround: usar los checkboxes por fila para cambios de estado por ítem.

## Estilo de commits
- Mensajes cortos en inglés.
- Commits enfocados (lógica, docs, CI).
