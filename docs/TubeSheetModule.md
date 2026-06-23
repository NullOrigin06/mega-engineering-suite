# Mega Engineering Suite - TubeSheet Module

## Overview
The TubeSheet module is responsible for the complete parametric generation of Heat Exchanger Tube Sheets. It supports both single-view and multi-view (template) generation modes for both the Rear and Front assemblies.

## Generation Phases

### Front Face Generation
1. **Outer Profiles**: Finish OD, Flange ID, and Shell ID are generated as standard concentric `CadCircle` entities.
2. **Centerlines**: Main vertical and horizontal crosshairs extending beyond the Finish OD.
3. **Bolt Circle Generation**: Computes the Bolt Pitch Circle Diameter (PCD) as a Phantom red circle.
4. **Bolt Hole Generation**: Equally distributes the specified number of bolt holes mathematically around the PCD.
5. **Tube Layout Generation**:
   - Parses triangular or square pitch arrays.
   - Computes intersection points.
   - Bounding logic trims tubes falling outside the Flange ID limit or within Pass Partition lanes.
   - Uses `Blue` for inner tube holes and `White` for outer tube boundaries.

### Side View Generation
1. **Profile Generation**: Draws the rectangular side profile using the Finish OD as the height and the specified `Thickness` parameter as the width.
2. **Hatching**: Injects an `ANSI31` pattern inside the profile to indicate a cross-section.
3. **Dimensioning**: Programmatically generates a linear dimension denoting the thickness, placed below the geometry.

### Callouts and Annotations
1. **Row Count Logic**: 
   - Scans the generated tube array to count tubes in discrete Y-axis rows.
   - Generates horizontal leaders extending outward.
   - Places text labels denoting the count.
2. **Anchor Placement**: If in Template Mode, coordinates are mathematically translated from origin `(0,0)` to the respective Anchor coordinates (`REAR_TS_ANCHOR`, `FRONT_TS_ANCHOR`, etc.).

## Current Limitations
- Complex multi-pass partition layouts (e.g., 6-pass or 8-pass with angled ribbons) are mathematically approximated and may require manual trimming.
- Non-standard U-tube layouts require manual post-processing.
