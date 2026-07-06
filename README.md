<div align="center">

# MEGA Engineering Suite

**Professional CAD Automation Software**

Generate production-ready engineering drawings directly from engineering parameters using C#, AutoLISP, and GstarCAD automation.

<br>

[![C#](https://img.shields.io/badge/C%23-239120?style=flat-square&logo=c-sharp&logoColor=white)](#)
[![.NET](https://img.shields.io/badge/.NET-512BD4?style=flat-square&logo=dotnet&logoColor=white)](#)
[![Windows](https://img.shields.io/badge/Windows-0078D6?style=flat-square&logo=windows&logoColor=white)](#)
[![GstarCAD](https://img.shields.io/badge/GstarCAD-004481?style=flat-square&logo=autocad&logoColor=white)](#)
[![AutoLISP](https://img.shields.io/badge/AutoLISP-333333?style=flat-square&logo=gnu-bash&logoColor=white)](#)
[![Build Status](https://img.shields.io/badge/Build-Passing-brightgreen.svg?style=flat-square)](#)
[![Version](https://img.shields.io/badge/Version-1.0.3-blue.svg?style=flat-square)](#)
[![License: MEGA EPC PVT LTD](https://img.shields.io/badge/License-MEGA_EPC_PVT_LTD-yellow.svg?style=flat-square)](#)

<br>

<p align="center">
  <img src="docs/assets/cad_automation_demo.gif" width="900" alt="CAD Automation Demo">
</p>

</div>

## Overview

Industrial equipment design traditionally relies on manual CAD drafting, a process that is time-consuming, prone to human error, and difficult to standardize across engineering teams. Generating accurate layouts for tube sheets and baffles requires precise geometric calculations and iterative positioning.

MEGA Engineering Suite addresses this engineering problem by replacing manual drafting with programmatic generation. By entering strict engineering parameters into the suite, the application computes the required geometry, resolves spatial constraints, and generates deterministic output. These scripts and COM instructions directly interface with GstarCAD to output standardized, production-ready engineering drawings in seconds. The modular architecture ensures that new equipment types and layout variations can be seamlessly integrated into the existing workflow.

---

## Features

| Feature | Description |
| --- | --- |
| Parametric CAD Generation | Automatically generates drawings from engineering parameters |
| Standardized Document Lifecycle | Immutable master templates with automated drafting copies |
| COM Automation | Direct manipulation of CAD entities and title blocks |
| Modular Architecture | Flat, scalable structure supporting independent modules |
| Self-Healing Validation | Startup validations to ensure required templates exist |

---

## Modules

### TubeSheet Module

| Capability | Status |
| --- | --- |
| Front / Rear Tube Sheet | Complete |
| Tube Layout & Bolt Holes | Complete |
| Partition Plates & Side Views | Complete |
| Dimensioning & Annotations | Complete |
| Bonnet Flange Integration | Complete |
| LISP Generation Engine | Complete |

### Flange Module (Bonnet)

| Capability | Status |
| --- | --- |
| Dimensional Lookups | Complete |
| GstarCAD COM Automation | Complete |
| Title Block Extraction & Update | Complete |
| Isolated Template Pipeline | Complete |

---

## Architecture

```mermaid
graph TD;
    A[Engineering Inputs] --> B[Validation & App Config];
    B --> C[Calculation Engine];
    C --> D[CAD Adapter / Automation Service];
    D --> E[Template Copied to GeneratedDrawings];
    E --> F[GstarCAD COM / AutoLISP Execution];
    F --> G[Production CAD Drawing];
```

---

## CAD Document Lifecycle

The suite adheres to a strict, standardized CAD document lifecycle to ensure templates remain uncorrupted and Administrator privileges are never required:

```mermaid
graph TD;
    A[Master Template] -->|Copy| B[GeneratedDrawings Folder];
    B -->|Open via COM| C[Modify Geometry & Attributes];
    C -->|Save| D[Output Drawing];
    D -->|Close| E[Display to User];
```

---

## Technology Stack

| Technology | Purpose |
| --- | --- |
| C# (.NET 10.0) | Core application, forms, and COM adapters |
| ClosedXML | Reading parameters from Excel datasets |
| GstarCAD COM API | Direct programmatic CAD drafting |
| AutoLISP | Batch geometric calculations in CAD |

---

## Repository Structure

The suite uses a flat, highly portable architecture ensuring it runs immediately upon cloning:

```text
MEGA Engineering Suite
│
├── MegaEngineeringSuite      # Core Application & Modules (C#)
│   ├── BonnetFlange          # Bonnet Flange generator & annotation engine
│   ├── TubeSheet             # Tube Sheet automation & geometry
│   ├── Infrastructure        # CAD Adapters, COM Sessions, & Logging
│   └── Config                # Configuration managers & validation
│
├── Templates                 # Immutable CAD (.dwg) & Excel (.xlsx) templates
│
├── GeneratedDrawings         # Output directory for finalized DWG files
│
├── GeneratedLisp             # Output directory for generated LISP scripts
│
├── Logs                      # System diagnostics and execution logs
│
├── Config                    # Application configuration (Settings.json)
│
├── Docs                      # Engineering Standards & Deployment Guides
│
└── TestConsole               # Headless COM testing utility
```

---

## Future Roadmap

| Module / Feature | Status |
| --- | --- |
| TubeSheet | Complete |
| Flange / Bonnet | Complete |
| Baffle | Planned |
| Nozzle | Planned |
| Pipe Support | Planned |
| Tank | Planned |
| BOM Generation | Planned |
| Multi-CAD Support | Planned |

---

## Documentation

| Document | Link |
| --- | --- |
| CAD Lifecycle Standard | [CAD_Document_Lifecycle_Standard.md](docs/CAD_Document_Lifecycle_Standard.md) |
| Deployment Guide | [Deployment_Guide.md](docs/Deployment_Guide.md) |

---

<div align="center">
  
**MEGA Engineering Suite**

Developed by **MEGA Engineering Projects Pvt. Ltd.**

Professional CAD Automation for Process Equipment Design

</div>
