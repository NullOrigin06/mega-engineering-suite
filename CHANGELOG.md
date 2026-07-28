# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.2.1] - 2026-07-28
### Added
- Production-grade version-independent CAD Discovery Subsystem (`CadDiscoveryService`).
- Multi-tier dynamic discovery engine (Gstarsoft vendor registry keys, App Paths, shell commands, dynamic directory scanner).
- Native support for GstarCAD 2023, GstarCAD 2026, and future GstarCAD releases without hardcoded year numbers.
- Auto-updating `Settings.json` persistence for discovered CAD executable paths.

### Fixed
- Fixed CAD detection failure on GstarCAD 2023 systems.
- Fixed unquoted registry string parsing exception handling in shell open commands.

## [1.2.0] - 2026-07-15
### Added
- Interactive vs Automation modes for drawing pipeline (`PipelineExecutionMode`).
- Drawing lifecycle separation: interactive mode safely detaches the COM wrapper without closing GstarCAD.
- Pipeline optimizations and COM bottleneck resolutions (Stage 12).
- Caching for Title Block discovery.

### Changed
- Refactored `PipelineContext` out to its own domain model.
- Restructured COM release workflow to prevent orphaned processes.

## [1.1.0] - 2026-07-14
### Added
- Tube Sheet BOM Replacement pipeline.
- Title block parsing and synchronization.
- Hardcoded dimension values dynamically updated in templates.

## [1.0.0] - 2026-06-30
### Added
- Initial release of MEGA Engineering Suite.
- Front/Rear Tube Sheet module with AutoLISP integration.
- COM Automation engine for GstarCAD.
- Immutable template management system.
