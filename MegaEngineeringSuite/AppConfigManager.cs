using System;
using System.IO;
using System.Text.Json;

namespace MegaEngineeringSuite
{
    public class AppSettings
    {
        public string CadPath { get; set; } = "";
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
        private static readonly string SettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Settings.json");
        private static AppSettings? _current;

        // Portable Root resolution
        public static string RootFolder
        {
            get
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                DirectoryInfo? dir = new DirectoryInfo(baseDir);
                while (dir != null)
                {
                    if (Directory.Exists(Path.Combine(dir.FullName, "Templates")))
                    {
                        return dir.FullName;
                    }
                    dir = dir.Parent;
                }
                return Path.GetFullPath(Path.Combine(baseDir, "..")); // Fallback
            }
        }

        public static string TemplatesFolder => Path.Combine(RootFolder, "Templates");
        public static string GeneratedDrawingsFolder => Path.Combine(RootFolder, "GeneratedDrawings");
        public static string GeneratedLispFolder => Path.Combine(RootFolder, "GeneratedLisp");
        public static string LogsFolder => Path.Combine(RootFolder, "Logs");

        public static AppSettings Current
        {
            get
            {
                if (_current == null)
                    Load();
                return _current!;
            }
        }

        [System.Runtime.InteropServices.DllImport("oleaut32.dll", PreserveSig = false)]
        static extern void GetActiveObject(ref Guid rclsid, IntPtr pvReserved, [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.IUnknown)] out object ppunk);

        private static object? TryGetActiveCOMObject(string progId)
        {
            Type? type = Type.GetTypeFromProgID(progId);
            if (type == null) return null;
            Guid clsid = type.GUID;
            try
            {
                object obj;
                GetActiveObject(ref clsid, IntPtr.Zero, out obj);
                return obj;
            }
            catch
            {
                return null;
            }
        }

        public static void InitializeFolders()
        {
            if (!Directory.Exists(TemplatesFolder)) Directory.CreateDirectory(TemplatesFolder);
            if (!Directory.Exists(GeneratedDrawingsFolder)) Directory.CreateDirectory(GeneratedDrawingsFolder);
            if (!Directory.Exists(GeneratedLispFolder)) Directory.CreateDirectory(GeneratedLispFolder);
            if (!Directory.Exists(LogsFolder)) Directory.CreateDirectory(LogsFolder);
        }

        public static void Load()
        {
            if (File.Exists(SettingsPath))
            {
                try
                {
                    string json = File.ReadAllText(SettingsPath);
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
            
            InitializeFolders();

            if (string.IsNullOrEmpty(_current.ExcelTemplatePath) || !File.Exists(_current.ExcelTemplatePath))
            {
                _current.ExcelTemplatePath = Path.Combine(TemplatesFolder, "Heat Exchanger BOM Details.xlsx");
                settingsUpdated = true;
            }

            if (string.IsNullOrEmpty(_current.DwgTemplatePath) || !File.Exists(_current.DwgTemplatePath))
            {
                _current.DwgTemplatePath = Path.Combine(TemplatesFolder, "FINAL TUBESHEET.dwg");
                settingsUpdated = true;
            }

            if (string.IsNullOrEmpty(_current.BonnetTemplatePath) || !File.Exists(_current.BonnetTemplatePath))
            {
                _current.BonnetTemplatePath = Path.Combine(TemplatesFolder, "BAFFLE_Flange_template.dwg");
                settingsUpdated = true;
            }

            if (string.IsNullOrEmpty(_current.BonnetOutputFolder) || !Directory.Exists(_current.BonnetOutputFolder))
            {
                _current.BonnetOutputFolder = GeneratedDrawingsFolder;
                settingsUpdated = true;
            }

            if (string.IsNullOrEmpty(_current.CadPath) || !File.Exists(_current.CadPath))
            {
                string detectedPath = DetectCadPath();
                if (!string.IsNullOrEmpty(detectedPath))
                {
                    _current.CadPath = detectedPath;
                    settingsUpdated = true;
                }
            }

            if (!File.Exists(SettingsPath) || settingsUpdated)
            {
                Save();
            }
        }

        private static string DetectCadPath()
        {
            try
            {
                dynamic? cadApp = TryGetActiveCOMObject("GstarCAD.Application");
                if (cadApp != null) return cadApp.FullName;
            }
            catch { }

            try
            {
                Type? type = Type.GetTypeFromProgID("GstarCAD.Application");
                if (type != null)
                {
                    dynamic cadApp = Activator.CreateInstance(type);
                    string path = cadApp.FullName;
                    cadApp.Quit();
                    return path;
                }
            }
            catch { }

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
            File.WriteAllText(SettingsPath, json);
        }
    }
}
