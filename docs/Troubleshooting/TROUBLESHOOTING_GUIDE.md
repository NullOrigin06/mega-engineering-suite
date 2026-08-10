# Master Troubleshooting Guide

Welcome to the **MEGA Engineering Suite Master Troubleshooting Guide**. This guide provides step-by-step diagnostic workflows for resolving issues in CAD COM automation, drawing generation, placeholder replacement, and environment configuration.

---

## Quick Navigation

```text
Docs/
└── Troubleshooting/
    ├── TROUBLESHOOTING_GUIDE.md              (This file)
    ├── CAD_COM_TROUBLESHOOTING.md            (GstarCAD / COM Server / RPC issues)
    ├── DWG_GENERATION_TROUBLESHOOTING.md     (Drawing generation & file access)
    └── PLACEHOLDER_REPLACEMENT_TROUBLESHOOTING.md (Annotations, text, formatting)
```

---

## High-Frequency Issues & Quick Fixes

### 1. "can not access file GeneratedDrawings\..."
- **Symptoms**: `System.ArgumentException` at `Documents.Open()`.
- **Cause**: Relative path passed to out-of-process CAD COM server.
- **Reference**: [ERR-001](file:///C:/Users/PARTH/source/repos/MegaEngineeringSuite/Docs/Errors/ERROR_001_BODY_FLANGE_COM_RELATIVE_PATH.md).
- **Action**: Ensure paths are canonicalized with `Path.GetFullPath()`.

### 2. "The RPC server is unavailable (0x800706BA)"
- **Symptoms**: Silent crash on background thread or generation failure.
- **Cause**: COM calls on GC Finalizer thread or dead `_cadApp` RCW singleton.
- **Reference**: [ERR-002](file:///C:/Users/PARTH/source/repos/MegaEngineeringSuite/Docs/Errors/ERROR_002_CLR_FINALIZER_COM_RPC_CRASH.md) and [ERR-003](file:///C:/Users/PARTH/source/repos/MegaEngineeringSuite/Docs/Errors/ERROR_003_STALE_RCW_SESSION_EVICTION.md).
- **Action**: Verify `CadSessionManager` liveness probe and finalizer safety.

### 3. Placeholders Still Visible on Drawing
- **Symptoms**: Raw tags like `{{BFT}}` remain in the output DWG.
- **Cause**: Escaped brackets `\{\{...\}\}` in CAD dimension text or missing dictionary mapping.
- **Reference**: [ERR-004](file:///C:/Users/PARTH/source/repos/MegaEngineeringSuite/Docs/Errors/ERROR_004_ESCAPED_BRACE_CAD_ANNOTATION.md).
- **Action**: Check `GstarCadAdapter` regex escaping and run `TemplateAuditor.cs`.

### 4. Generation Latency > 10 Seconds
- **Symptoms**: UI remains busy for 10–38 seconds.
- **Cause**: Double ModelSpace traversal without attribute caching.
- **Reference**: [ERR-005](file:///C:/Users/PARTH/source/repos/MegaEngineeringSuite/Docs/Errors/ERROR_005_DOUBLE_MODELSPACE_SCAN_LATENCY.md).
- **Action**: Ensure single-pass caching is active in `ReplaceAnnotationPlaceholders()`.

---

## General Diagnostic Strategy

When investigating any new anomaly:

1. **Check Logs**: Inspect `Logs/` for timestamped execution traces from `SimpleLogger`.
2. **Isolate Environment**: Check whether GstarCAD is running in background tasks (`Task Manager -> gcad.exe`).
3. **Run Test Console**: Execute `dotnet run --project TestConsole\TestConsole.csproj` to run automated diagnostic and regression suites.
