# Mega Engineering Suite

## Project Overview
Mega Engineering Suite is a professional C# (.NET) desktop application designed to automate the generation of CAD engineering drawings for heavy equipment, such as heat exchangers. The application acts as a bridge between user-input dimensions and CAD software (GstarCAD/AutoCAD), driving both legacy LISP geometry engines and modern COM-based dynamic placeholder replacement.

## Features
- **Tube Sheet Generation**: Fully automates drawing of Tube Sheets using a highly-optimized LISP geometry generation engine.
- **Bonnet Flange Generation**: Uses a COM-based dynamic placeholder architecture to accurately populate standard Bonnet Flange templates.
- **Dynamic Title Block & BOM**: Seamless integration with existing CAD templates to update bills of materials, descriptions, and dimensional properties.
- **Portable Deployment**: Runs completely standalone and intelligently auto-generates all requisite folder structures on the fly.

## Requirements
- **OS**: Windows 10/11
- **Runtime**: .NET 10.0 Windows Desktop Runtime
- **CAD**: GstarCAD (or AutoCAD) installed locally
- **IDE**: Visual Studio 2022 (for developers only)

## Installation
1. **Clone the repository**:
   ```bash
   git clone <repository_url>
   ```
2. **Restore Packages & Build**:
   Open `MegaEngineeringSuite.sln` in Visual Studio and Build the Solution.
3. **Launch**:
   Run the compiled `MegaEngineeringSuite.exe`.

## Required Templates
For the generation algorithms to work, ensure the following proprietary templates exist in the `Templates/` directory at the root of the project:

- `Templates/Drawings/FINAL TUBESHEET.dwg`
- `Templates/Drawings/BAFFLE_Flange_template.dwg`
- `Templates/Excel/Heat Exchanger BOM Details.xlsx`

If they are missing on startup, the application will alert you.

## Folder Structure
```text
MegaEngineeringSuite
│
├── MegaEngineeringSuite.sln
├── MegaEngineeringSuite/ (Source Code)
│
├── Templates/
│      ├── Drawings/
│      │      FINAL TUBESHEET.dwg
│      │      BAFFLE_Flange_template.dwg
│      └── Excel/
│             Heat Exchanger BOM Details.xlsx
│
├── Config/ (Generated at runtime)
│      Settings.json
│
├── GeneratedDrawings/ (Generated at runtime)
├── GeneratedLisp/ (Generated at runtime)
├── Logs/ (Generated at runtime)
│
├── Docs/
│      Deployment_Guide.md
│      Configuration_Guide.md
│
├── CHANGELOG.md
└── README.md
```

## Screenshots
*(UI Screenshots will be added here in a future release)*
