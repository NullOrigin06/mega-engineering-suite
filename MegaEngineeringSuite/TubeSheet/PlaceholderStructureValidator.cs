using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MegaEngineeringSuite.TubeSheet
{
    public class PlaceholderStructureValidator
    {
        public ValidationReport ValidateStructure(PlaceholderIndex index, IEnumerable<SchemaDefinition> activeSchema)
        {
            var sw = Stopwatch.StartNew();
            var report = new ValidationReport { ValidationStage = "Structure Validation" };

            var allPlaceholders = index.Enumerate().ToList();

            // 1. Duplicate Handles
            var duplicateHandles = allPlaceholders.GroupBy(p => p.EntityHandle)
                                                  .Where(g => g.Count() > 1)
                                                  .Select(g => g.Key);
            foreach (var handle in duplicateHandles)
            {
                report.AddError($"Duplicate handle detected in index: {handle}");
                report.Duplicates.Add(handle);
            }

            // 1.5 Duplicate Placeholder Names
            var duplicateNames = allPlaceholders.GroupBy(p => p.PlaceholderName)
                                                .Where(g => g.Count() > 1)
                                                .Select(g => g.Key);
            foreach (var name in duplicateNames)
            {
                report.AddError($"Duplicate placeholder name detected: {name}. Only one authoritative placeholder is allowed per name.");
                report.Duplicates.Add(name);
            }

            var schemaDict = activeSchema.ToDictionary(s => s.PlaceholderName, s => s, System.StringComparer.OrdinalIgnoreCase);

            var validTypes = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase) 
            { 
                "Text", "MText", "Attribute", "Block Attribute", "AcDbText", "AcDbMText", "RotatedDimension", "AcDbRotatedDimension"
            };

            RuntimeTraceLogger.Log("\n### Structure Validation Trace");

            foreach (var p in allPlaceholders)
            {
                RuntimeTraceLogger.Log($"\n#### Validating Placeholder: {p.PlaceholderName}");
                RuntimeTraceLogger.Log($"- Handle: {p.EntityHandle}");
                RuntimeTraceLogger.Log($"- Entity Type: {p.EntityType}");
                RuntimeTraceLogger.Log($"- Layer: {p.Layer}");
                
                // Rule: Layer
                bool isLayerValid = p.Layer.StartsWith("META_", System.StringComparison.OrdinalIgnoreCase) || 
                                    p.Layer.Equals("DIM", System.StringComparison.OrdinalIgnoreCase) ||
                                    p.Layer.Equals("TEXT", System.StringComparison.OrdinalIgnoreCase);
                RuntimeTraceLogger.Log($"- Rule [Layer Valid (META_ or DIM)]: {(isLayerValid ? "PASS" : "FAIL")}");
                if (!isLayerValid)
                {
                    report.AddError($"Placeholder {p.PlaceholderName} (Handle: {p.EntityHandle}) is on an invalid layer: {p.Layer}");
                    report.Unexpected.Add(p.PlaceholderName);
                }

                // Rule: Entity Type
                bool isTypeValid = validTypes.Contains(p.EntityType);
                RuntimeTraceLogger.Log($"- Rule [Entity Type Valid]: {(isTypeValid ? "PASS" : "FAIL")}");
                if (!isTypeValid)
                {
                    report.AddError($"Placeholder {p.PlaceholderName} (Handle: {p.EntityHandle}) has an invalid entity type: {p.EntityType}");
                }

                // Rule: Syntax
                IdentifierStyle requiredStyle = IdentifierStyle.Bracketed;
                if (schemaDict.TryGetValue(p.PlaceholderName, out var def))
                {
                    requiredStyle = def.Style;
                }

                bool isSyntaxValid = true;
                if (requiredStyle == IdentifierStyle.Bracketed)
                {
                    if (!p.PlaceholderName.StartsWith("<") || !p.PlaceholderName.EndsWith(">"))
                    {
                        isSyntaxValid = false;
                        report.AddError($"Malformed placeholder syntax (Expected Bracketed): {p.PlaceholderName} (Handle: {p.EntityHandle})");
                        report.Malformed.Add(p.PlaceholderName);
                    }
                }
                else if (requiredStyle == IdentifierStyle.Flat)
                {
                    if (p.PlaceholderName.StartsWith("<") || p.PlaceholderName.EndsWith(">"))
                    {
                        isSyntaxValid = false;
                        report.AddError($"Malformed placeholder syntax (Expected Flat): {p.PlaceholderName} (Handle: {p.EntityHandle})");
                        report.Malformed.Add(p.PlaceholderName);
                    }
                }
                RuntimeTraceLogger.Log($"- Rule [Syntax Valid ({requiredStyle})]: {(isSyntaxValid ? "PASS" : "FAIL")}");
            }

            sw.Stop();
            report.ExecutionTimeMs = sw.ElapsedMilliseconds;

            if (!report.Success)
            {
                RuntimeTraceLogger.Log($"\n[ERROR] Structure Validation Failed with {report.Errors.Count} errors:");
                foreach(var err in report.Errors) RuntimeTraceLogger.Log(err);
            }

            return report;
        }
    }
}
