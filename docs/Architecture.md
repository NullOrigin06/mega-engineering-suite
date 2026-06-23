# Mega Engineering Suite - Architecture

## System Overview
The Mega Engineering Suite is a modular Windows-based desktop application designed to fully automate the parametric generation of industrial engineering drawings. The suite takes standard engineering inputs (such as diameters, pressures, thicknesses, and layouts), performs geometrical and structural computations, and generates procedural AutoLISP macros that automatically draw the components in GstarCAD.

## WinForms Architecture
The user interface is built on .NET Windows Forms (WinForms). It acts as the orchestration layer:
- **Form1 / Form2**: Handle initial landing and component selection.
- **Form3**: Dedicated interface for the TubeSheet Module, collecting raw engineering data (Finish OD, Flange ID, Bolt PCD, Tube counts, Pitches).
- **EngineeringDataModel**: Serves as the DTO bridging the UI layer to the calculation engine.

## AutoLISP Generation Workflow
Rather than relying on COM interop for every drawing command (which is slow and error-prone), the application functions as a **Procedural Macro Generator**:
1. **Geometry Model generation**: The calculation engine translates engineering data into geometric objects (`CadCircle`, `CadLine`, `CadHatch`, `CadDimension`, etc.).
2. **Drafting Abstraction**: Classes deriving from `ICadView` (like `RearTubeSheetView` and `FrontTubeSheetView`) orchestrate these objects into comprehensive 2D representations.
3. **AutoLISP Building**: `DrawingAutomationService` serializes these objects into robust AutoLISP strings using pure text generation.
4. **Execution**: The system saves the script and invokes the CAD engine via COM `SendCommand`, completely decoupling the calculation engine from the CAD platform's API limitations.

## Template Architecture
Instead of drawing standard title blocks or fixed notes programmatically every time, the application leverages a Template Architecture:
- A base DWG file containing title blocks, standard notes, and empty viewports is loaded.
- The automation engine searches for predefined **Anchors** (block references or coordinates).
- Geometry is injected exactly where the anchors dictate, merging static template standards with dynamic parametric geometry seamlessly.
