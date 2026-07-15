using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MegaEngineeringSuite.TubeSheet
{
    public class PlaceholderSchemaValidator
    {
        public ValidationReport ValidateSchema(PlaceholderIndex index, IEnumerable<SchemaDefinition> activeDefinitions)
        {
            var sw = Stopwatch.StartNew();
            var report = new ValidationReport { ValidationStage = "Schema Validation" };

            var schemaDefinitions = activeDefinitions.ToList();
            var discoveredPlaceholders = index.Enumerate().ToList();

            var discoveredNames = new HashSet<string>(discoveredPlaceholders.Select(p => p.PlaceholderName), System.StringComparer.OrdinalIgnoreCase);

            var schemaNames = new HashSet<string>(schemaDefinitions.Select(d => d.PlaceholderName), System.StringComparer.OrdinalIgnoreCase);
            
            RuntimeTraceLogger.Log("\n### Schema Validation Trace");
            RuntimeTraceLogger.Log($"Active Schema Definitions Count: {schemaDefinitions.Count}");
            RuntimeTraceLogger.Log($"Discovered Placeholders Count: {discoveredPlaceholders.Count}");
            
            RuntimeTraceLogger.Log("\n#### Active Schema Definitions:");
            foreach (var s in schemaDefinitions)
            {
                RuntimeTraceLogger.Log($"- {s.PlaceholderName} (Required: {s.Required}, Layers: {string.Join(", ", s.AllowedLayers)}, Types: {string.Join(", ", s.AllowedEntityTypes)})");
            }
            
            RuntimeTraceLogger.Log("\n#### Discovered Placeholders:");
            foreach (var p in discoveredPlaceholders)
            {
                RuntimeTraceLogger.Log($"- {p.PlaceholderName} (Handle: {p.EntityHandle})");
            }

            RuntimeTraceLogger.Log("\n#### Schema Comparisons:");

            foreach (var definition in schemaDefinitions)
            {
                RuntimeTraceLogger.Log($"\nEvaluating Schema Placeholder: {definition.PlaceholderName}");
                // Verify Required exist
                if (definition.Required && !discoveredNames.Contains(definition.PlaceholderName))
                {
                    RuntimeTraceLogger.Log($"- RESULT: MISSING REQUIRED");
                    report.AddError($"Missing required placeholder: {definition.PlaceholderName}");
                    report.MissingRequired.Add(definition.PlaceholderName);
                    continue; // No point checking layer/type if it doesn't exist
                }

                if (!definition.Required && !discoveredNames.Contains(definition.PlaceholderName))
                {
                    RuntimeTraceLogger.Log($"Placeholder Missing: {definition.PlaceholderName}");
                    RuntimeTraceLogger.Log("Replacement skipped.");
                    report.MissingOptional.Add(definition.PlaceholderName);
                    continue;
                }

                var physicalInstances = discoveredPlaceholders.Where(p => p.PlaceholderName.Equals(definition.PlaceholderName, System.StringComparison.OrdinalIgnoreCase)).ToList();

                foreach (var instance in physicalInstances)
                {
                    RuntimeTraceLogger.Log($"- Found Instance Handle: {instance.EntityHandle}");
                    // Layer correctness
                    bool layerMatch = definition.AllowedLayers.Count == 0 || definition.AllowedLayers.Any(l => l.Equals(instance.Layer, System.StringComparison.OrdinalIgnoreCase));
                    RuntimeTraceLogger.Log($"  - Layer Match (Allowed [{string.Join(", ", definition.AllowedLayers)}] vs Found {instance.Layer}): {(layerMatch ? "PASS" : "FAIL")}");
                    if (!layerMatch)
                    {
                        report.AddError($"Placeholder {instance.PlaceholderName} is on wrong layer. Allowed: {string.Join(", ", definition.AllowedLayers)}, found {instance.Layer}.");
                    }

                    // Entity type correctness
                    bool typeMatch = definition.AllowedEntityTypes.Count == 0 || definition.AllowedEntityTypes.Any(t => t.Equals(instance.EntityType, System.StringComparison.OrdinalIgnoreCase));
                    RuntimeTraceLogger.Log($"  - Type Match (Allowed [{string.Join(", ", definition.AllowedEntityTypes)}] vs Found {instance.EntityType}): {(typeMatch ? "PASS" : "FAIL")}");
                    if (!typeMatch)
                    {
                        report.AddError($"Placeholder {instance.PlaceholderName} entity type mismatch. Allowed: {string.Join(", ", definition.AllowedEntityTypes)}, found {instance.EntityType}.");
                    }
                }
            }

            // Unexpected placeholders
            RuntimeTraceLogger.Log("\n#### Unexpected Placeholders:");
            var unexpected = discoveredNames.Where(name => !schemaNames.Contains(name)).ToList();
            if (unexpected.Count == 0) RuntimeTraceLogger.Log("- None");
            foreach (var name in unexpected)
            {
                RuntimeTraceLogger.Log($"- UNEXPECTED: {name}");
                report.AddError($"Unexpected placeholder discovered not in schema: {name}");
                report.Unexpected.Add(name);
            }

            sw.Stop();
            report.ExecutionTimeMs = sw.ElapsedMilliseconds;

            if (!report.Success)
            {
                RuntimeTraceLogger.Log($"\n[ERROR] Schema Validation Failed with {report.Errors.Count} errors.");
            }

            return report;
        }
    }
}
