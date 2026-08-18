# Release Notes — Mega Engineering Suite

## 🚀 Version 1.2.2 (August 2026)

### Key Features & Enhancements
* **Correlation ID & Structured Logging:**
  * Implemented `RunContext` generating unique `RUN-YYYYMMDD-HHMMSS-XXXX` identifiers for every user action.
  * Added categorized runtime logging (`Logs/Runtime`, `Logs/Errors`, `Logs/CAD`, `Logs/Generation`) writing directly to `%LOCALAPPDATA%`.
* **CAD COM Resilience & Crash Prevention:**
  * Resolved intermittent CLR Finalizer `0x800706BA` RPC Server Unavailable crashes in `GstarCadAdapter`.
  * Implemented single-pass ModelSpace entity discovery caching, reducing cold generation latency from up to 38s down to under 4s.
  * Added active Running Object Table (ROT) recovery and stale RCW eviction in `CadSessionManager`.
  * Implemented Regex-based handling for escaped curly braces (`\{\{`) in CAD MTEXT annotations.
* **Concurrency & UI Responsiveness:**
  * Hardened non-blocking generation serialization locks across all modules (`TubeSheet`, `BodyFlange`, `HeatExchanger`).
  * Added automatic UI cursor feedback (`WaitCursor`) during background CAD rendering.
* **Heat Exchanger Module:**
  * Fully integrated Heat Exchanger fabrication data mapping, calculations, and CAD template replacement.
* **Installer Hardening:**
  * Updated Inno Setup installer script to strictly respect Windows LocalAppData policies, eliminating `Program Files` write permission requirements.

---

## 📦 Version 1.2.1 (July 2026)
* Production release with decoupled configuration paths, title block attribute updates, and enhanced Bonnet Flange weight calculations.

## 📦 Version 1.2.0 (July 2026)
* Initial integrated release bundling Tube Sheet and Bonnet Flange engineering modules with interactive GstarCAD drafting automation.
