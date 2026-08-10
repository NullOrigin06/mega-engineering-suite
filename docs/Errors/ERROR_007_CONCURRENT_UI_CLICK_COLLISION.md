# ERROR-007: Concurrent UI Click Collision and File Access Conflict

| Metadata Field | Value |
| :--- | :--- |
| **Error ID** | `ERR-007` |
| **Date Identified** | 2026-08-10 |
| **Module** | WinForms UI / Concurrency |
| **Severity** | **Major (MAJ)** |
| **Status** | 🟢 **FIXED & VERIFIED** |

---

## 1. Symptoms & Exact Exception

Rapidly clicking the **Generate Heat Exchanger** or **Generate Body Flange** button multiple times resulted in an application error dialog:

```text
System.IO.IOException: The process cannot access the file '...dwg' because it is being used by another process.
```

---

## 2. Root Cause

1. **Unprotected Event Handlers**:
   - The UI button click handlers were `async void` and did not disable the button or track ongoing background tasks.
   - A rapid double-click spawned two concurrent `Task.Run` executions that both attempted to write to the same timestamped file path simultaneously.

---

## 3. Fix Applied

1. **`Form3.cs`**:
   - Introduced a static `SemaphoreSlim(1, 1)` lock guard.
   - Disabled generation buttons immediately upon entry, changed cursor to `Cursors.WaitCursor`, and restored UI state deterministically inside a `finally` block:

```csharp
private static readonly SemaphoreSlim _generationLock = new SemaphoreSlim(1, 1);

if (!_generationLock.Wait(0))
{
    MessageBox.Show("Drawing generation is already in progress. Please wait.", "Generation In Progress", MessageBoxButtons.OK, MessageBoxIcon.Information);
    return;
}

try
{
    Cursor = Cursors.WaitCursor;
    btnGenerateHeatExchanger.Enabled = false;
    // Execute generation...
}
finally
{
    btnGenerateHeatExchanger.Enabled = true;
    Cursor = Cursors.Default;
    _generationLock.Release();
}
```

---

## 4. Prevention Rule

> [!IMPORTANT]
> **RULE**: All asynchronous background tasks initiated by UI events must enforce immediate button disabling, a busy cursor state, and a non-blocking re-entrancy lock (`Wait(0)`).
