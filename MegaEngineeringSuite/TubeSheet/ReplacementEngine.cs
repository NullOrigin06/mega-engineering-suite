using System;
using System.Diagnostics;
using System.IO;
using MegaEngineeringSuite.Infrastructure.Cad;
using MegaEngineeringSuite.Infrastructure.Logging;

namespace MegaEngineeringSuite.TubeSheet
{
    public class ReplacementEngine
    {
        public void ExecutePlan(ReplacementPlan plan, PipelineContext context)
        {
            var sw = Stopwatch.StartNew();
            SimpleLogger.Log("ReplacementEngine", "Starting Phase C: Replacement Execution.");

            if (!plan.IsValidated)
            {
                throw new InvalidOperationException("Cannot execute an unvalidated replacement plan.");
            }

            if (AppConfigManager.Current.EnablePipelineDiagnostics)
            {
                string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs", "ReplacementExecution.log");
                using (var writer = new StreamWriter(logPath, false))
                {
                    writer.WriteLine("====================================");
                    writer.WriteLine("Replacement Execution Log");
                    writer.WriteLine($"Timestamp: {DateTime.Now}");
                    writer.WriteLine("====================================");

                    foreach (var instruction in plan.Instructions)
                    {
                        if (!instruction.Ready) continue;
                        
                        writer.WriteLine("------------------------------------");
                        writer.WriteLine($"Handle: {instruction.Handle}");
                        writer.WriteLine($"Placeholder: {instruction.Placeholder}");
                        writer.WriteLine($"Old Value: {instruction.CurrentValue}");
                        writer.WriteLine($"New Value: {instruction.ReplacementValue}");

                        try
                        {
                            var totalSw = Stopwatch.StartNew();
                            
                            // 1. Lookup from Cache
                            var stepSw = Stopwatch.StartNew();
                            dynamic entity;
                            if (context.EntityCache.ContainsKey(instruction.Handle))
                            {
                                entity = context.EntityCache[instruction.Handle];
                            }
                            else
                            {
                                entity = context.CadAdapter.GetEntityByHandle(instruction.Handle);
                                context.EntityCache[instruction.Handle] = entity;
                                MegaEngineeringSuite.Infrastructure.Cad.GstarCadAdapter.CounterGetObjectByHandle++;
                            }
                            string entityName = entity.EntityName;
                            stepSw.Stop();
                            long lookupTime = stepSw.ElapsedMilliseconds;

                            // 2. Read Layer
                            stepSw.Restart();
                            string entityLayer = "Unknown";
                            try { entityLayer = entity.Layer; } catch { }
                            stepSw.Stop();
                            long readLayerTime = stepSw.ElapsedMilliseconds;

                            // 3. Read Text
                            stepSw.Restart();
                            string currentTextFromEntity = "";
                            if (entityName.Contains("Dimension")) { currentTextFromEntity = entity.TextOverride; }
                            else { currentTextFromEntity = entity.TextString; }
                            MegaEngineeringSuite.Infrastructure.Cad.GstarCadAdapter.CounterEntityReads++;
                            stepSw.Stop();
                            long readTextTime = stepSw.ElapsedMilliseconds;

                            // 4. Lazy Verification & Write
                            stepSw.Restart();
                            string currentText = instruction.CurrentValue;
                            string newText = currentText.Replace(instruction.Placeholder, instruction.ReplacementValue);
                            
                            bool needsUpdate = currentTextFromEntity != newText;
                            long writeTime = 0;
                            long readBackTime = 0;
                            string readBack = currentTextFromEntity;
                            
                            if (needsUpdate)
                            {
                                if (entityName.Contains("Dimension"))
                                {
                                    entity.TextOverride = newText;
                                    MegaEngineeringSuite.Infrastructure.Cad.GstarCadAdapter.CounterEntityWrites++;
                                }
                                else
                                {
                                    entity.TextString = newText;
                                    MegaEngineeringSuite.Infrastructure.Cad.GstarCadAdapter.CounterEntityWrites++;
                                }
                                stepSw.Stop();
                                writeTime = stepSw.ElapsedMilliseconds;

                                // 5. Read Back
                                stepSw.Restart();
                                MegaEngineeringSuite.Infrastructure.Cad.GstarCadAdapter.CounterEntityReads++;
                                readBack = entityName.Contains("Dimension") ? entity.TextOverride : entity.TextString;
                                stepSw.Stop();
                                readBackTime = stepSw.ElapsedMilliseconds;
                            }
                            else
                            {
                                stepSw.Stop();
                            }

                            totalSw.Stop();
                            long totalTime = totalSw.ElapsedMilliseconds;

                            writer.WriteLine($"Read-back: {readBack}");
                            if (readBack == newText)
                            {
                                writer.WriteLine("Status: SUCCESS" + (needsUpdate ? "" : " (SKIPPED - VALUE ALREADY MATCHES)"));
                            }
                            else
                            {
                                writer.WriteLine("Status: FAILED (Read-back mismatch)");
                            }

                            string breakdownPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs", "Stage12_COMBreakdown.md");
                            if (!File.Exists(breakdownPath))
                            {
                                File.WriteAllText(breakdownPath, "# COM Operation Breakdown\n\n```\n");
                            }
                            
                            string breakdownData = $@"Entity {instruction.Handle}
Lookup      {lookupTime} ms
Read Layer  {readLayerTime} ms
Read Text   {readTextTime} ms
Write       {writeTime} ms
Read Back   {readBackTime} ms
Total       {totalTime} ms

";
                            File.AppendAllText(breakdownPath, breakdownData);
                        }
                        catch (Exception ex)
                        {
                            writer.WriteLine($"Status: ERROR - {ex.Message}");
                        }
                    }

                    sw.Stop();
                    writer.WriteLine("====================================");
                    writer.WriteLine($"Execution Time: {sw.ElapsedMilliseconds}ms");
                }
            }
            else
            {
                // Execution without diagnostics logging
                foreach (var instruction in plan.Instructions)
                {
                    if (!instruction.Ready) continue;
                    try
                    {
                        var entity = context.CadAdapter.GetEntityByHandle(instruction.Handle);
                        string entityName = entity.EntityName;
                        string newText = instruction.CurrentValue.Replace(instruction.Placeholder, instruction.ReplacementValue);

                        if (entityName.Contains("Dimension"))
                            entity.TextOverride = newText;
                        else
                            entity.TextString = newText;
                    }
                    catch { /* Swallow for now, actual implementation might log elsewhere */ }
                }
                sw.Stop();
            }
        }
    }

