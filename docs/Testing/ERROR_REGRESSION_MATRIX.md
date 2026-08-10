# Regression Test Matrix

Every confirmed historical error must have a documented regression test to ensure it is not reintroduced.

## Test Procedures

### RT-CAD-002: Body Flange Absolute Path Check
**Error Reverted:** ERR-001 (Body Flange relative path CAD opening failure)
**Procedure:**
1. Generate Body Flange.
2. Verify that GstarCAD opens the generated DWG properly.
3. Validate that the UI displays `Generated : Body Flange` without crashing.

### RT-PLACEHOLDER-001: Escaped Brace Resiliency
**Error Reverted:** ERR-002 (Heat Exchanger escaped placeholder text `\{\{`)
**Procedure:**
1. Open Heat Exchanger template in GstarCAD and intentionally save it with MTEXT formatting that escapes braces.
2. Run Heat Exchanger generation.
3. Ensure the generator correctly identifies and replaces `\{\{TST}}` or similar formatted tags without leaving artifacts.

### RT-PLACEHOLDER-002: Token Mapping Integrity
**Error Reverted:** ERR-003 (Heat Exchanger mapping mismatches)
**Procedure:**
1. Populate specific parameters (e.g., Baffle Thickness = 4.5).
2. Generate Heat Exchanger.
3. Verify that `{{BAFFLE_THK}}` maps exactly to `4.5` and not a truncated/default value.

### RT-DWG-001: Annotation Formatting & Spacing
**Error Reverted:** ERR-004 (Heat Exchanger annotation formatting problems)
**Procedure:**
1. Inspect the generated Heat Exchanger DWG.
2. Verify that annotations do not overlap block geometry or table borders.

### RT-INSTALL-001: CAD Discovery
**Error Reverted:** ERR-005 (GstarCAD version compatibility)
**Procedure:**
1. Run application on a machine with *only* GstarCAD 2023, then *only* 2026.
2. Application must detect the executable path dynamically and populate `Settings.json` correctly.

### RT-CAD-001: Intermittent Generation Stress Test
**Error Reverted:** ERR-006 (Intermittent generation/opening problems)
**Procedure:**
Perform 5 consecutive Heat Exchanger generations without closing the application.
**Record Data:**
| Run ID | Module | Start Time | Duration | DWG Created | DWG Opened | CAD Version | Result | Error Code |
|--------|--------|------------|----------|-------------|------------|-------------|--------|------------|
|        |        |            |          |             |            |             |        |            |
