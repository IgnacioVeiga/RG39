# Release Process

## When is a release created?
A release is created only when a tag that matches `v*` is pushed.

Examples:
- `v1.0.0`
- `v1.1.2`
- `v2.0.0-rc1`

## Beginner steps
1. Make sure your changes are already merged.
2. Make sure CI is green (`Build and test` workflow).
3. Create and push a tag:
   ```bash
   git tag vX.Y.Z
   git push origin vX.Y.Z
   ```
4. Open GitHub Actions and check the `Release` workflow.
5. Open the Releases page and verify uploaded files.

## Pipeline summary
1. Restore dependencies.
2. Run core tests.
3. Publish WPF app for `win-x64`.
4. Create `RandomGameLauncher.zip`.
5. Generate `RandomGameLauncher.zip.sha256`.
6. Create GitHub Release and upload both files.
