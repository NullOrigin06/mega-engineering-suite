# Master Error & Failure Register

This document is the official, permanent **Error & Failure Register** for the **MEGA Engineering Suite**. Every documented runtime failure, architectural flaw, or regression identified during development and testing is formally tracked, analyzed, and linked to its dedicated root-cause and prevention specification.

---

## Error Register Index

| Error ID | Module | Failure Symptom | Confirmed Root Cause | Resolution / Fix | Status | Regression Test |
| :--- | :--- | :--- | :--- | :--- | :---: | :--- |
| [**ERR-001**](file:///C:/Users/PARTH/source/repos/MegaEngineeringSuite/Docs/Errors/ERROR_001_BODY_FLANGE_COM_RELATIVE_PATH.md) | Body Flange | `ArgumentException: can not access file` at `Documents.Open()` | Relative path passed across out-of-process COM boundary | Canonicalize paths with `Path.GetFullPath()`; normalize config paths | 🟢 FIXED | [TEST-BF-01](file:///C:/Users/PARTH/source/repos/MegaEngineeringSuite/Docs/Testing/REGRESSION_TESTS.md#test-bf-01) |
| [**ERR-002**](file:///C:/Users/PARTH/source/repos/MegaEngineeringSuite/Docs/Errors/ERROR_002_CLR_FINALIZER_COM_RPC_CRASH.md) | CAD Infrastructure | Intermittent crash (`0x800706BA: RPC server unavailable`) | C# Finalizer `~GstarCadAdapter()` invoking COM methods across threads | Finalizer only releases RCWs; no remote COM calls in GC finalizers | 🟢 FIXED | [TEST-REL-01](file:///C:/Users/PARTH/source/repos/MegaEngineeringSuite/Docs/Testing/REGRESSION_TESTS.md#test-rel-01) |
| [**ERR-003**](file:///C:/Users/PARTH/source/repos/MegaEngineeringSuite/Docs/Errors/ERROR_003_STALE_RCW_SESSION_EVICTION.md) | CAD Infrastructure | Crash on subsequent runs after user closes GstarCAD | Stale RCW singleton retained in `CadSessionManager._cadApp` | Active liveness probe with automatic stale RCW eviction & reconnect | 🟢 FIXED | [TEST-CAD-01](file:///C:/Users/PARTH/source/repos/MegaEngineeringSuite/Docs/Testing/REGRESSION_TESTS.md#test-cad-01) |
| [**ERR-004**](file:///C:/Users/PARTH/source/repos/MegaEngineeringSuite/Docs/Errors/ERROR_004_ESCAPED_BRACE_CAD_ANNOTATION.md) | Annotation Engine | Placeholders (`{{...}}`) not replaced in Dimension/MText | CAD engine formats dimension text with escaped braces (`\{\{...\}\}`) | Multi-pattern regex & escaped-brace matching in text replacement | 🟢 FIXED | [TEST-ANN-01](file:///C:/Users/PARTH/source/repos/MegaEngineeringSuite/Docs/Testing/REGRESSION_TESTS.md#test-ann-01) |
| [**ERR-005**](file:///C:/Users/PARTH/source/repos/MegaEngineeringSuite/Docs/Errors/ERROR_005_DOUBLE_MODELSPACE_SCAN_LATENCY.md) | Performance | Generation latency 10s–38s on warm sessions (1,870 COM calls) | Sequential double ModelSpace scan (Pass 1: Annotations, Pass 2: Title Block) | Unified single-pass ModelSpace traversal with attribute caching (<10ms) | 🟢 FIXED | [TEST-PERF-01](file:///C:/Users/PARTH/source/repos/MegaEngineeringSuite/Docs/Testing/REGRESSION_TESTS.md#test-perf-01) |
| [**ERR-006**](file:///C:/Users/PARTH/source/repos/MegaEngineeringSuite/Docs/Errors/ERROR_006_HARDCODED_TEMPLATE_DIMENSION_TST.md) | Heat Exchanger | Static dimension "25" remaining on DWG output | Hardcoded static text in master DWG template cross-section | Introduced dynamic placeholder `{{TST}}` in schema, mapper & formatter | 🟢 FIXED | [TEST-HE-01](file:///C:/Users/PARTH/source/repos/MegaEngineeringSuite/Docs/Testing/REGRESSION_TESTS.md#test-he-01) |
| [**ERR-007**](file:///C:/Users/PARTH/source/repos/MegaEngineeringSuite/Docs/Errors/ERROR_007_CONCURRENT_UI_CLICK_COLLISION.md) | UI / Workflow | Rapid double-clicking causes `IOException` file collisions | Missing UI event re-entrancy protection and lock guards | UI `SemaphoreSlim(1,1)` lock, immediate button disable & `WaitCursor` | 🟢 FIXED | [TEST-UI-01](file:///C:/Users/PARTH/source/repos/MegaEngineeringSuite/Docs/Testing/REGRESSION_TESTS.md#test-ui-01) |
| [**ERR-008**](file:///C:/Users/PARTH/source/repos/MegaEngineeringSuite/Docs/Errors/ERROR_008_INVISIBLE_CAD_PROCESS_REOPENING.md) | CAD Lifecycle | Drawing not visible on screen after generation | Background CAD instance kept invisible; close ➔ `Process.Start` flaw | In-session activation (`KeepDocumentOpenOnDispose`, `ActivateAndShow`) | 🟢 FIXED | [TEST-VIS-01](file:///C:/Users/PARTH/source/repos/MegaEngineeringSuite/Docs/Testing/REGRESSION_TESTS.md#test-vis-01) |

---

## Error Classification & Severity Standards

1. **Critical (CRIT)**: Application crash, CLR process termination, or unhandled COM exception causing data loss.
2. **Major (MAJ)**: Generation failure, missing drawing output, or unhandled I/O error preventing drawing generation.
3. **Moderate (MOD)**: Incorrect annotation formatting, unreplaced placeholders, or UI freeze > 5 seconds.
4. **Minor (MIN)**: Visual artifact, drawing window focus issue, or non-blocking configuration warning.

---

## Mandatory Engineering Rules for Error Prevention

1. **Absolute Paths Across Process Boundaries**:
   - Any path passed to external COM APIs (`Documents.Open()`), Shell executions, or background workers **MUST** be converted to an absolute path via `Path.GetFullPath()`.
   - Never assume the external process shares the CLR host's current working directory.

2. **COM Finalizer Safety Contract**:
   - .NET finalizers (`~ClassName()`) must **NEVER** call remote COM server methods (such as `_cadDoc.Close()`).
   - Finalizers may only safely release local Runtime Callable Wrappers (`Marshal.FinalReleaseComObject`).
   - Deterministic resource teardown is exclusively the responsibility of `IDisposable.Dispose(true)`.

3. **Singleton Liveness Validation**:
   - Cached out-of-process COM pointers (`_cadApp`) must be tested for liveness (`_cadApp.Name`) prior to use.
   - If an RPC disconnect (`0x800706BA`) is encountered, the stale RCW must be evicted immediately and cleanly reacquired.

4. **Single-Pass COM Collections Traversal**:
   - Never iterate ModelSpace or Layout block entity collections multiple times.
   - Combine entity scanning, text replacement, and block attribute caching into a single unified pass.
