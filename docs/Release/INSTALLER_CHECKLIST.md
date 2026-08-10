# Installer Checklist

Run these manual validation steps with every compiled `MegaEngineeringSuite_Setup_vX.X.X.exe`:

## 1. Clean Environment Installation
- [ ] Run the installer on a machine (or VM) that has NEVER had Mega Engineering Suite installed.
- [ ] Verify it successfully installs to `Program Files`.
- [ ] Verify `Settings.json` is correctly placed in `Program Files\Mega Engineering Suite\Config\`.

## 2. First Launch
- [ ] Launch the application as a standard user.
- [ ] Verify `%LOCALAPPDATA%\MEGA Engineering Suite\Config\Settings.json` was correctly seeded.
- [ ] Verify `%LOCALAPPDATA%\MEGA Engineering Suite\GeneratedDrawings\` and `Logs\` folders are created automatically.

## 3. Functional Execution
- [ ] Perform a CAD calculation and generation workflow for Tube Sheet.
- [ ] Perform generation for Body Flange.
- [ ] Perform generation for Heat Exchanger.
- [ ] Ensure DWG files open cleanly in GstarCAD.
- [ ] Verify `Logs\Runtime` and `Logs\CAD` have structured Run ID entries, and `Logs\Errors` has no unexplained exceptions.

## 4. Upgrade Scenario
- [ ] Re-run the installer over the existing installation.
- [ ] Verify that existing `%LOCALAPPDATA%` user settings and `GeneratedDrawings` are preserved and NOT wiped by the installer.
