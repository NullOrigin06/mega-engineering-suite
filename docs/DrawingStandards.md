# Mega Engineering Suite - Drawing Standards

This document establishes the master drafting standards applied universally across all generated modules to ensure visual consistency and compliance with company drafting standards.

## Colors & Layering
Geometry color assignments override "ByLayer" when specifically required, but generally conform to the following schema:
- **Tube Holes (Inner)**: `Blue`
- **Finish OD / Flange ID / Shell ID Profiles**: `Blue`
- **Bolt PCD (Pitch Circle Diameter)**: `Red`
- **Bolt Holes**: `White`
- **Centerlines**: `Yellow`
- **Hatching (ANSI31)**: `White`
- **Dimensions & Text**: `White`

## Linetypes
- **Bolt PCD**: `PHANTOM`
- **Centerlines**: `CENTER`
- **Standard Profiles**: `Continuous`

## Annotations & Leaders
All generated textual callouts must adhere to:
- **Text Height**: `15.0` units.
- **Leader Style**: 2-Segment Polyline layout.
  - The leader originates near the targeted object (e.g., tube boundary).
  - Extends outward at an angle (usually 45 degrees or mathematically determined clearing the shell).
  - Finishes with a pure horizontal landing segment exactly `40.0` units long.
  - The Text label is placed `5.0` units away from the end of the landing segment.

## Row Count Standards
Row counts denote the number of tubes in a specific horizontal or vertical row.
- **Rear Tube Sheet**: Row-count leaders and labels are placed on the **RIGHT** side of the tube field.
- **Front Tube Sheet**: Row-count leaders and labels are placed on the **LEFT** side of the tube field (horizontally mirrored).

## Dimension Standards
Linear and radial dimensions generated programmatically must:
- Default to standard DIMSTYLE configurations (or explicitly override layer attributes).
- Use proper offset spacing (e.g., placing the thickness dimension `50.0` units below the side view profile).
