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

            // 1. Discover
            var allAttributes = cadAdapter.DiscoverBlockAttributes("A2");
            var discoveryLog = new List<string> { "# Stage 9 - Title Block Discovery\n\n| Block Name | Block Handle | Attribute Handle | Tag | Value | Layout |", "|---|---|---|---|---|---|" };
            foreach (var attr in allAttributes)
            {
                discoveryLog.Add($"| {attr.BlockName} | {attr.BlockHandle} | {attr.AttributeHandle} | {attr.Tag} | {attr.Value.Replace("\n", "\\n").Replace("\r", "")} | {attr.Layout} |");
            }
            File.WriteAllLines(Path.Combine(artifactsDir, "Stage9_TitleBlockDiscovery.md"), discoveryLog);

            if (allAttributes.Count == 0)
            {
                SimpleLogger.Log("TitleBlockService", "WARNING: No A2 block found or it has no attributes.");
                return;
            }

            // Identify the unique block handles
            var blockHandles = allAttributes.Select(a => a.BlockHandle).Distinct().ToList();
            if (blockHandles.Count > 1)
            {
                SimpleLogger.Log("TitleBlockService", $"WARNING: Found {blockHandles.Count} A2 blocks. Using the first one in PaperSpace if possible.");
            }

            // Choose the target block (prefer Layout != "Model")
            string targetBlockHandle = blockHandles.FirstOrDefault(h => allAttributes.First(a => a.BlockHandle == h).Layout != "Model") ?? blockHandles.First();
            var targetAttributes = allAttributes.Where(a => a.BlockHandle == targetBlockHandle).ToList();

            // 2. Validate & Map
            var mappingProfile = new TitleBlockMappingProfile();
            var mappingLog = new List<string> { "# Stage 9 - Title Block Mapping\n\n| Expected Tag | Target Value | Discovered Attribute Handle | Status |", "|---|---|---|---|" };
            
            var replacementQueue = new List<(BlockAttributeDescriptor descriptor, string expectedTag, string targetValue)>();

            foreach (var mapping in mappingProfile.TagMappings)
            {
                string expectedTag = mapping.Key;
                string targetValue = mapping.Value(info);

                var discoveredAttr = targetAttributes.FirstOrDefault(a => a.Tag.Equals(expectedTag, StringComparison.OrdinalIgnoreCase));
                if (discoveredAttr != null)
                {
                    mappingLog.Add($"| {expectedTag} | {targetValue.Replace("\n", "\\n").Replace("\r", "")} | {discoveredAttr.AttributeHandle} | MATCH |");
                    replacementQueue.Add((discoveredAttr, expectedTag, targetValue));
                }
                else
                {
                    mappingLog.Add($"| {expectedTag} | {targetValue.Replace("\n", "\\n").Replace("\r", "")} | N/A | **WARNING: MISSING** |");
                    SimpleLogger.Log("TitleBlockService", $"WARNING: Expected tag {expectedTag} not found in A2 block.");
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
                    readBack = cadAdapter.UpdateBlockAttribute(item.descriptor.BlockHandle, item.expectedTag, item.targetValue);
                    if (readBack == item.targetValue)
                    {
                        status = "PASS";
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

                string safeOld = item.descriptor.Value.Replace("\n", "\\n").Replace("\r", "");
                string safeNew = item.targetValue.Replace("\n", "\\n").Replace("\r", "");
                string safeReadBack = readBack.Replace("\n", "\\n").Replace("\r", "");

                replacementLog.Add($"| {item.expectedTag} | {safeOld} | {safeNew} | {safeReadBack} | {status} |");

                summaryLog.Add($"### {item.expectedTag}");
                summaryLog.Add($"**Old:**\n{safeOld}\n");
                summaryLog.Add($"**New:**\n{safeNew}\n");
                if (status != "PASS") summaryLog.Add($"**Read Back:**\n{safeReadBack}\n");
                summaryLog.Add($"**Status:** {status}\n");
                summaryLog.Add("---");
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

