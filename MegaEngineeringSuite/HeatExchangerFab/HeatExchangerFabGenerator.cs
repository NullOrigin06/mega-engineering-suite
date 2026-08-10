using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using MegaEngineeringSuite.Infrastructure.Cad;
using MegaEngineeringSuite.Infrastructure.Logging;

namespace MegaEngineeringSuite.HeatExchangerFab
{
    public class HeatExchangerFabGenerator
    {
        public string Generate(HeatExchangerFabData data, DrawingInformation drawInfo)
        {
            SimpleLogger.Log("HeatExchangerFab", "Starting Heat Exchanger Fabrication Drawing Generation...");
            
            string templatePath = AppConfigManager.Current.HeatExchangerTemplatePath;
            if (string.IsNullOrEmpty(templatePath) || !File.Exists(templatePath))
            {
                throw new InvalidOperationException($"Configuration Exception: Template file not found at: {templatePath}");
            }

            string outputFolder = AppConfigManager.Current.HeatExchangerOutputFolder;
            if (string.IsNullOrEmpty(outputFolder))
            {
                throw new InvalidOperationException("Configuration Exception: HeatExchangerOutputFolder is not configured.");
            }

            if (!Directory.Exists(outputFolder))
            {
                Directory.CreateDirectory(outputFolder);
            }

            // 1. Format text replacements
            var replacements = HeatExchangerFabFormatter.Format(data);
            
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string fileName = $"HE_Fab_{data.ShellID}_{timestamp}.dwg";
            string outputPath = Path.Combine(outputFolder, fileName);

            // Phase Timings
            var swAcquire = new Stopwatch();
            var swOpen = new Stopwatch();
            var swSave = new Stopwatch();
            var swClose = new Stopwatch();
            CadOperationTimes innerTimes = new CadOperationTimes();

            SimpleLogger.Log("HeatExchangerFab", "--- HEAT EXCHANGER FABRICATION GENERATION ---");
            SimpleLogger.Log("HeatExchangerFab", $"Template: {templatePath}");
            SimpleLogger.Log("HeatExchangerFab", $"Output: {outputPath}");

            // 1. Copy Template Before Launch
            try
            {
                if (File.Exists(outputPath))
                {
                    File.SetAttributes(outputPath, FileAttributes.Normal);
                    File.Delete(outputPath);
                }

                File.Copy(templatePath, outputPath);
                File.SetAttributes(outputPath, FileAttributes.Normal);
                
                if (!File.Exists(outputPath))
                {
                    throw new IOException("Template copy failed: Output file does not exist after copy operation.");
                }
                SimpleLogger.Log("HeatExchangerFab", "Copy SUCCESS");
            }
            catch (Exception ex)
            {
                SimpleLogger.Log("HeatExchangerFab", $"Copy FAILED: {ex.Message}");
                throw;
            }

            // 2. Connect to CAD & Update
            swAcquire.Start();
            using (ICadAdapter cadAdapter = new GstarCadAdapter())
            {
                swAcquire.Stop();
                cadAdapter.KeepDocumentOpenOnDispose = true;
                
                swOpen.Start();
                cadAdapter.OpenDrawing(outputPath);
                SimpleLogger.Log("HeatExchangerFab", "Open SUCCESS");
                swOpen.Stop();

                var engine = new MegaEngineeringSuite.BonnetFlange.AnnotationEngine(cadAdapter);
                innerTimes = engine.ProcessAnnotations(replacements);
                
                // Update Title Block using single-pass cache
                cadAdapter.UpdateTitleBlockAttributes(drawInfo);
                SimpleLogger.Log("HeatExchangerFab", "Annotation & Title Block SUCCESS");

                swSave.Start();
                cadAdapter.Save();
                SimpleLogger.Log("HeatExchangerFab", "Save SUCCESS");
                swSave.Stop();
                
                // Activate document and show visible CAD window
                cadAdapter.ActivateAndShow();
            }
            SimpleLogger.Log("HeatExchangerFab", "Drawing Active and Visible in CAD Session");

            SimpleLogger.Log("HeatExchangerFab", $"Output: {outputPath}");
            SimpleLogger.Log("HeatExchangerFab", "Finished");
            
            // Output timing breakdown
            long totalMs = swAcquire.ElapsedMilliseconds + swOpen.ElapsedMilliseconds + 
                           innerTimes.ScanTimeMs + innerTimes.ReplaceTimeMs + 
                           swSave.ElapsedMilliseconds + swClose.ElapsedMilliseconds;

            SimpleLogger.Log("HeatExchangerFab", "--- TIMING BREAKDOWN ---");
            SimpleLogger.Log("HeatExchangerFab", $"Acquire CAD........ {swAcquire.ElapsedMilliseconds} ms");
            SimpleLogger.Log("HeatExchangerFab", $"Open Drawing....... {swOpen.ElapsedMilliseconds} ms");
            SimpleLogger.Log("HeatExchangerFab", $"Scan Entities...... {innerTimes.ScanTimeMs} ms");
            SimpleLogger.Log("HeatExchangerFab", $"Replace............ {innerTimes.ReplaceTimeMs} ms");
            SimpleLogger.Log("HeatExchangerFab", $"Save Drawing....... {swSave.ElapsedMilliseconds} ms");
            SimpleLogger.Log("HeatExchangerFab", $"Close Drawing...... {swClose.ElapsedMilliseconds} ms");
            SimpleLogger.Log("HeatExchangerFab", $"Total.............. {totalMs} ms");
            SimpleLogger.Log("HeatExchangerFab", "------------------------");
            
            return outputPath;
        }
    }
}
