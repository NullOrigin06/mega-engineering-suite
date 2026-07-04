# Deployment Guide

## Overview
This document outlines the steps required to prepare, build, and deploy the Mega Engineering Suite to a clean target machine (e.g., a senior engineer's desktop or a production workstation).

## Prerequisites on Target Machine
- **GstarCAD**: Must be installed. The application detects the active COM object or standard installation paths automatically.
- **.NET Runtime**: The machine must have the `.NET 10.0 Windows Desktop Runtime` installed.
- **Permissions**: The application requires write permissions to its root directory to generate temporary LISP files, output drawings, and logs.

## Build and Distribution
To distribute this software without requiring Visual Studio on the target machine:

1. **Publish the Project**:
   Run the following command from the source directory to create a self-contained release:
   ```bash
   dotnet publish -c Release -r win-x64 --self-contained false
   ```
2. **Packaging**:
   Zip the contents of the `bin/Release/net10.0-windows/win-x64/publish/` folder.
3. **Template Setup**:
   Ensure you include the `Templates/Drawings/` and `Templates/Excel/` directories alongside the published `.exe`. The application will dynamically find them as long as they are located in the same directory as the executable or any parent directory.

## First-Run Experience
Upon first execution, `MegaEngineeringSuite.exe` will:
1. Validate the system for GstarCAD.
2. Check for the presence of the three mandatory templates.
3. Auto-generate the `Config`, `GeneratedDrawings`, `GeneratedLisp`, and `Logs` folders.
4. If anything is missing, a friendly message will guide the user to resolve the dependency rather than crashing.
