# CAD Document Lifecycle Standard

This document defines the official, standardized lifecycle for any drawing generation module in the Mega Engineering Suite (e.g., Bonnet Flange, Tube Sheet, Nozzle, Tank, Pipe Support).

## Requirements

Every CAD generation module MUST strictly satisfy the following criteria:

1. **Templates are immutable.**
   - Never modify files in the `Templates/` folder under any circumstances.
   
2. **Every generation starts with a copy.**
   - Before interacting with CAD, the module must copy the master template from `Templates/` to `GeneratedDrawings/`.
   - The copying process must ensure no read-only attributes are carried over from the master template to the output file.
   - If an output file already exists, it must be deleted before the copy.

3. **CAD only opens generated copies.**
   - The COM `OpenDrawing()` method must target the copied file in `GeneratedDrawings/`, never the master template.
   - The module must verify the active document path after opening to ensure CAD opened the correct file.

4. **Save() instead of SaveAs().**
   - Since the file being modified is already the output file, use `Save()` to commit the modifications in-place.

5. **Generated files only exist in `GeneratedDrawings/`.**
   - The `Templates/` folder must never contain `.dwl` or `.dwl2` lock files, ensuring portability and avoiding permission boundaries.

6. **No Administrator Required.**
   - By isolating all read/write and CAD lock behaviors to the `GeneratedDrawings/` folder (a standard user-owned directory), the application guarantees that Administrator privileges are never required to generate drawings.

## Lifecycle Diagram

```text
Master Template (Read Only)
        │
        ▼
Copy to GeneratedDrawings
        │
        ▼
Open Copied Drawing via COM
        │
        ▼
Perform geometry / placeholder updates
        │
        ▼
Save()
        │
        ▼
Close()
        │
        ▼
Show drawing
```
