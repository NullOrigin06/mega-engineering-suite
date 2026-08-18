# Heat Exchanger UI Data Flow & Architecture

This document formalizes the active-value pipeline, parameter visibility, and override model for the **Heat Exchanger** module in Mega Engineering Suite.

---

## 1. Core Architectural Concept

```text
Excel Template (Read-Only Reference)
      ↓ (Initial lookup on Calculate)
Default Engineering Values
      ↓
Active UI Layer (Form3: DataGridViews)
      │
      ├── [Engineering Parameters] (11 Visible & Editable)
      ├── [Extras] (3 Editable: FS Length, RS Length, Dishend THK)
      └── [Hidden Internals] (8 Properties Preserved in Memory)
      ↓ (Snapshot on Generation/Export)
Active Runtime Generation Data (HeatExchangerFabData)
      ↓
CAD Generator (HeatExchangerFabGenerator)
      ↓
DWG Output
```

> [!IMPORTANT]
> **Excel is strictly Read-Only.** UI edits are runtime overrides for the current generation session. The application never writes back or alters the Excel workbook.

---

## 2. Parameter Visibility Breakdown

### A. Visible & Editable Engineering Parameters (11)
1. **Shell I.D.** (Updates Calculated Summary dynamically upon edit)
2. **Tube Sheet Finish THK**
3. **Body Flange Finish THK**
4. **Partition Plate THK**
5. **Baffle THK**
6. **Bolt Size**
7. **Bolt Length**
8. **No Of Bolts**
9. **Flange I.D.**
10. **Tube Sheet Finish O.D.**
11. **Tie Rod Qty.**

### B. New Editable EXTRAS Section (3)
1. **Bonnet Shell FS Length** (User Input, defaults to 500 mm)
2. **Bonnet Shell RS Length** (User Input, defaults to 500 mm)
3. **Dishend THK** (Loaded initially from Excel Col 6 `Dishend Thk`, fully editable)

### C. Hidden Internal Parameters (8)
*These parameters are hidden from the visual UI to prevent clutter, but are 100% preserved in `EngineeringDataModel` and `HeatExchangerFabData` for CAD bolt-hole generation and drafting layout:*
1. **Hole Dia.**
2. **Bolt P.C.D.**
3. **Liner / Gasket O.D.**
4. **Tie Rod Dia.**
5. **Spacer Tube**
6. **Tube Sheet Raw O.D.**
7. **Tube Sheet Raw THK**
8. **Body Flange Raw THK**

---

## 3. UI Synchronization & Active Value Snapshot

* **Cell Editing:** Parameter names are protected (`ReadOnly = true`), while values are directly editable.
* **Auto-Sync:** Changes trigger `SyncUiToDataModel()` on `CellEndEdit` as well as immediately before generation (`BtnGenerateHeatExchanger_Click`, `BtnGenerateTubeSheet_Click`, `BtnGenerateBodyFlange_Click`, `BtnExport_Click`).
* **Isolation:** The Tube Sheet and Body Flange modules remain completely functional and consume active values without behavioral regression.
