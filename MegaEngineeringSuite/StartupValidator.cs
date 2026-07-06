using System;
using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;

namespace MegaEngineeringSuite
{
    public class ValidationResult
    {
        public bool TemplatesValid { get; set; } = true;
        public bool CadValid { get; set; } = true;
        public bool ComValid { get; set; } = true;
        
        public bool IsValid => TemplatesValid && CadValid && ComValid;
    }

    public static class StartupValidator
    {
        public static ValidationResult Validate()
        {
            var result = new ValidationResult();
            var config = AppConfigManager.Current;
            var missingFiles = new List<string>();
            var templatesFolder = Path.Combine(AppConfigManager.RootFolder, "Templates");

            var logLines = new List<string> 
            { 
                "Root Folder", 
                AppConfigManager.RootFolder,
                "",
                "Templates Folder",
                templatesFolder,
                ""
            };

            // 1. Validate Templates
            string dwgPath = config.DwgTemplatePath;
            string bonnetPath = config.BonnetTemplatePath;
            string excelPath = config.ExcelTemplatePath;

            bool dwgExists = File.Exists(dwgPath);
            bool bonnetExists = File.Exists(bonnetPath);
            bool excelExists = File.Exists(excelPath);

            logLines.Add("Tube Sheet");
            logLines.Add(dwgPath);
            logLines.Add($"Exists={(dwgExists ? "True" : "False")}");
            logLines.Add("");

            logLines.Add("Bonnet");
            logLines.Add(bonnetPath);
            logLines.Add($"Exists={(bonnetExists ? "True" : "False")}");
            logLines.Add("");

            logLines.Add("Excel");
            logLines.Add(excelPath);
            logLines.Add($"Exists={(excelExists ? "True" : "False")}");
            logLines.Add("");

            if (!dwgExists) missingFiles.Add("FINAL TUBESHEET.dwg");
            if (!bonnetExists) missingFiles.Add("BAFFLE_Flange_template.dwg");
            if (!excelExists) missingFiles.Add("Heat Exchanger BOM Details.xlsx");

            string missingTemplatesMsg = null;
            if (missingFiles.Count > 0)
            {
                result.TemplatesValid = false;
                string msg = "Missing Runtime Resources\n\nRoot Folder\n" + AppConfigManager.RootFolder + "\n\nTemplates Folder\n" + templatesFolder + "\n\nMissing\n\n";
                foreach (var f in missingFiles)
                    msg += $"• {f}\n\n";
                msg += "Generation modules will remain disabled until the missing resources are restored.";
                missingTemplatesMsg = msg;
            }

            // Check Generated Folders
            string[] reqFolders = { "GeneratedDrawings", "GeneratedLisp", "Logs", "Config" };
            foreach (var f in reqFolders)
            {
                string p = Path.Combine(AppConfigManager.RootFolder, f);
                logLines.Add(f);
                logLines.Add(p);
                logLines.Add($"Exists={(Directory.Exists(p) ? "True" : "False")}");
                logLines.Add("");
            }

            // 2. Validate CAD Executable
            bool cadExists = !string.IsNullOrEmpty(config.CadPath) && File.Exists(config.CadPath);
            logLines.Add("CAD");
            logLines.Add(config.CadPath ?? "None");
            logLines.Add($"Exists={(cadExists ? "True" : "False")}");
            logLines.Add("");

            string missingCadMsg = null;
            if (!cadExists)
            {
                result.CadValid = false;
                missingCadMsg = "GstarCAD not found.\n\nPlease install GstarCAD before using Mega Engineering Suite.";
            }

            // 3. Validate COM Connection
            string comStatus = "False";
            string comErrorMsg = null;
            try
            {
                Type? type = Type.GetTypeFromProgID("GstarCAD.Application");
                if (type == null)
                {
                    result.ComValid = false;
                    comErrorMsg = "Unable to connect to GstarCAD COM.\n\nGstarCAD COM components are not registered properly.";
                }
                else
                {
                    comStatus = "True";
                }
            }
            catch
            {
                result.ComValid = false;
                comErrorMsg = "Unable to connect to GstarCAD COM.\n\nAn unexpected error occurred while querying the COM registry.";
            }
            
            logLines.Add("COM");
            logLines.Add("GstarCAD.Application");
            logLines.Add($"Exists={comStatus}");
            logLines.Add("");

            // Write Log
            try
            {
                string logPath = Path.Combine(AppConfigManager.RootFolder, "Logs", "startup.log");
                File.WriteAllLines(logPath, logLines);
            }
            catch { }

            // Display MessageBoxes after logging so headless testing isn't completely blocked before logs are written
            if (missingTemplatesMsg != null)
                MessageBox.Show(missingTemplatesMsg, "Missing Runtime Resources", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            
            if (missingCadMsg != null)
                MessageBox.Show(missingCadMsg, "CAD Missing", MessageBoxButtons.OK, MessageBoxIcon.Warning);

            if (comErrorMsg != null)
                MessageBox.Show(comErrorMsg, "COM Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);

            return result;
        }
    }
}
