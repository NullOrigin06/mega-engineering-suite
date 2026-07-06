using System;
using System.IO;
using System.Diagnostics;
using MegaEngineeringSuite.Infrastructure.Cad;
using MegaEngineeringSuite.Infrastructure.Logging;

namespace MegaEngineeringSuite.BonnetFlange
{
    public class BonnetFlangeGenerator
    {
        public string Generate(BonnetFlangeData data, DrawingInformation drawInfo)
        {
            SimpleLogger.Log("BonnetFlange", "Starting Drawing Generation...");
            
            string templatePath = AppConfigManager.Current.BonnetTemplatePath;
            if (string.IsNullOrEmpty(templatePath) || !File.Exists(templatePath))
            {
                throw new InvalidOperationException($"Configuration Exception: Template file not found at: {templatePath}");
            }

            string outputFolder = AppConfigManager.Current.BonnetOutputFolder;
            if (string.IsNullOrEmpty(outputFolder))
            {
                throw new InvalidOperationException("Configuration Exception: BonnetOutputFolder is not configured.");
            }

            if (!Directory.Exists(outputFolder))
            {
                Directory.CreateDirectory(outputFolder);
            }

            // 1. Format the text
            var replacements = AnnotationFormatter.Format(data);
            
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string fileName = $"BF_{data.OD}_{data.ID}_{timestamp}.dwg";
            string outputPath = Path.Combine(outputFolder, fileName);

            // Phase Timings
            var swAcquire = new Stopwatch();
            var swOpen = new Stopwatch();
            var swSave = new Stopwatch();
            var swClose = new Stopwatch();
            CadOperationTimes innerTimes = new CadOperationTimes();

            SimpleLogger.Log("BonnetFlange", "--- BONNET GENERATION ---");
            SimpleLogger.Log("BonnetFlange", $"Template: {templatePath}");
            SimpleLogger.Log("BonnetFlange", $"Output: {outputPath}");

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
                SimpleLogger.Log("BonnetFlange", "Copy SUCCESS");
            }
            catch (Exception ex)
            {
                SimpleLogger.Log("BonnetFlange", $"Copy FAILED: {ex.Message}");
                throw;
            }

            // 2. Connect to CAD & Update
            swAcquire.Start();
            using (ICadAdapter cadAdapter = new GstarCadAdapter())
            {
                swAcquire.Stop();
                
                swOpen.Start();
                cadAdapter.OpenDrawing(outputPath);
                SimpleLogger.Log("BonnetFlange", "Open SUCCESS");
                swOpen.Stop();

                var engine = new AnnotationEngine(cadAdapter);
                innerTimes = engine.ProcessAnnotations(replacements);
                
                // Update Title Block
                cadAdapter.UpdateTitleBlockAttributes(drawInfo);
                SimpleLogger.Log("BonnetFlange", "Annotation SUCCESS");

                swSave.Start();
                cadAdapter.Save();
                SimpleLogger.Log("BonnetFlange", "Save SUCCESS");
                swSave.Stop();
                
                swClose.Start();
            }
            swClose.Stop();
            SimpleLogger.Log("BonnetFlange", "Close SUCCESS");

            SimpleLogger.Log("BonnetFlange", $"Output: {outputPath}");
            SimpleLogger.Log("BonnetFlange", "Finished");
            
            // Output timing breakdown exactly as requested
            long totalMs = swAcquire.ElapsedMilliseconds + swOpen.ElapsedMilliseconds + 
                           innerTimes.ScanTimeMs + innerTimes.ReplaceTimeMs + 
                           swSave.ElapsedMilliseconds + swClose.ElapsedMilliseconds;

            SimpleLogger.Log("BonnetFlange", "--- TIMING BREAKDOWN ---");
            SimpleLogger.Log("BonnetFlange", $"Acquire CAD........ {swAcquire.ElapsedMilliseconds} ms");
            SimpleLogger.Log("BonnetFlange", $"Open Drawing....... {swOpen.ElapsedMilliseconds} ms");
            SimpleLogger.Log("BonnetFlange", $"Scan Entities...... {innerTimes.ScanTimeMs} ms");
            SimpleLogger.Log("BonnetFlange", $"Replace............ {innerTimes.ReplaceTimeMs} ms");
            SimpleLogger.Log("BonnetFlange", $"Save Drawing....... {swSave.ElapsedMilliseconds} ms");
            SimpleLogger.Log("BonnetFlange", $"Close Drawing...... {swClose.ElapsedMilliseconds} ms");
            SimpleLogger.Log("BonnetFlange", $"Total.............. {totalMs} ms");
            SimpleLogger.Log("BonnetFlange", "------------------------");
            
            return outputPath;
        }
    }
}
