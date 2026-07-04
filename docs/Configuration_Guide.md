# Configuration Guide

## Architecture of `AppConfigManager`
The `AppConfigManager.cs` serves as the absolute single source of truth for all file paths, CAD configuration, and application state in the Mega Engineering Suite.

It is designed to be **100% portable** and **dynamic**. No paths are hardcoded to a specific developer's machine.

### Dynamic Root Resolution
When the application starts, it identifies its executing path (`AppDomain.CurrentDomain.BaseDirectory`). It searches upwards iteratively to find the `Templates` directory.
- If `Templates` is found, that directory becomes the `RootFolder`.
- If it is not found, the `RootFolder` defaults to one directory above the executable (to keep the `bin` folder clean).

### Folders Auto-Generated
The manager automatically creates the following subdirectories inside `RootFolder`:
- `Templates/Drawings/`
- `Templates/Excel/`
- `GeneratedDrawings/`
- `GeneratedLisp/`
- `Logs/`
- `Config/`

### Settings Persistence
User preferences (such as previously entered Dropdown histories) are serialized into JSON and saved automatically to:
`Config/Settings.json`
This ensures state is maintained between CAD generation sessions.

### CAD Auto-Discovery Fallback Waterfall
`AppConfigManager.DetectCadPath()` is a highly resilient method used to find GstarCAD:
1. **Active COM Object**: It hooks `oleaut32.dll` to find an actively running instance of `GstarCAD.Application`.
2. **Registry Activation**: It attempts to instantiate the COM object via `Activator.CreateInstance`.
3. **Hardcoded Fallbacks**: As a last resort, it scans common `C:\Program Files\...` installation paths.
