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
```

## Guías de código
- Mantener lógica pura en `RandomGameLauncher.Core`.
- Mantener código específico de Windows en `RandomGameLauncher/Services`.
- Evitar nuevos servicios estáticos.
- Preferir dependencias por interfaz e inyección por constructor.
- Mantener comportamiento de propiedad manual vs bibliotecas.

## Estrategia de testing
- Agregar/extender tests unitarios en `RandomGameLauncher.Core.Tests` cuando cambie lógica de core.
- Ejecutar build WPF en CI de Windows antes de mergear.

## Estilo de commits
- Mensajes cortos en inglés.
- Commits enfocados (lógica, docs, CI).
