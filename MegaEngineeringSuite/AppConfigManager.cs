using System;
using System.IO;
using System.Text.Json;

namespace MegaEngineeringSuite
{
    public class AppSettings
    {
        public string CadPath { get; set; } = @"C:\Program Files\Gstarsoft\GstarCAD2026\gcad.exe";
        public string ExcelTemplatePath { get; set; } = "";
        public string DwgTemplatePath { get; set; } = "";
        public string BonnetTemplatePath { get; set; } = "";
        public string BonnetOutputFolder { get; set; } = "";
        public System.Collections.Generic.List<string> CustomerHistory { get; set; } = new System.Collections.Generic.List<string> { "MEGA EPC", "L&T", "Thermax", "BHEL", "Reliance" };
        public System.Collections.Generic.List<string> DrawingNoHistory { get; set; } = new System.Collections.Generic.List<string>();
        public System.Collections.Generic.List<string> DrawingTitleHistory { get; set; } = new System.Collections.Generic.List<string> { "Bonnet Flange Details For", "Tube Sheet Details For", "Body Flange Details For", "Heat Chamber Details For", "Cylinder Details For", "Channel Details For", "Floating Head Details For" };

        public System.Collections.Generic.List<string> HTAHistory { get; set; } = new System.Collections.Generic.List<string>();
        public System.Collections.Generic.List<string> TubeODHistory { get; set; } = new System.Collections.Generic.List<string>();
        public System.Collections.Generic.List<string> TubeLengthHistory { get; set; } = new System.Collections.Generic.List<string>();
        public System.Collections.Generic.List<string> TubeTHKHistory { get; set; } = new System.Collections.Generic.List<string>();
        public System.Collections.Generic.List<string> BaffleQtyHistory { get; set; } = new System.Collections.Generic.List<string>();
        public System.Collections.Generic.List<string> NoOfPassHistory { get; set; } = new System.Collections.Generic.List<string> { "1", "2", "4", "6", "8" };
        
        public System.Collections.Generic.List<string> ProjectNoHistory { get; set; } = new System.Collections.Generic.List<string>();
        public System.Collections.Generic.List<string> RevisionHistory { get; set; } = new System.Collections.Generic.List<string>();
        public System.Collections.Generic.List<string> DateHistory { get; set; } = new System.Collections.Generic.List<string>();
        public System.Collections.Generic.List<string> PreparedByHistory { get; set; } = new System.Collections.Generic.List<string>();
        public System.Collections.Generic.List<string> CheckedByHistory { get; set; } = new System.Collections.Generic.List<string>();
        public System.Collections.Generic.List<string> ApprovedByHistory { get; set; } = new System.Collections.Generic.List<string>();
    }

    public static class AppConfigManager
    {
        private static string _rootFolder = "";
        private static string _settingsPath = "";
        private static AppSettings? _current;

        public static string RootFolder => _rootFolder;

        public static AppSettings Current
        {
            get
            {
                if (_current == null)
                    Load();
                return _current!;
            }
        }

        private static void DetermineRootFolder()
        {
            string currentDir = AppDomain.CurrentDomain.BaseDirectory;
            while (!string.IsNullOrEmpty(currentDir))
            {
                if (Directory.Exists(Path.Combine(currentDir, "Templates")))
                {
                    _rootFolder = currentDir;
                    return;
                }
                currentDir = Directory.GetParent(currentDir)?.FullName ?? "";
            }
            // Fallback to BaseDirectory if not found upwards
            _rootFolder = AppDomain.CurrentDomain.BaseDirectory;
        }

        private static void InitializeFolders()
        {
            string[] folders = { "Templates", "GeneratedDrawings", "GeneratedLisp", "Logs", "Config" };
            foreach (var folder in folders)
            {
                string path = Path.Combine(_rootFolder, folder);
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }
        }

        public static void Load()
        {
            DetermineRootFolder();
            InitializeFolders();

            _settingsPath = Path.Combine(_rootFolder, "Config", "Settings.json");

            if (File.Exists(_settingsPath))
            {
                try
                {
                    string json = File.ReadAllText(_settingsPath);
                    _current = JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
                }
                catch
                {
                    _current = new AppSettings();
                }
            }
            else
            {
                _current = new AppSettings();
            }

            bool settingsUpdated = false;

            // Dynamically set default template paths if empty
            if (string.IsNullOrEmpty(_current.ExcelTemplatePath))
            {
                _current.ExcelTemplatePath = Path.Combine(_rootFolder, "Templates", "Heat Exchanger BOM Details.xlsx");
                settingsUpdated = true;
            }
            if (string.IsNullOrEmpty(_current.DwgTemplatePath))
            {
                _current.DwgTemplatePath = Path.Combine(_rootFolder, "Templates", "FINAL TUBESHEET.dwg");
                settingsUpdated = true;
            }
            if (string.IsNullOrEmpty(_current.BonnetTemplatePath))
            {
                _current.BonnetTemplatePath = Path.Combine(_rootFolder, "Templates", "BAFFLE_Flange_template.dwg");
                settingsUpdated = true;
            }
            if (string.IsNullOrEmpty(_current.BonnetOutputFolder))
            {
                _current.BonnetOutputFolder = Path.Combine(_rootFolder, "GeneratedDrawings");
                settingsUpdated = true;
            }

            // CAD detection
            if (string.IsNullOrEmpty(_current.CadPath) || !File.Exists(_current.CadPath))
            {
                string detectedPath = DetectCadPath();
                if (!string.IsNullOrEmpty(detectedPath))
                {
                    _current.CadPath = detectedPath;
                    settingsUpdated = true;
                }
            }

            if (!File.Exists(_settingsPath) || settingsUpdated)
            {
                Save();
            }
        }

        private static string DetectCadPath()
        {
            // 2. Windows registry install location
            try
            {
                using (var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\gcad.exe"))
                {
                    if (key != null)
                    {
                        string? path = key.GetValue("") as string;
                        if (!string.IsNullOrEmpty(path) && File.Exists(path))
                        {
                            return path;
                        }
                    }
                }
            }
            catch { }

            try
            {
                using (var key = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(@"gcad.exe\shell\open\command"))
                {
                    if (key != null)
                    {
                        string? command = key.GetValue("") as string;
                        if (!string.IsNullOrEmpty(command))
                        {
                            string path = command.Split('"')[1];
                            if (File.Exists(path)) return path;
                        }
                    }
                }
            }
            catch { }

            // 3. standard installation directories
            string[] commonPaths = new string[]
            {
                @"C:\Program Files\Gstarsoft\GstarCAD2026\gcad.exe",
                @"C:\Program Files\Gstarsoft\GstarCAD2025\gcad.exe",
                @"C:\Program Files\Gstarsoft\GstarCAD2024\gcad.exe",
                @"C:\Program Files\Autodesk\AutoCAD 2024\acad.exe",
                @"C:\Program Files\Autodesk\AutoCAD 2023\acad.exe"
            };

            foreach (string path in commonPaths)
            {
                if (File.Exists(path)) return path;
            }

            return "";
        }

        public static void Save()
        {
            if (_current == null) return;
            string json = JsonSerializer.Serialize(_current, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_settingsPath, json);
        }
    }
}
