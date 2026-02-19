# Proceso de Release

## ¿Cuándo se crea una release?
Una release se crea solo cuando haces push de un tag que cumpla `v*`.

Ejemplos:
- `v1.0.0`
- `v1.1.2`
- `v2.0.0-rc1`

## Pasos para principiantes
1. Asegúrate de que tus cambios ya estén mergeados.
2. Confirma que CI esté en verde (`Build and test`).
3. Crea y sube un tag:
   ```bash
   git tag vX.Y.Z
   git push origin vX.Y.Z
   ```
4. Abre GitHub Actions y revisa el workflow `Release`.
5. En la página de Releases, verifica los archivos subidos.

## Resumen del pipeline
1. Restaura dependencias.
2. Ejecuta tests de core.
3. Publica la app WPF para `win-x64`.
4. Crea `RandomGameLauncher.zip`.
5. Genera `RandomGameLauncher.zip.sha256`.
6. Crea la GitHub Release y sube ambos archivos.
