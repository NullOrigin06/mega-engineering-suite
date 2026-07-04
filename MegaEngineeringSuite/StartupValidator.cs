using System;
using System.IO;
using System.Windows.Forms;
using MegaEngineeringSuite.Infrastructure.Cad;

namespace MegaEngineeringSuite
{
    public static class StartupValidator
    {
        public static void Validate()
        {
            try
            {
                // 1. Validate Configuration and Folders
                try
                {
                    AppConfigManager.Load();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Unable to initialize configuration or create required folders (GeneratedDrawings, Logs, etc.).\n\nPlease ensure you have write permissions to the application directory.\n\nError: {ex.Message}",
                        "Configuration Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }

                // 2. Validate Templates
                var config = AppConfigManager.Current;
                bool missingDwg = string.IsNullOrEmpty(config.DwgTemplatePath) || !File.Exists(config.DwgTemplatePath);
                bool missingBonnet = string.IsNullOrEmpty(config.BonnetTemplatePath) || !File.Exists(config.BonnetTemplatePath);
                bool missingExcel = string.IsNullOrEmpty(config.ExcelTemplatePath) || !File.Exists(config.ExcelTemplatePath);

                if (missingDwg || missingBonnet || missingExcel)
                {
                    MessageBox.Show(
                        "Templates missing.\n\nExpected:\n\nFINAL TUBESHEET.dwg (in Templates/Drawings/)\nBAFFLE_Flange_template.dwg (in Templates/Drawings/)\nHeat Exchanger BOM Details.xlsx (in Templates/Excel/)\n\nPlease place these files in the correct directories and restart the application.",
                        "Missing Templates",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }

                // 3. Validate GstarCAD Installation
                if (string.IsNullOrEmpty(config.CadPath) || !File.Exists(config.CadPath))
                {
                    MessageBox.Show(
                        "GstarCAD not found.\n\nPlease install GstarCAD before using Mega Engineering Suite.",
                        "CAD Missing",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }

                // 4. Validate COM Connection
                try
                {
                    Type? type = Type.GetTypeFromProgID("GstarCAD.Application");
                    if (type == null)
                    {
                        MessageBox.Show(
                            "Unable to connect to GstarCAD COM.\n\nGstarCAD COM components are not registered properly.",
                            "COM Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                    }
                }
                catch
                {
                    MessageBox.Show(
                        "Unable to connect to GstarCAD COM.\n\nAn unexpected error occurred while querying the COM registry.",
                        "COM Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unexpected error during startup validation: {ex.Message}", "Startup Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
