using System;
using System.IO;
using System.Text.Json;

namespace MegaEngineeringSuite
{
    public class AppSettings
    {
        public bool UsePipelineV2 { get; set; } = true;
        public bool EnablePipelineDiagnostics { get; set; } = true;
        public string CadPath { get; set; } = @"C:\Program Files\Gstarsoft\GstarCAD2026\gcad.exe";
        public string ExcelTemplatePath { get; set; } = "";
        public string DwgTemplatePath { get; set; } = "";
        public string BonnetTemplatePath { get; set; } = "";
        public string BonnetOutputFolder { get; set; } = "";
        public string HeatExchangerTemplatePath { get; set; } = "";
        public string HeatExchangerOutputFolder { get; set; } = "";
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
        private static string _userDataFolder = "";
        private static string _settingsPath = "";
        private static AppSettings? _current;

        public static string RootFolder
        {
            get
            {
                if (string.IsNullOrEmpty(_rootFolder))
                    DetermineFolders();
                return _rootFolder;
            }
        }

        public static string UserDataFolder
        {
            get
            {
                if (string.IsNullOrEmpty(_userDataFolder))
                    DetermineFolders();
                return _userDataFolder;
            }
        }

        public static AppSettings Current
        {
            get
            {
                if (_current == null)
                    Load();
                return _current!;
            }
        }

        private static void DetermineFolders()
        {
            _rootFolder = AppDomain.CurrentDomain.BaseDirectory;
            _userDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MEGA Engineering Suite");
        }

        private static void InitializeFolders()
        {
            // Application directories
            string templatesPath = Path.Combine(_rootFolder, "Templates");
            if (!Directory.Exists(templatesPath)) Directory.CreateDirectory(templatesPath);

            // User data directories
            string[] userFolders = { "GeneratedDrawings", "GeneratedLisp", "Logs", "Config" };
            foreach (var folder in userFolders)
            {
                string path = Path.Combine(_userDataFolder, folder);
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }
        }

        public static string NormalizeResourcePath(string? path, string defaultRelative)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return Path.Combine(_rootFolder, defaultRelative);
            }
            if (Path.IsPathRooted(path))
            {
                return path;
            }
            return Path.GetFullPath(Path.Combine(_rootFolder, path));
        }

        public static string NormalizeUserDataPath(string? path, string defaultRelative)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return Path.Combine(_userDataFolder, defaultRelative);
            }
            if (Path.IsPathRooted(path))
            {
                return path;
            }
            return Path.GetFullPath(Path.Combine(_userDataFolder, path));
        }

        public static void Load()
        {
            DetermineFolders();
            InitializeFolders();

            _settingsPath = Path.Combine(_userDataFolder, "Config", "Settings.json");
            string defaultSettingsPath = Path.Combine(_rootFolder, "Config", "Settings.json");

            // Seed from default installation config if missing in LocalAppData
            if (!File.Exists(_settingsPath) && File.Exists(defaultSettingsPath))
            {
                try
                {
                    File.Copy(defaultSettingsPath, _settingsPath, false);
                }
                catch { }
            }

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

            // Dynamically normalize and set default template paths (Application Resources)
            _current.ExcelTemplatePath = NormalizeResourcePath(_current.ExcelTemplatePath, Path.Combine("Templates", "Heat Exchanger BOM Details.xlsx"));
            _current.DwgTemplatePath = NormalizeResourcePath(_current.DwgTemplatePath, Path.Combine("Templates", "FINAL TUBESHEET.dwg"));
            _current.BonnetTemplatePath = NormalizeResourcePath(_current.BonnetTemplatePath, Path.Combine("Templates", "BAFFLE_Flange_template.dwg"));
            _current.HeatExchangerTemplatePath = NormalizeResourcePath(_current.HeatExchangerTemplatePath, Path.Combine("Templates", "Heat_Exchanger_Fabrication_template.dwg"));

            // Dynamically normalize output folders (User Data)
            _current.BonnetOutputFolder = NormalizeUserDataPath(_current.BonnetOutputFolder, "GeneratedDrawings");
            _current.HeatExchangerOutputFolder = NormalizeUserDataPath(_current.HeatExchangerOutputFolder, "GeneratedDrawings");

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
            var discovered = Infrastructure.Cad.CadDiscoveryService.DiscoverInstalledCadExecutables();
            return discovered.Count > 0 ? discovered[0].ExecutablePath : "";
        }

        public static void Save()
        {
            if (_current == null) return;
            string json = JsonSerializer.Serialize(_current, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_settingsPath, json);
        }
    }
}
