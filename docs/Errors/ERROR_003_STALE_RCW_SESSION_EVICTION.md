# ERROR-003: Stale RCW Session Singleton Crash After CAD Exit

| Metadata Field | Value |
| :--- | :--- |
| **Error ID** | `ERR-003` |
| **Date Identified** | 2026-08-10 |
| **Module** | CAD Session Manager |
| **Severity** | **Major (MAJ)** |
| **Status** | 🟢 **FIXED & VERIFIED** |

---

## 1. Symptoms & Exact Exception

If a user closed GstarCAD while the MEGA Engineering Suite was running and subsequently attempted to generate another drawing, the application failed immediately:

```text
System.Runtime.InteropServices.COMException (0x800706BA): The RPC server is unavailable. (0x800706BA)
   at System.RuntimeType.ForwardCallToInvokeMember(...)
   at MegaEngineeringSuite.Infrastructure.Cad.CadSessionManager.GetCadApplication()
```

---

## 2. Root Cause

1. **Dead Pointer Retention in Singleton**:
   - `CadSessionManager.Instance` maintained a singleton reference `_cadApp`.
   - When the user closed GstarCAD, the OS process `gcad.exe` terminated, but the .NET CLR still held an un-evicted Runtime Callable Wrapper (RCW).
   - On the next generation request, `GetCadApplication()` returned the dead RCW without probing for server liveness, causing immediate RPC failure upon property access.

---

## 3. Forensic Evidence & Trial Results

- In Test Set 4 (CAD closed midway), 5 cycles of manually killing `gcad.exe` between runs resulted in 100% clean detection, stale RCW eviction, fresh GstarCAD launch, and successful drawing completion across all 10 runs.

---

## 4. Fix Applied

1. **`CadSessionManager.cs`**:
   - Added thread-safe locking (`lock (_lock)`).
   - Implemented an active liveness probe (`_cadApp.Name`).
   - If an RPC exception (`0x800706BA`) is thrown, the stale RCW is released and a fresh COM session is reacquired automatically:

```csharp
public dynamic GetCadApplication()
{
    lock (_lock)
    {
        if (_cadApp != null)
        {
            try
            {
                // Liveness probe
                string name = _cadApp.Name;
                return _cadApp;
            }
            catch (COMException ex) when ((uint)ex.ErrorCode == 0x800706BA)
            {
                SimpleLogger.Log("CadSessionManager", "Stale CAD RCW detected (0x800706BA). Evicting reference...");
                try { Marshal.FinalReleaseComObject(_cadApp); } catch { }
                _cadApp = null;
            }
            catch (Exception ex)
            {
                SimpleLogger.Log("CadSessionManager", $"CAD session check failed: {ex.Message}. Reacquiring...");
                _cadApp = null;
            }
        }

        _cadApp = StartNewCadInstance();
        return _cadApp;
    }
}
```

---

## 5. Prevention Rule

> [!IMPORTANT]
> **RULE**: Singleton managers for out-of-process COM servers must never assume a cached pointer remains valid over the application lifetime. An active liveness probe with automatic stale RCW eviction must precede the return of cached pointers.
