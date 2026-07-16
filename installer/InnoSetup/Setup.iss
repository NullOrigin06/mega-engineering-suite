; Mega Engineering Suite Inno Setup Script
; Stage 14.2 - Production Installer

#define MyAppName "Mega Engineering Suite"
#define MyAppVersion "1.2.0"
#define MyAppPublisher "Parth Devs"
#define MyAppExeName "MegaEngineeringSuite.exe"

[Setup]
; App Information
AppId={{D8C02B8A-7C91-4512-B647-9831F2E8A9C1}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}

; Version Info Metadata for Windows Explorer
VersionInfoVersion={#MyAppVersion}
VersionInfoCompany={#MyAppPublisher}
VersionInfoDescription={#MyAppName} Setup
VersionInfoProductName={#MyAppName}

; Output Configuration
OutputDir=..\Output
OutputBaseFilename=MegaEngineeringSuite_Setup_v1.2.0
Compression=lzma2
SolidCompression=yes

; General Settings
PrivilegesRequired=admin
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
DisableWelcomePage=no
DisableDirPage=no

; Uninstall & Upgrade Behavior
UninstallDisplayIcon={app}\{#MyAppExeName}
CloseApplications=yes
RestartApplications=no

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
; 1. Package the Config file separately to preserve user settings on upgrade
Source: "..\Output\publish\Config\Settings.json"; DestDir: "{app}\Config"; Flags: onlyifdoesntexist uninsneveruninstall

; 2. Package all other files, strictly excluding generated runtime folders and development artifacts
Source: "..\Output\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs; Excludes: "Config\Settings.json, GeneratedDrawings\*, GeneratedLisp\*, Logs\*, *.bak, *.pdb, *.xml, *.tmp, *.log, desktop.ini, .DS_Store"

[Dirs]
; Create dynamic folders required by the application explicitly, even if empty
Name: "{app}\GeneratedDrawings"; Permissions: users-modify
Name: "{app}\GeneratedLisp"; Permissions: users-modify
Name: "{app}\Logs"; Permissions: users-modify
Name: "{app}\Config"; Permissions: users-modify

[Icons]
; Start Menu Shortcut
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
; Start Menu Uninstall Shortcut
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
; Optional Desktop Shortcut
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Code]
var
  DotNetMissing: Boolean;
  GstarCADMissing: Boolean;

// Helper function to check registry for GstarCAD
function IsGstarCADInstalled(): Boolean;
begin
  Result := RegKeyExists(HKEY_CLASSES_ROOT, 'GstarCAD.Application');
end;

// Helper function to check if .NET 10 Desktop Runtime is installed
function IsDotNet10DesktopInstalled(): Boolean;
begin
  Result := RegKeyExists(HKEY_LOCAL_MACHINE, 'SOFTWARE\dotnet\Setup\InstalledVersions\x64\sharedhost');
end;

function InitializeSetup(): Boolean;
begin
  Result := True;
  DotNetMissing := False;
  GstarCADMissing := False;

  // 1. Check for .NET 10 Desktop Runtime
  if not IsDotNet10DesktopInstalled() then
  begin
    DotNetMissing := True;
    MsgBox('Mega Engineering Suite requires the Microsoft .NET 10 Desktop Runtime to be installed.' + #13#10#13#10 +
           'Please download and install it from Microsoft, then run this setup again.', mbCriticalError, MB_OK);
    Result := False;
    Exit;
  end;

  // 2. Check for GstarCAD
  if not IsGstarCADInstalled() then
  begin
    GstarCADMissing := True;
    if MsgBox('GstarCAD was not detected on this system.' + #13#10#13#10 +
              'Mega Engineering Suite requires GstarCAD for drawing generation.' + #13#10 +
              'You may continue with the installation, but drawing generation will fail until GstarCAD is installed.' + #13#10#13#10 +
              'Do you want to continue with the installation?', mbConfirmation, MB_YESNO) = IDNO then
    begin
      Result := False;
      Exit;
    end;
  end;
end;

function InitializeUninstall(): Boolean;
begin
  // Explicitly remind the user to close GstarCAD and the application before uninstalling 
  // so that the COM objects and executables are freed and not locked by the OS.
  if MsgBox('Please ensure that Mega Engineering Suite and GstarCAD are completely closed before continuing.' + #13#10#13#10 +
            'If they are running, the uninstaller may fail to remove locked files.' + #13#10#13#10 +
            'Are you ready to continue?', mbConfirmation, MB_YESNO) = IDNO then
  begin
    Result := False;
  end
  else
  begin
    Result := True;
  end;
end;
