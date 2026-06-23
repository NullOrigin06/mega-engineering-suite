# MEGA TubeSheet Automation

MEGA TubeSheet Automation is a professional C# Windows Forms application designed to automate the generation of highly detailed, anchor-based engineering drawings for Heat Exchanger Tube Sheets using GstarCAD.

## Overview

Generating intricate Tube Sheet drawings manually is time-consuming and prone to human error. This project bridges the gap between engineering calculations and CAD drafting by dynamically generating precise AutoLISP (`.lsp`) scripts. These scripts communicate directly with GstarCAD to automatically draft Front and Rear Tube Sheets, complete with annotations, hole patterns, bolt circles, flanges, and side-view cross-sections.

## Features

- **Anchor-Based Template System**: Uses predefined CAD anchors (`REAR_TS_ANCHOR`, `FRONT_TS_ANCHOR`, `REAR_SIDEVIEW_ANCHOR`, `FRONT_SIDEVIEW_ANCHOR`) to insert geometry directly into specific coordinates of a pre-existing engineering drawing template.
- **Parametric Generation**: Generates geometry based on exact engineering inputs (e.g., Tube OD, Pitch, Pass Partitions, Flange ID/OD).
- **Drafting Automation**: Automatically scales and places dimensions, leaders, and text (e.g., Row-count labels) on corresponding sides (Rear on the right, Front mirrored to the left).
- **Side View Cross Sections**: Automatically creates rectangular side profiles with `ANSI31` hatching and thickness dimensions.
- **Layer & Style Management**: Ensures compliance with drafting standards (e.g., Centerlines on `CENTER`, Bolt holes on `PHANTOM` with Red styling, inner tube holes in Blue).
- **One-Click Execution**: Generates the LISP script, creates an SCR file, and launches GstarCAD to execute the drawing completely unattended.

## Architecture & Technologies Used

- **Language**: C#
- **Framework**: .NET (Windows Forms)
- **CAD Automation**: AutoLISP (`.lsp`) and AutoCAD Scripting (`.scr`)
- **Target CAD Platform**: GstarCAD (Compatible with AutoCAD)

## Installation

1. **Prerequisites**:
   - .NET SDK (Compatible with .NET 10.0 / .NET 6.0+)
   - GstarCAD (or AutoCAD) installed on the host machine.
2. **Clone the Repository**:
   ```bash
   git clone https://github.com/yourusername/mega-tubesheet-automation.git
   ```
3. **Build the Project**:
   Open the solution in Visual Studio and build, or run:
   ```bash
   dotnet build
   ```

## Usage

1. Launch the `loginpage1.exe` application.
2. Input the required mechanical parameters for the Tube Sheet (OD, ID, Tube Pitch, Tube Count, Passes, etc.).
3. Click the **Generate Template CAD** button.
4. The application will compute all coordinates, generate a `.lsp` script, and automatically launch GstarCAD to draw the components at the designated template anchors.

## Project Structure

- `DrawingAutomationService.cs`: The core engine responsible for compiling the AutoLISP syntax, extracting anchor coordinates, and sequencing the drawing phases.
- `TubeSheetViewBase.cs`: Base logic for geometric calculations.
- `FrontTubeSheetView.cs` / `RearTubeSheetView.cs`: Specific implementations mirroring the front and rear configurations.
- `DrawingLayoutEngine.cs`: Handles spacing, scaling, and spatial reasoning for the drafting layout.

## Future Roadmap

- Further alignment of drafting standards between views.
- Support for additional Heat Exchanger components (Baffles, Shell, Channels).
- Enhanced collision detection for dense annotation fields.

## License

This project is licensed under the MIT License.
