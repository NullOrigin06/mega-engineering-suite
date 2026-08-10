# ERROR-008: Invisible CAD Generation (Close & Process.Start Launch Flaw)

| Metadata Field | Value |
| :--- | :--- |
| **Error ID** | `ERR-008` |
| **Date Identified** | 2026-08-10 |
| **Module** | CAD Lifecycle / Visibility Management |
| **Severity** | **Major (MAJ)** |
| **Status** | 🟢 **FIXED & VERIFIED** |

---

## 1. Symptoms

When users clicked **Generate Drawing**, generation completed in the background, but the resulting drawing window did not appear on screen. In other cases, a second CAD window launched with visual flicker.

---

## 2. Root Cause

1. **Destructive Close-and-Reopen Lifecycle**:
   - `GstarCadAdapter` was instantiated in headless mode (`_cadApp.Visible = false`).
   - After saving, `cadAdapter.Dispose()` closed the document (`_cadDoc.Close()`).
   - `Form3.cs` then executed `Process.Start("gcad.exe", outputPath)`.
   - Windows routed the open request to the existing hidden `gcad.exe` instance, keeping the drawing completely invisible to the user.

---

## 3. Fix Applied

1. **In-Session Activation Pattern**:
   - Added `KeepDocumentOpenOnDispose = true` to `ICadAdapter` and `GstarCadAdapter`.
   - Implemented `cadAdapter.ActivateAndShow()`, which sets `_cadApp.Visible = true;`, calls `_cadDoc.Activate();`, and brings the CAD window to the foreground via Win32 `SetForegroundWindow()`.
   - Removed redundant `Process.Start()` calls from UI handlers.

---

## 4. Prevention Rule

> [!TIP]
> **RULE**: Never close and re-launch generated CAD documents via `Process.Start()`. Maintain deterministic in-session document activation and make the active CAD window visible directly.
