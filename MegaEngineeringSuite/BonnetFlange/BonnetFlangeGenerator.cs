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

            // 2. Connect to CAD & Update
            swAcquire.Start();
            using (ICadAdapter cadAdapter = new GstarCadAdapter())
            {
                swAcquire.Stop();
                
                swOpen.Start();
                cadAdapter.OpenDrawing(templatePath);
                swOpen.Stop();

                var engine = new AnnotationEngine(cadAdapter);
                innerTimes = engine.ProcessAnnotations(replacements);
                
                // Update Title Block
                cadAdapter.UpdateTitleBlockAttributes(drawInfo);

                swSave.Start();
                cadAdapter.SaveAs(outputPath);
                swSave.Stop();
                
                swClose.Start();
            }
            swClose.Stop();

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
