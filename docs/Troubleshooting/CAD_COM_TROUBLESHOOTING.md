# CAD COM Troubleshooting

If you encounter `CAD-001` or `CAD-002` errors in Mega Engineering Suite, follow these troubleshooting steps.

## The Problem
Mega Engineering Suite communicates with GstarCAD using Microsoft's Component Object Model (COM) interface. If this connection breaks, the application cannot generate drawings or resolve paths.

### 1. Verify GstarCAD is Running and Licensed
- Launch GstarCAD manually.
- Ensure no license prompts or dialogs are blocking the application.
- Try running the generation from the Suite while GstarCAD is already open.

### 2. Verify File Permissions
- `CAD-002` occurs if GstarCAD cannot read or write to the DWG output directory.
- Verify that `%LOCALAPPDATA%\MEGA Engineering Suite\GeneratedDrawings` is not blocked by antivirus.

### 3. Clear Stale COM Processes
Sometimes an invisible GstarCAD process gets stuck in the background and refuses COM connections.
1. Open Windows Task Manager (Ctrl+Shift+Esc).
2. Go to the **Details** tab.
3. Find and End Task for all `gcad.exe` processes.
4. Restart Mega Engineering Suite.

### 4. Provide Run ID
If you cannot resolve the issue, open a Bug Report on GitHub and provide the **Run ID** and the error log from `%LOCALAPPDATA%\MEGA Engineering Suite\Logs\Errors`.
