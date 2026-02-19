# Release Process

## Trigger
A release is created when pushing a tag that matches:

- `v*` (for example: `v1.3.0`, `v1.4.0-rc1`)

## Pipeline summary
1. Restore and test core logic.
2. Publish WPF app for `win-x64`.
3. Package publish output into `RandomGameLauncher.zip`.
4. Generate `RandomGameLauncher.zip.sha256`.
5. Create GitHub Release and upload both artifacts.

## Suggested release checklist
1. Merge all pending changes to the target branch.
2. Confirm CI is green (`Build and test`).
3. Create and push a tag:
   - `git tag vX.Y.Z`
   - `git push origin vX.Y.Z`
4. Verify generated assets and checksum in GitHub Release.
