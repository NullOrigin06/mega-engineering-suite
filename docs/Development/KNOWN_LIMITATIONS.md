# Known Limitations

This document tracks current known technical constraints and environment requirements in **Mega Engineering Suite** v1.2.2.

---

### 1. CAD Application Compatibility
* **Supported CAD Platforms:** GstarCAD 2023, GstarCAD 2026, AutoCAD 2021–2026 (via COM automation).
* **Limitation:** CAD application must be installed and activated locally on Windows. Web-based CAD or headless Linux environments are not supported due to native Windows COM dependence.

### 2. File Locking on Active DWG Documents
* **Behavior:** If a user has a generated DWG file opened in exclusive write mode in an external CAD editor, re-generating the same drawing without closing or saving in CAD may trigger a file access lock exception (`EXC-001` or `CAD-002`).
* **Workaround:** The application provides a structured error prompt suggesting closing the active drawing in GstarCAD or specifying a new filename.

### 3. High-DPI Scaling on Legacy WinForms Controls
* **Behavior:** On 4K monitors with >150% DPI scaling, certain static group box borders or DataGridView cells in legacy dialogs may experience minor visual padding offsets.
* **Workaround:** The application specifies Per-Monitor V2 DPI awareness in its app manifest.

### 4. Excel Format Requirements for Heat Exchanger BOM
* **Behavior:** The Excel BOM data importer requires standard `.xlsx` formats conforming to the documented column structure. Corrupted, password-protected, or legacy binary `.xls` files will be rejected with structured validation errors.
