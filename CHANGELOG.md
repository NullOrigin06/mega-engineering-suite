# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- Standardized documentation structure (`docs/Architecture`, `docs/UserGuide`).
- `.github` workflow templates for issues and pull requests.
- CI/CD workflow for automated .NET builds.
- Clean development branch `develop`.

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
