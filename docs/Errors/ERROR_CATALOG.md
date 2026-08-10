# Mega Engineering Suite Error Catalog

This catalog documents the standardized, machine-readable Error Codes used throughout the application. 

| Error Code | Category         | Description | Root Cause / History Link |
|------------|------------------|-------------|---------------------------|
| **CAD-001**| GstarCAD COM     | Failed to generate/communicate with GstarCAD COM layer. Usually means the application is not installed, not registered, or unresponsive. | [ERR-006: Intermittent generation problems](ERROR_006_CAD_INTERMITTENT_GENERATION.md) |
| **CAD-002**| GstarCAD COM     | Failed to open DWG file via COM. Usually indicates path resolution issues across process boundaries. | [ERR-001: Body Flange relative path CAD opening failure](ERROR_001_BODY_FLANGE_COM_RELATIVE_PATH.md) |
| **GEN-001**| Drawing Gen      | General generation failure in Body Flange module. | |
| **GEN-002**| Drawing Gen      | General generation failure in Heat Exchanger module. | |
| **PATH-001**| File System     | A required file (e.g. Template) was not found at the configured location. | |
| **EXC-001**| Export           | Failed to export to Excel (likely file lock or permission issue). | |
| **PLACEHOLDER-001**| Data Mapping| Heat Exchanger placeholder text was escaped by GstarCAD (e.g. `\{\{` instead of `{{`). | [ERR-002: Heat Exchanger placeholder escaped text](ERROR_002_HEAT_EXCHANGER_ESCAPED_TEXT.md) |
| **PLACEHOLDER-002**| Data Mapping| Heat Exchanger placeholder token mismatches or mapping logic failures. | [ERR-003: Heat Exchanger mapping mismatches](ERROR_003_HEAT_EXCHANGER_TOKEN_MISMATCH.md) |
| **DWG-001**| DWG Formatting   | Heat Exchanger engineering annotation formatting or overlapping text issues. | [ERR-004: Heat Exchanger formatting problems](ERROR_004_HEAT_EXCHANGER_FORMATTING.md) |
| **INSTALL-001**| Environment  | GstarCAD 2023/2026 CAD discovery or version compatibility issue. | [ERR-005: GstarCAD Version Compatibility](ERROR_005_CAD_DISCOVERY_COMPATIBILITY.md) |

*(Note: Click the links above to read the full historical evidence and root cause analysis for confirmed errors.)*
