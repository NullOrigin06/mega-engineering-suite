using System;
using System.Diagnostics;
using System.IO;
using MegaEngineeringSuite.Infrastructure.Cad;
using MegaEngineeringSuite.Infrastructure.Logging;

namespace MegaEngineeringSuite.TubeSheet
{
    public class AnnotationDiscoveryEngine
    {
        public void DiscoverAnnotations(PipelineContext context)
        {
            var sw = Stopwatch.StartNew();
            int foundCount = 0;
            int missingCount = 0; // Later for Phase B
            int duplicatesCount = 0;
            int malformedCount = 0;

            SimpleLogger.Log("AnnotationDiscovery", "Starting Phase A: Discovery ONLY.");
            
            // Log file path
            string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs", "PlaceholderDiscovery.log");
            Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);
            
            using (var writer = new StreamWriter(logPath, false))
            {
                writer.WriteLine("====================================");
                writer.WriteLine("Placeholder Discovery Log");
                writer.WriteLine($"Timestamp: {DateTime.Now}");
                writer.WriteLine("====================================");

                // Execute live COM discovery via CadAdapter (using All mode to find DIM layers)
                var livePlaceholders = context.CadAdapter.DiscoverPlaceholders(DiscoveryMode.All);
                
                var activeProfile = MigrationProfile.Stage10_BOM;
                var schema = new TubeSheetPlaceholderSchema();
                var activeNames = new System.Collections.Generic.HashSet<string>(
                    System.Linq.Enumerable.Select(schema.GetActiveProfileDefinitions(activeProfile), d => d.PlaceholderName), 
                    StringComparer.OrdinalIgnoreCase);

                foreach (var p in livePlaceholders)
                {
                    if (activeNames.Contains(p.PlaceholderName))
                    {
                        if (context.PlaceholderIndex.Contains(p.EntityHandle))
                        {
                            duplicatesCount++;
                        }
                        else
                        {
                            context.PlaceholderIndex.Add(p);
                            foundCount++;
                            
                            writer.WriteLine("------------------------------------");
                            writer.WriteLine($"Layer: {p.Layer}");
                            writer.WriteLine($"Name: {p.PlaceholderName}");
                            writer.WriteLine($"Handle: {p.EntityHandle}");
                            writer.WriteLine($"Type: {p.EntityType}");
                        }
                    }
                }

                sw.Stop();
                
                writer.WriteLine("------------------------------------");
                writer.WriteLine("Summary");
                writer.WriteLine($"Placeholders Found: {foundCount}");
                writer.WriteLine($"Missing: {missingCount}");
                writer.WriteLine($"Duplicates: {duplicatesCount}");
                writer.WriteLine($"Malformed: {malformedCount}");
                writer.WriteLine($"Execution Time: {sw.ElapsedMilliseconds}ms");
            }
            
            SimpleLogger.Log("AnnotationDiscovery", $"Discovery complete. Found {foundCount} placeholders. Log written to {logPath}");
        }
    }
}
