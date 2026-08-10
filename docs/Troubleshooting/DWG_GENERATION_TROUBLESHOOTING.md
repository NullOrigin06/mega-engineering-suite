# DWG Generation & File Lifecycle Troubleshooting Guide

This guide covers troubleshooting procedures for DWG template copying, drawing file locks, path resolution, and drawing lifecycle management.

---

## 1. The Standard Drawing Lifecycle

All drawing generation modules (Tube Sheet, Body Flange, Heat Exchanger Fabrication) follow the **Immutable Template Lifecycle Standard**:

```text
Templates/*.dwg (Immutable Master)
       │
       ▼ (1. File.Copy to user-accessible folder)
GeneratedDrawings/*.dwg (Working Copy)
       │
       ▼ (2. COM Open Copied File via Absolute Path)
_cadApp.Documents.Open(fullPath)
       │
       ▼ (3. Modify Entities & Update Title Block in Single Pass)
GstarCadAdapter.ReplaceAnnotationPlaceholders() & UpdateTitleBlockAttributes()
       │
       ▼ (4. Save In-Place)
cadDoc.Save()
       │
       ▼ (5. Activate Drawing & Bring CAD to Foreground)
cadAdapter.ActivateAndShow()
```

---

## 2. Common Drawing File Errors & Solutions

### Issue 1: `ArgumentException: can not access file ...`
- **Cause**: The path passed to `_cadApp.Documents.Open()` was a relative path.
- **Solution**: Always use `Path.GetFullPath(filePath)` before passing file paths across COM boundaries.

---

### Issue 2: `IOException: The process cannot access the file ... because it is being used by another process`
- **Causes**:
  1. A previous generation crashed without releasing the file handle or CAD document lock.
  2. GstarCAD is currently holding the file open with an active `.dwl` lock.
  3. A background file stream was not properly wrapped in a `using` statement.
- **Diagnostics**:
  1. Check if the file is currently open in GstarCAD.
  2. Test file access programmatically:
     ```csharp
     using (var fs = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None)) { }
     ```
  3. Delete stale lock files (`.dwl`, `.dwl2`) if GstarCAD is no longer running.

---

### Issue 3: Template File Modified Accidentally
- **Prevention**: Master templates in `Templates/` should be marked as Read-Only. The application must always copy the template to `GeneratedDrawings/` before opening.
