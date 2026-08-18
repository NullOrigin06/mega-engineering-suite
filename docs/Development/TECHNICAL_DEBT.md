# Technical Debt & Architecture Refactoring Backlog

This document records identified technical debt and planned architectural enhancements for future versions of **Mega Engineering Suite**.

---

### 1. Unified CAD Abstraction Layer (`ICadAdapter`)
* **Current State:** `GstarCadAdapter` uses `dynamic` late-binding dispatch to communicate with both AutoCAD and GstarCAD COM interfaces.
* **Refactoring Plan:** Implement strongly-typed interface wrappers and compile-time COM interop assemblies to reduce reflection/dynamic dispatch overhead and improve compile-time type safety.

### 2. Async/Await Cancellation Token Propagation
* **Current State:** Drawing generation operations are executed asynchronously on background tasks (`Task.Run`), protected by a non-blocking `SemaphoreSlim` lock.
* **Refactoring Plan:** Thread `CancellationToken` through `ICadAdapter`, `PipelineOrchestrator`, and template discovery loops to allow clean cancellation of long-running generation jobs.

### 3. Native DXF/DWG Direct Binary Parser (Zero-CAD Headless Mode)
* **Current State:** All drawing modifications require an active desktop CAD session via COM.
* **Refactoring Plan:** Integrate a headless open-source CAD binary parser (e.g. `netDxf`) to allow offline DWG/DXF metadata extraction and batch PDF conversion without requiring GstarCAD to be installed.
