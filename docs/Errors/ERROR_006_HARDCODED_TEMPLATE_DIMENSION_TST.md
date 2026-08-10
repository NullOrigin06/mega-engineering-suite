# ERROR-006: Hardcoded Static Dimension in Master DWG Template

| Metadata Field | Value |
| :--- | :--- |
| **Error ID** | `ERR-006` |
| **Date Identified** | 2026-08-09 |
| **Module** | Heat Exchanger Fabrication / Template Drafting |
| **Severity** | **Minor (MIN)** |
| **Status** | 🟢 **FIXED & VERIFIED** |

---

## 1. Symptoms

In the generated Heat Exchanger Fabrication drawing, the horizontal dimension beneath the right-side Tube Sheet cross-section always displayed the static value **"25"**, regardless of the actual Tube Sheet Finish Thickness calculated in the engineering data model.

---

## 2. Root Cause

1. **Static Template Override**:
   - The master DWG template `Heat_Exchanger_Fabrication_template.dwg` had a static dimension override string `"25"` rather than a dynamic placeholder token.
   - Because no placeholder existed at that entity, the replacement engine skipped it.

---

## 3. Fix Applied

1. **Schema & Mapper Update**:
   - Introduced official placeholder `{{TST}}` (Tube Sheet Thickness).
   - Placed `{{TST}}` in the master DWG template text override.
   - Mapped `{{TST}}` in `HeatExchangerFabData` and `HeatExchangerFabFormatter.cs`:
     ```csharp
     dict["{{TST}}"] = data.TubeSheetThickness > 0 ? $"{data.TubeSheetThickness:F0}" : "25";
     ```

---

## 4. Prevention Rule

> [!IMPORTANT]
> **RULE**: All engineering values in template drawings must be verified with `TemplateAuditor.cs` to ensure no hardcoded numeric overrides exist outside the registered placeholder schema.
