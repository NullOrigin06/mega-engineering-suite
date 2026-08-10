# ERROR-002: Intermittent Single-Click CLR Finalizer RPC Crash (0x800706BA)

| Metadata Field | Value |
| :--- | :--- |
| **Error ID** | `ERR-002` |
| **Date Identified** | 2026-08-10 |
| **Module** | CAD Infrastructure / Memory Management |
| **Severity** | **Critical (CRIT)** |
| **Status** | 🟢 **FIXED & VERIFIED** |

---

## 1. Symptoms & Exact Exception

During drawing generation, the application intermittently crashed with an unhandled COM exception or terminated silently without showing the drawing:

```text
Unhandled exception. System.Runtime.InteropServices.COMException (0x800706BA): The RPC server is unavailable. (0x800706BA)
   at Microsoft.CSharp.RuntimeBinder.ComInterop.ComRuntimeHelpers.CheckThrowException(Int32 hresult, ExcepInfo& excepInfo, UInt32 argErr, String message)
   at CallSite.Target(Closure, CallSite, ComObject, Boolean)
   at MegaEngineeringSuite.Infrastructure.Cad.GstarCadAdapter.CloseDrawing() in GstarCadAdapter.cs:line 573
   at MegaEngineeringSuite.Infrastructure.Cad.GstarCadAdapter.Dispose(Boolean disposing) in GstarCadAdapter.cs:line 588
   at MegaEngineeringSuite.Infrastructure.Cad.GstarCadAdapter.Finalize() in GstarCadAdapter.cs:line 598
   at System.GC.RunFinalizers()
```

---

## 2. Root Cause

1. **Destructor / Finalizer COM Method Invocation**:
   - `GstarCadAdapter` implemented a C# finalizer: `~GstarCadAdapter() => Dispose(disposing: false)`.
   - `Dispose(false)` called `CloseDrawing() => _cadDoc.Close(false)`.
   - The .NET Garbage Collector runs finalizers on a dedicated background MTA Finalizer thread (`GC.RunFinalizers()`).
   - If the user or application had already closed the CAD session or disconnected the COM server, invoking `_cadDoc.Close(false)` on a disconnected COM proxy threw `COMException (0x800706BA)`.
   - Because exceptions on the CLR Finalizer thread cannot be caught by application-level `try/catch` handlers, the .NET runtime terminated the entire process immediately.

---

## 3. Forensic Evidence

- **Reproduction**: Reproduced in controlled 40-run tests (`S1-Cold-05` and `S1-Cold-06`) under GC pressure.
- **Verification**: Following fix implementation, 30 consecutive single-click trials under heavy GC stress passed with **100% success (0 exceptions, 0 crashes)**.

---

## 4. Fix Applied

1. **`GstarCadAdapter.cs`**:
   - Strictly separated managed disposal (`disposing == true`) from finalizer invocation (`disposing == false`).
   - Finalizer now **ONLY** releases the local RCW pointer (`Marshal.FinalReleaseComObject`), never calling remote COM server methods.
   - Wrapped `CloseDrawing()` in a defensive `try / catch (COMException)` block with structured logging.

```csharp
protected virtual void Dispose(bool disposing)
{
    if (!_disposedValue)
    {
        if (disposing)
        {
            // Explicit managed disposal: Deterministically close drawing and release COM references
            if (!KeepDocumentOpenOnDispose)
            {
                CloseDrawing();
            }
            else
            {
                ReleaseDocumentReference();
            }
        }
        else
        {
            // Finalizer invocation: NEVER perform remote COM calls on the CLR Finalizer thread!
            ReleaseDocumentReference();
        }
        _disposedValue = true;
    }
}
```

---

## 5. Prevention Rule

> [!CAUTION]
> **RULE**: Never execute remote COM methods (`Close()`, `Save()`, `Quit()`, `SetVariable()`) inside a C# finalizer (`~ClassName()`). Finalizers must strictly perform non-remote RCW dereferencing. Deterministic cleanup must be handled via `IDisposable`.
