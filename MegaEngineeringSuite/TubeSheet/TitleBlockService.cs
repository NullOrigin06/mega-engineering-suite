using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MegaEngineeringSuite.Infrastructure.Cad;
using MegaEngineeringSuite.Infrastructure.Logging;

namespace MegaEngineeringSuite.TubeSheet
{
    public class TitleBlockService
    {
        public void Execute(PipelineContext context)
        {
            var cadAdapter = context.CadAdapter;
            var info = context.Info;
            string artifactsDir = @"C:\Users\PARTH\.gemini\antigravity-ide\brain\1137cba5-f0e6-446f-ad1b-9447135c7a43\";

            SimpleLogger.Log("TitleBlockService", "Starting Title Block Phase");

            // 1. Discover using Cache
            var targetAttributes = context.TitleBlockCache;
            
            var discoveryLog = new List<string> { "# Stage 9 - Title Block Discovery (Cached)\n\n| Tag | Value | Handle |", "|---|---|---|" };
            foreach (var kvp in targetAttributes)
            {
                try
                {
                    string tag = kvp.Key;
                    string val = kvp.Value.TextString;
                    string handle = kvp.Value.Handle;
                    discoveryLog.Add($"| {tag} | {val.Replace("\n", "\\n").Replace("\r", "")} | {handle} |");
                }
                catch { }
            }
            File.WriteAllLines(Path.Combine(artifactsDir, "Stage9_TitleBlockDiscovery.md"), discoveryLog);

            if (targetAttributes.Count == 0)
            {
                SimpleLogger.Log("TitleBlockService", "WARNING: No Title Block attributes found in cache.");
                return;
            }

            // 2. Validate & Map
            var mappingProfile = new TitleBlockMappingProfile();
            var mappingLog = new List<string> { "# Stage 9 - Title Block Mapping\n\n| Expected Tag | Target Value | Discovered Attribute Handle | Status |", "|---|---|---|---|" };
            
            var replacementQueue = new List<(dynamic attr, string expectedTag, string targetValue)>();

            foreach (var mapping in mappingProfile.TagMappings)
            {
                string expectedTag = mapping.Key;
                string targetValue = mapping.Value(info);
                string tagUpper = expectedTag.ToUpper();

                if (targetAttributes.ContainsKey(tagUpper))
                {
                    dynamic discoveredAttr = targetAttributes[tagUpper];
                    string attrHandle = discoveredAttr.Handle;
                    mappingLog.Add($"| {expectedTag} | {targetValue.Replace("\n", "\\n").Replace("\r", "")} | {attrHandle} | MATCH |");
                    replacementQueue.Add((discoveredAttr, expectedTag, targetValue));
                }
                else
                {
                    mappingLog.Add($"| {expectedTag} | {targetValue.Replace("\n", "\\n").Replace("\r", "")} | N/A | **WARNING: MISSING** |");
                    SimpleLogger.Log("TitleBlockService", $"WARNING: Expected tag {expectedTag} not found in cache.");
                }
            }
            File.WriteAllLines(Path.Combine(artifactsDir, "Stage9_TitleBlockMapping.md"), mappingLog);

            // 3. Replace
            var replacementLog = new List<string> { "# Stage 9 - Title Block Replacement\n\n| Tag | Old Value | New Value | Read Back Value | Status |", "|---|---|---|---|---|" };
            var summaryLog = new List<string> { "# Stage 9 - Title Block Summary\n" };
            
            bool anyFailed = false;

            foreach (var item in replacementQueue)
            {
                string readBack = "";
                string status = "FAIL";
                try
                {
                    // Lazy Verification
                    string currentText = item.attr.TextString;
                    if (currentText != item.targetValue)
                    {
                        item.attr.TextString = item.targetValue;
                        MegaEngineeringSuite.Infrastructure.Cad.GstarCadAdapter.CounterEntityWrites++;
                        
                        readBack = item.attr.TextString;
                        MegaEngineeringSuite.Infrastructure.Cad.GstarCadAdapter.CounterEntityReads++;
                    }
                    else
                    {
                        readBack = item.targetValue;
                    }

                    if (readBack == item.targetValue)
                    {
                        status = (currentText != item.targetValue) ? "PASS" : "PASS (SKIPPED - MATCHES)";
                    }
                    else
                    {
                        status = "VERIFY_FAIL";
                        anyFailed = true;
                    }
                }
                catch (Exception ex)
                {
                    status = $"ERROR: {ex.Message}";
                    anyFailed = true;
                    SimpleLogger.Log("TitleBlockService", $"COM Error updating tag {item.expectedTag}: {ex.Message}");
                }

                string oldVal = "";
                try { oldVal = item.attr.TextString; } catch { }
                replacementLog.Add($"| {item.expectedTag} | {oldVal.Replace("\n", "\\n").Replace("\r", "")} | {item.targetValue.Replace("\n", "\\n").Replace("\r", "")} | {readBack.Replace("\n", "\\n").Replace("\r", "")} | {status} |");
                
                if (status.Contains("FAIL") || status.Contains("ERROR"))
                {
                    summaryLog.Add($"- ❌ **Failed to update:** `{item.expectedTag}`");
                }
                else
                {
                    summaryLog.Add($"- ✅ **Successfully updated:** `{item.expectedTag}` to `{item.targetValue.Replace("\n", "\\n").Replace("\r", "")}`");
                }
            }
            
            File.WriteAllLines(Path.Combine(artifactsDir, "Stage9_TitleBlockReplacement.md"), replacementLog);
            File.WriteAllLines(Path.Combine(artifactsDir, "Stage9_TitleBlockVerification.md"), replacementLog); // Verification is same data
            File.WriteAllLines(Path.Combine(artifactsDir, "Stage9_TitleBlockSummary.md"), summaryLog);

            if (anyFailed)
            {
                SimpleLogger.Log("TitleBlockService", "WARNING: One or more Title Block attributes failed verification.");
            }
            else
            {
                SimpleLogger.Log("TitleBlockService", "Title Block Phase completed successfully.");
            }
        }
    }
}

