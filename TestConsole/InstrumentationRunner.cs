using System;
using System.IO;
using System.Collections.Generic;
using MegaEngineeringSuite;
using MegaEngineeringSuite.HeatExchangerFab;
using MegaEngineeringSuite.Infrastructure.Logging;
using MegaEngineeringSuite.Infrastructure.Cad;

namespace TestConsole
{
    public static class InstrumentationRunner
    {
        public static void RunDiagnostic()
        {
            Console.WriteLine("=================================================");
            Console.WriteLine("HEAT EXCHANGER FABRICATION PIPELINE INSTRUMENTATION");
            Console.WriteLine("=================================================");

            // 1. Log Formatter Production
            var testData = new HeatExchangerFabData();
            var replacements = HeatExchangerFabFormatter.Format(testData);

            Console.WriteLine($"\n--- STEP 1: FORMATTER PRODUCED {replacements.Count} PLACEHOLDERS ---");
            SimpleLogger.Log("Instrumentation", $"Formatter Dictionary Count: {replacements.Count}");
            foreach (var kvp in replacements)
            {
                Console.WriteLine($"  [PLACEHOLDER] Key: '{kvp.Key}' => Value: '{kvp.Value}'");
                SimpleLogger.Log("Instrumentation", $"Key: '{kvp.Key}' => Value: '{kvp.Value}'");
            }

            // 2. Locate DWG Template
            string projectRoot = System.IO.Path.GetFullPath(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\.."));
            string templatePath = System.IO.Path.Combine(projectRoot, @"Templates\Heat_Exchanger_Fabrication_template.dwg");
            if (!System.IO.File.Exists(templatePath))
            {
                // Fallback or skip
            }

            Console.WriteLine($"\n--- STEP 2: TEMPLATE PATH ---");
            Console.WriteLine($"  Path: {templatePath}");
            SimpleLogger.Log("Instrumentation", $"Using Template Path: {templatePath}");

            // 3. Connect to GstarCAD via COM
            Console.WriteLine($"\n--- STEP 3: CONNECTING TO GSTARCAD COM ---");
            var type = Type.GetTypeFromProgID("GstarCAD.Application");
            if (type == null)
            {
                Console.WriteLine("  ERROR: GstarCAD COM ProgID not found!");
                return;
            }

            dynamic acadApp = Activator.CreateInstance(type);
            acadApp.Visible = true;

            string testOutputPath = Path.Combine(Path.GetTempPath(), $"Test_HE_Fab_{DateTime.Now:yyyyMMdd_HHmmss}.dwg");
            File.Copy(templatePath, testOutputPath, true);
            File.SetAttributes(testOutputPath, FileAttributes.Normal);

            Console.WriteLine($"  Copied template to: {testOutputPath}");
            SimpleLogger.Log("Instrumentation", $"Test Output Path: {testOutputPath}");

            dynamic doc = acadApp.Documents.Open(testOutputPath, false);
            Console.WriteLine($"  Opened Document: {doc.Name}");

            // 4. Enumerating Layouts and Entities
            Console.WriteLine($"\n--- STEP 4: ENUMERATING TEXT & MTEXT ENTITIES ---");
            int totalEntitiesScanned = 0;
            int totalMatchesFound = 0;
            int totalReplacedCount = 0;

            dynamic layouts = doc.Layouts;
            for (int l = 0; l < layouts.Count; l++)
            {
                dynamic layout = layouts.Item(l);
                string layoutName = layout.Name;
                Console.WriteLine($"\n  === Layout: {layoutName} ===");
                SimpleLogger.Log("Instrumentation", $"Scanning Layout: {layoutName}");

                dynamic block = layout.Block;
                for (int i = 0; i < block.Count; i++)
                {
                    dynamic entity = block.Item(i);
                    string entityName = entity.EntityName;

                    string? rawText = null;
                    string? propName = null;

                    if (entityName.Contains("Dimension"))
                    {
                        try { rawText = entity.TextOverride; propName = "TextOverride"; } catch { }
                    }
                    else if (entityName.Contains("MText") || entityName.Contains("Text") || entityName.Contains("MLeader"))
                    {
                        try { rawText = entity.TextString; propName = "TextString"; } catch { }
                    }

                    if (rawText != null)
                    {
                        totalEntitiesScanned++;
                        Console.WriteLine($"    [{entityName}] Layer: {entity.Layer} | RawText: \"{rawText.Replace("\n", "\\n").Replace("\r", "\\r")}\"");
                        SimpleLogger.Log("Instrumentation", $"[{entityName}] Layer: {entity.Layer} | RawText: \"{rawText}\"");

                        string currentText = rawText;
                        bool entityModified = false;
                        List<string> matchedKeys = new List<string>();

                        foreach (var kvp in replacements)
                        {
                            if (currentText.Contains(kvp.Key))
                            {
                                totalMatchesFound++;
                                matchedKeys.Add(kvp.Key);
                                string beforeText = currentText;
                                currentText = currentText.Replace(kvp.Key, kvp.Value);
                                entityModified = true;

                                Console.WriteLine($"      ---> MATCH SUCCESS: Placeholder '{kvp.Key}' found!");
                                Console.WriteLine($"           Before: \"{beforeText}\"");
                                Console.WriteLine($"           After:  \"{currentText}\"");
                                SimpleLogger.Log("Instrumentation", $"MATCH: {kvp.Key} in {entityName} => {currentText}");
                            }
                        }

                        if (entityModified)
                        {
                            try
                            {
                                if (propName == "TextOverride") entity.TextOverride = currentText;
                                else if (propName == "TextString") entity.TextString = currentText;
                                totalReplacedCount++;
                                Console.WriteLine($"      [REPLACEMENT APPLIED & VERIFIED]");
                                SimpleLogger.Log("Instrumentation", $"Replaced entity text successfully.");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"      [ERROR WRITING PROPERTY]: {ex.Message}");
                                SimpleLogger.Log("Instrumentation", $"Error writing property: {ex.Message}");
                            }
                        }
                    }

                    // Also check block references with attributes
                    if (entityName == "AcDbBlockReference" && entity.HasAttributes)
                    {
                        var attrs = entity.GetAttributes();
                        foreach (dynamic attr in attrs)
                        {
                            string tag = attr.TagString;
                            string val = attr.TextString;
                            Console.WriteLine($"    [ATTRIB] Block: {entity.Name} | Tag: {tag} | Value: \"{val}\"");
                            SimpleLogger.Log("Instrumentation", $"[ATTRIB] Block: {entity.Name} | Tag: {tag} | Value: \"{val}\"");
                        }
                    }
                }
            }

            Console.WriteLine($"\n--- STEP 5: SAVING MODIFIED DRAWING ---");
            doc.Save();
            Console.WriteLine("  Drawing Saved Successfully!");
            SimpleLogger.Log("Instrumentation", "Drawing Saved Successfully.");

            doc.Close(false);
            acadApp.Quit();

            Console.WriteLine($"\n--- SUMMARY ---");
            Console.WriteLine($"  Total Entities Scanned: {totalEntitiesScanned}");
            Console.WriteLine($"  Total Placeholder Matches: {totalMatchesFound}");
            Console.WriteLine($"  Total Entities Modified: {totalReplacedCount}");
            Console.WriteLine("=================================================");
        }
    }
}