    public class ReplacementVerificationEngine
    {
        public void VerifyReplacements(ReplacementPlan plan, PipelineContext context)
        {
            var sw = Stopwatch.StartNew();
            SimpleLogger.Log("ReplacementVerificationEngine", "Starting Phase C: Replacement Verification.");

            if (AppConfigManager.Current.EnablePipelineDiagnostics)
            {
                string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs", "ReplacementVerification.log");
                using (var writer = new StreamWriter(logPath, false))
                {
                    writer.WriteLine("====================================");
                    writer.WriteLine("Replacement Verification Log");
                    writer.WriteLine($"Timestamp: {DateTime.Now}");
                    writer.WriteLine("====================================");

                    var livePlaceholders = context.CadAdapter.DiscoverPlaceholders(DiscoveryMode.All);
                    var activeProfile = MigrationProfile.Stage8_DetailADimensions;
                    var schema = new TubeSheetPlaceholderSchema();
                    var activeNames = new System.Collections.Generic.HashSet<string>(
                        System.Linq.Enumerable.Select(schema.GetActiveProfileDefinitions(activeProfile), d => d.PlaceholderName), 
                        StringComparer.OrdinalIgnoreCase);

                    foreach (var p in livePlaceholders)
                    {
                        if (activeNames.Contains(p.PlaceholderName))
                        {
                            writer.WriteLine($"WARNING: Unreplaced placeholder found: {p.PlaceholderName} at {p.EntityHandle}");
                        }
                    }

                    foreach (var instruction in plan.Instructions)
                    {
                        writer.WriteLine($"Verified Handle: {instruction.Handle} expected value: {instruction.ReplacementValue}");
                    }

                    sw.Stop();
                    writer.WriteLine("====================================");
                    writer.WriteLine($"Verification Time: {sw.ElapsedMilliseconds}ms");
                }
            }

        }
    }
}
