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
            var logLines = new List<string> { "Application Information", "", $"Root Folder:\n{AppConfigManager.RootFolder}", "" };

            // 1. Validate Templates
            string dwgPath = config.DwgTemplatePath;
            string bonnetPath = config.BonnetTemplatePath;
            string excelPath = config.ExcelTemplatePath;

            if (File.Exists(dwgPath))
                logLines.Add("Tube Sheet:\nFOUND\n");
            else
            {
                logLines.Add("Tube Sheet:\nMISSING\n");
                missingFiles.Add("FINAL TUBESHEET.dwg");
            }

            if (File.Exists(bonnetPath))
                logLines.Add("Bonnet:\nFOUND\n");
            else
            {
                logLines.Add("Bonnet:\nMISSING\n");
                missingFiles.Add("BAFFLE_Flange_template.dwg");
            }

            if (File.Exists(excelPath))
                logLines.Add("Excel:\nFOUND\n");
            else
            {
                logLines.Add("Excel:\nMISSING\n");
                missingFiles.Add("Heat Exchanger BOM Details.xlsx");
            }

            if (missingFiles.Count > 0)
            {
                result.TemplatesValid = false;
                string msg = "Expected Folder:\n" + Path.Combine(AppConfigManager.RootFolder, "Templates") + "\n\nMissing Files:\n";
                foreach (var f in missingFiles)
                    msg += $"- {f}\n";

                MessageBox.Show(msg, "Missing Templates", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            // 2. Validate CAD Executable
            if (string.IsNullOrEmpty(config.CadPath) || !File.Exists(config.CadPath))
            {
                result.CadValid = false;
                logLines.Add("CAD:\nMISSING\n");
                MessageBox.Show("GstarCAD not found.\n\nPlease install GstarCAD before using Mega Engineering Suite.", "CAD Missing", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                logLines.Add("CAD:\nFOUND\n");
            }

            // 3. Validate COM Connection
            try
            {
                Type? type = Type.GetTypeFromProgID("GstarCAD.Application");
                if (type == null)
                {
                    result.ComValid = false;
                    logLines.Add("COM:\nDISCONNECTED\n");
                    MessageBox.Show("Unable to connect to GstarCAD COM.\n\nGstarCAD COM components are not registered properly.", "COM Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    logLines.Add("COM:\nCONNECTED\n");
                }
            }
            catch
            {
                result.ComValid = false;
                logLines.Add("COM:\nERROR\n");
                MessageBox.Show("Unable to connect to GstarCAD COM.\n\nAn unexpected error occurred while querying the COM registry.", "COM Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // Write Log
            try
            {
                string logPath = Path.Combine(AppConfigManager.RootFolder, "Logs", "startup.log");
                File.WriteAllLines(logPath, logLines);
            }
            catch { }

            return result;
        }
    }
}
