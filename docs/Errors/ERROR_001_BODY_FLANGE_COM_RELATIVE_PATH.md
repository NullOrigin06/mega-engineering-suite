# ERROR-001: Body Flange Out-of-Process COM Relative Path Access Failure

| Metadata Field | Value |
| :--- | :--- |
| **Error ID** | `ERR-001` |
| **Date Identified** | 2026-08-10 |
| **Module** | Body Flange Generator / CAD Infrastructure |
| **Severity** | **Major (MAJ)** |
| **Status** | 🟢 **FIXED & VERIFIED** |

---

## 1. Symptoms & Exact Exception

When the user clicked **Generate Body Flange**, the application copied the template successfully but threw an exception during `OpenDrawing`:

```text
System.ArgumentException: can not access file GeneratedDrawings\BF_925_812_20260810_101309.dwg
   at Microsoft.CSharp.RuntimeBinder.ComInterop.ComRuntimeHelpers.CheckThrowException(Int32 hresult, ExcepInfo& excepInfo, UInt32 argErr, String message)
   at CallSite.Target(Closure, CallSite, ComObject, String)
   at MegaEngineeringSuite.Infrastructure.Cad.GstarCadAdapter.OpenDrawing(String filePath) in GstarCadAdapter.cs:line 61
   at MegaEngineeringSuite.BonnetFlange.BonnetFlangeGenerator.Generate(BonnetFlangeData data, DrawingInformation drawInfo) in BonnetFlangeGenerator.cs:line 81
```

---

## 2. Execution Flow

```text
Form3.BtnGenerateBodyFlange_Click()
    ↓
BonnetFlangeGenerator.Generate()
    ↓
outputPath = Path.Combine("GeneratedDrawings", "BF_925_812_...dwg")  [Relative Path]
    ↓
File.Copy(templatePath, outputPath)  [.NET resolves relative to App Directory -> SUCCESS]
    ↓
cadAdapter.OpenDrawing(outputPath)
    ↓
_cadApp.Documents.Open("GeneratedDrawings\BF_...")  [COM IPC to gcad.exe]
    ↓
GstarCAD (PID 13232) evaluates path relative to C:\Program Files\Gstarsoft\GstarCAD2026\
    ↓
File not found in CAD install directory -> System.ArgumentException (0x80070057)
```

---

## 3. Root Cause

1. **Working Directory Boundary Mismatch**:
   - `Settings.json` stored `"BonnetOutputFolder": "GeneratedDrawings"` (a relative path).
   - In .NET, `File.Copy` resolved the relative path against `Environment.CurrentDirectory` (`C:\Users\PARTH\source\repos\MegaEngineeringSuite`).
   - When passed across the out-of-process COM IPC boundary, `_cadApp.Documents.Open()` was executed by `gcad.exe`, whose working directory was `C:\Program Files\Gstarsoft\GstarCAD2026\`.
   - GstarCAD attempted to open `C:\Program Files\Gstarsoft\GstarCAD2026\GeneratedDrawings\BF_...`, which does not exist.

---

## 4. Forensic Evidence & Trial Results

| Trial Configuration | Runs | Pass | Fail | Result |
| :--- | :---: | :---: | :---: | :--- |
| Relative Path (`"GeneratedDrawings\..."`) | 5 | 0 | 5 | 🔴 **100% Failed** (`ArgumentException: can not access file`) |
| Absolute Path (`"C:\...\GeneratedDrawings\..."`) | 5 | 5 | 0 | 🟢 **100% Succeeded** (`Open SUCCESS in ~900-1400ms`) |

### Ruled Out:
- ❌ **File Lock**: Exclusive `FileStream` test passed with `FileShare.None`.
- ❌ **Incomplete Copy**: Copy completed 5.3s before open.
- ❌ **CAD Lock Files**: Zero `.dwl` / `.dwl2` files existed.
- ❌ **DWG Corruption**: Template SHA256 was intact.

---

## 5. Fix Applied

1. **`GstarCadAdapter.cs`**:
   Ensured incoming paths are converted to absolute paths and existence is verified before calling COM:
   ```csharp
   string fullPath = Path.GetFullPath(filePath);
   if (!File.Exists(fullPath))
   {
       throw new FileNotFoundException($"DWG file does not exist: {fullPath}", fullPath);
   }
   _cadDoc = _cadApp.Documents.Open(fullPath);
   ```

2. **`AppConfigManager.cs`**:
   Added programmatic path normalization against `_rootFolder` so that portable relative paths in `Settings.json` are dynamically converted to absolute paths in memory:
   ```csharp
   public static string NormalizePath(string? path, string defaultRelative)
   {
       if (string.IsNullOrWhiteSpace(path)) return Path.Combine(_rootFolder, defaultRelative);
       if (Path.IsPathRooted(path)) return path;
       return Path.GetFullPath(Path.Combine(_rootFolder, path));
   }
   ```

---

## 6. Files Modified & Preserved

- **Modified**:
  - `MegaEngineeringSuite\Infrastructure\Cad\GstarCadAdapter.cs`
  - `MegaEngineeringSuite\AppConfigManager.cs`
- **Preserved (Untouched)**:
  - Body Flange engineering calculations, placeholder schemas, and geometry.
  - Tube Sheet & Heat Exchanger modules.

---

## 7. Prevention & Validation Rule

> [!IMPORTANT]
> **RULE**: All file paths passed to external out-of-process COM APIs or operating system processes **MUST** be canonicalized using `Path.GetFullPath()`. Never assume external processes share the application working directory.
