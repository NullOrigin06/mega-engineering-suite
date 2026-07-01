using System;
using System.IO;
using System.Text.Json;

namespace MegaEngineeringSuite
{
    public class AppSettings
    {
        public string CadPath { get; set; } = @"C:\Program Files\Gstarsoft\GstarCAD2026\gcad.exe";
        public string ExcelTemplatePath { get; set; } = @"C:\Users\PARTH\OneDrive\Desktop\MEGA_TEMPLATES\Heat Exchanger BOM Details.xlsx";
        public string DwgTemplatePath { get; set; } = @"C:\Users\PARTH\OneDrive\Desktop\MEGA_TEMPLATES\FINAL TUBESHEET.dwg";
        public string BonnetTemplatePath { get; set; } = @"C:\Users\PARTH\OneDrive\Desktop\MEGA_TEMPLATES\BAFFLE_Flange_template.dwg";
        public string BonnetOutputFolder { get; set; } = @"C:\Users\PARTH\OneDrive\Desktop\MEGA_TEMPLATES\GeneratedDrawings";
    }

    public static class AppConfigManager
    {
        private static readonly string SettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Settings.json");
        private static AppSettings? _current;

        public static AppSettings Current
        {
            get
            {
                if (_current == null)
                    Load();
                return _current!;
            }
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
            if (!File.Exists(_current.CadPath))
            {
                string[] commonPaths = new string[]
                {
                    @"C:\Program Files\Gstarsoft\GstarCAD2026\gcad.exe",
                    @"C:\Program Files\Gstarsoft\GstarCAD2025\gcad.exe",
                    @"C:\Program Files\Gstarsoft\GstarCAD2024\gcad.exe"
                };

                foreach (string path in commonPaths)
                {
                    if (File.Exists(path))
                    {
                        _current.CadPath = path;
                        settingsUpdated = true;
                        break;
                    }
                }
            }

            if (!File.Exists(SettingsPath) || settingsUpdated)
            {
                Save();
            }
        }

        public static void Save()
        {
            if (_current == null) return;
            string json = JsonSerializer.Serialize(_current, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(SettingsPath, json);
        }
    }
}
