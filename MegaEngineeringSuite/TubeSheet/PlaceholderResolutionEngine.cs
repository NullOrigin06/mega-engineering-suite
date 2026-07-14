using System;

namespace MegaEngineeringSuite.TubeSheet
{
    public class PlaceholderResolutionEngine
    {
        public ReplacementPlan Resolve(PlaceholderIndex index, IEnumerable<SchemaDefinition> activeDefinitions, IReadOnlyDictionary<string, string> replacementDictionary)
        {
            var plan = new ReplacementPlan();
            int order = 0;

            var schemaDict = activeDefinitions.ToDictionary(d => d.PlaceholderName, d => d, StringComparer.OrdinalIgnoreCase);

            foreach (var descriptor in index.Enumerate())
            {
                string writableProperty = "TextString";
                if (schemaDict.TryGetValue(descriptor.PlaceholderName, out var def))
                {
                    writableProperty = def.WritableProperty;
                }

                if (replacementDictionary.TryGetValue(descriptor.PlaceholderName, out string replacementValue))
                {
                    var instruction = new ReplacementInstruction
                    {
                        Handle = descriptor.EntityHandle,
                        Placeholder = descriptor.PlaceholderName,
                        CurrentValue = descriptor.PlaceholderName, // Usually identical to name initially
                        ReplacementValue = replacementValue,
                        ExpectedEntityType = descriptor.EntityType,
                        Layer = descriptor.Layer,
                        WritableProperty = writableProperty,
                        Ready = true,
                        ValidationState = "Resolved",
                        ExecutionOrder = order++
                    };
                    plan.AddInstruction(instruction);
                }
                else
                {
                    // Even if we couldn't resolve it, we might add it to the plan as unready to fail validation later,
                    // or we ignore it if it's optional. Let's add it so the plan validator can catch unresolved mandatory items.
                    var instruction = new ReplacementInstruction
                    {
                        Handle = descriptor.EntityHandle,
                        Placeholder = descriptor.PlaceholderName,
                        CurrentValue = descriptor.PlaceholderName,
                        ReplacementValue = string.Empty,
                        ExpectedEntityType = descriptor.EntityType,
                        Layer = descriptor.Layer,
                        WritableProperty = writableProperty,
                        Ready = false,
                        ValidationState = "Unresolved",
                        ExecutionOrder = order++
                    };
                    plan.AddInstruction(instruction);
                }
            }

            plan.LockPlan();
            return plan;
        }
    }
}
