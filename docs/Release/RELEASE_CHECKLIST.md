# Release Checklist

Before tagging and publishing a new release of Mega Engineering Suite, perform the following verification:

## 1. Versioning Integrity
- [ ] Verify `MegaEngineeringSuite.csproj` `<Version>` and `<AssemblyVersion>` match the intended release.
- [ ] Verify `Setup.iss` `#define MyAppVersion` matches.
- [ ] Verify `README.md` or Changelogs mention the new version correctly.

## 2. Compilation
- [ ] Run `dotnet build MegaEngineeringSuite.slnx -c Release`.
- [ ] Verify 0 Errors and 0 Warnings.

## 3. Security & Path Audit
- [ ] Verify no `C:\Users\` paths exist in configuration, templates, or runtime code.
- [ ] Verify no sensitive keys or passwords are in the repository.

## 4. Testing
- [ ] Execute `TestConsole` automated test runners to ensure data pipeline parity and stability.
- [ ] Verify `Docs/Testing/ERROR_REGRESSION_MATRIX.md` has been updated with results for all historical bugs.

## 5. Installer Preparation
- [ ] Compile the installer using Inno Setup.
- [ ] Complete the `INSTALLER_CHECKLIST.md` steps using the compiled `.exe`.

## 6. Deployment
- [ ] Create a GitHub Release and tag exactly matching the version (e.g. `v1.2.2`).
- [ ] Upload the `.exe` installer as a release asset.
