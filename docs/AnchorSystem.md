# Mega Engineering Suite - Anchor System

The application utilizes an Anchor-based architecture to inject parametric geometry into pre-drafted templates. An "Anchor" is a named AutoCAD Block Reference or Text entity within the DWG template that serves as the absolute origin (`0,0,0`) for a specific generated view.

## Current Anchors

### 1. `REAR_TS_ANCHOR`
- **Purpose**: Defines the insertion point for the Rear Tube Sheet front-face view.
- **Coordinate Extraction**: The LISP generation engine queries the active drawing for a block reference named `REAR_TS_ANCHOR`. If found, its `InsertionPoint` (X, Y) is extracted.
- **Related Generation**: Triggers the translation of all Rear Tube Sheet geometry (Outer profile, Bolt holes, Tube holes, Row counts) to be centered at this coordinate.

### 2. `FRONT_TS_ANCHOR`
- **Purpose**: Defines the insertion point for the Front Tube Sheet front-face view.
- **Coordinate Extraction**: Queries for the block reference named `FRONT_TS_ANCHOR`.
- **Related Generation**: Aligns the Front Tube Sheet geometry relative to this extracted center point.

### 3. `REAR_SIDEVIEW_ANCHOR`
- **Purpose**: Defines the insertion point for the Rear Tube Sheet side-profile (thickness) section view.
- **Coordinate Extraction**: Queries for `REAR_SIDEVIEW_ANCHOR`.
- **Related Generation**: Centers the rectangular profile, ANSI31 section hatch, and thickness dimensions at this point.

### 4. `FRONT_SIDEVIEW_ANCHOR`
- **Purpose**: Defines the insertion point for the Front Tube Sheet side-profile (thickness) section view.
- **Coordinate Extraction**: Queries for `FRONT_SIDEVIEW_ANCHOR`.
- **Related Generation**: Centers the generated side view of the Front Tube Sheet at this location.

## Anchor Fallback
If an anchor is not detected in the template, the system defaults to predefined hardcoded offsets or an origin `(0,0)`, ensuring the drawing is still generated (albeit requiring manual movement by the draftsman).
