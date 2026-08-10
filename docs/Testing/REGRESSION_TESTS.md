# Master Regression Test Suite Specifications

This document defines the official regression test procedures required before committing changes to the MEGA Engineering Suite repository.

---

## Regression Test Inventory

### TEST-BF-01: Body Flange Path Resolution & Generation
- **Target**: Body Flange Module & `GstarCadAdapter.OpenDrawing()`
- **Procedure**:
  1. Set `BonnetOutputFolder` in `Settings.json` to a relative path (`"GeneratedDrawings"`).
  2. Instantiate `BonnetFlangeGenerator` and generate 5 consecutive drawings.
  3. Verify that all 5 DWG files are created in `GeneratedDrawings/` with size > 0 and open successfully in CAD without throwing `ArgumentException`.
- **Pass Criteria**: 5 / 5 runs pass (100% success).

---

### TEST-REL-01: COM Finalizer Stability & Single-Click Reliability
- **Target**: `GstarCadAdapter` Finalizer & GC Safety
- **Procedure**:
  1. Instantiate and release 30 `GstarCadAdapter` instances in rapid succession under forced GC:
     ```csharp
     GC.Collect();
     GC.WaitForPendingFinalizers();
     ```
  2. Trigger drawing generation immediately following garbage collection.
- **Pass Criteria**: 0 unhandled COM exceptions (`0x800706BA`), 0 CLR process crashes.

---

### TEST-CAD-01: CAD Disconnect & Stale RCW Recovery
- **Target**: `CadSessionManager` Singleton Eviction
- **Procedure**:
  1. Start GstarCAD and acquire session via `CadSessionManager.Instance.GetCadApplication()`.
  2. Kill `gcad.exe` process externally (`Stop-Process -Name gcad -Force`).
  3. Call `GetCadApplication()` and request drawing generation.
- **Pass Criteria**: Dead RCW is evicted, fresh CAD instance is spawned automatically, and drawing generation succeeds.

---

### TEST-ANN-01: Annotation & Escaped Brace Replacement
- **Target**: `GstarCadAdapter.ReplaceAnnotationPlaceholders()`
- **Procedure**:
  1. Load drawing with dimension text overrides containing `\{\{BFT\}\}` and `\{\{BFO\}\}`.
  2. Run `ReplaceAnnotationPlaceholders()`.
  3. Inspect output text and confirm 0 unreplaced `{{...}}` tokens remain.
- **Pass Criteria**: All placeholders replaced accurately.

---

### TEST-PERF-01: Single-Pass Performance Benchmark
- **Target**: Single-pass ModelSpace traversal and Title Block update
- **Procedure**:
  1. Execute 10 consecutive drawing generations on a warm CAD session.
  2. Measure `TitleBlockUpdateMs` and `TotalDurationMs`.
- **Pass Criteria**: `TitleBlockUpdateMs` < 20 ms (Avg < 12 ms), `TotalDurationMs` < 4.5 seconds.

---

### TEST-HE-01: Heat Exchanger Dynamic Placeholders
- **Target**: Heat Exchanger Fabrication Pipeline
- **Procedure**:
  1. Generate drawing for Shell ID = 914.
  2. Verify all 39 placeholders (including `{{TST}}`, `{{LT}}`, `{{LTH}}`, `{{BFO}}`, `{{BHC}}`) are formatted and replaced dynamically from calculations.
- **Pass Criteria**: Zero static overrides remain.
