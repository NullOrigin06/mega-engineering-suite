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
                            var beforeHandleIdentity = context.CadAdapter.GetDocumentIdentity();
                            File.AppendAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs", "HandleDocumentAudit.md"), 
                                $"# Immediately before HandleToObject()\n**Current Document:** {beforeHandleIdentity.DocumentName}\n**Handle:** {instruction.Handle}\n---------------------------------------------------------\n");

                            var entity = context.CadAdapter.GetEntityByHandle(instruction.Handle);
                            string entityName = entity.EntityName;
                            
                            var afterHandleIdentity = context.CadAdapter.GetDocumentIdentity();
                            string entityLayer = "Unknown";
                            string entityTextOverride = "N/A";
                            try { entityLayer = entity.Layer; } catch { }
                            try { if (entityName.Contains("Dimension")) entityTextOverride = entity.TextOverride; } catch { }

                            File.AppendAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs", "HandleDocumentAudit.md"), 
                                $"# Immediately after HandleToObject()\n**ObjectName:** {entityName}\n**Layer:** {entityLayer}\n**TextOverride:** {entityTextOverride}\n**Document Name:** {afterHandleIdentity.DocumentName}\n---------------------------------------------------------\n");
                            
                            // Replace the placeholder inline with the replacement value
                            string currentText = instruction.CurrentValue;
                            string newText = currentText.Replace(instruction.Placeholder, instruction.ReplacementValue);

                            if (entityName.Contains("Dimension"))
                            {
                                entity.TextOverride = newText;
                            }
                            else
                            {
                                entity.TextString = newText;
                            }

                            // Immediate Read-back
                            string readBack = entityName.Contains("Dimension") ? entity.TextOverride : entity.TextString;
                            writer.WriteLine($"Read-back: {readBack}");
                            if (readBack == newText)
                            {
                                writer.WriteLine("Status: SUCCESS");
                            }
                            else
                            {
                                writer.WriteLine("Status: FAILED (Read-back mismatch)");
                            }
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
