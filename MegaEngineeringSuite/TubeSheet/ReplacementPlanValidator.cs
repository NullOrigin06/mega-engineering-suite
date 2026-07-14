using System;
using System.Collections.Generic;
using System.Linq;

namespace MegaEngineeringSuite.TubeSheet
{
    public class ReplacementPlanValidator
    {
        public ValidationReport ValidatePlan(ReplacementPlan plan, IEnumerable<SchemaDefinition> activeDefinitions)
        {
            var report = new ValidationReport { ValidationStage = "Replacement Plan Validation" };

            var requiredPlaceholders = activeDefinitions
                                             .Where(d => d.Required)
                                             .Select(d => d.PlaceholderName)
                                             .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var handlesSeen = new HashSet<string>();
            var maxTextLength = 255; // arbitrary sanity check limit

            foreach (var instruction in plan.Instructions)
            {
                // 1. Duplicate handles
                if (!handlesSeen.Add(instruction.Handle))
                {
                    report.AddError($"Duplicate handle execution planned: {instruction.Handle}");
                }

                // 2. NULL or Empty strings for required items
                if (string.IsNullOrEmpty(instruction.ReplacementValue))
                {
                    if (requiredPlaceholders.Contains(instruction.Placeholder))
                    {
                        report.AddError($"Required placeholder {instruction.Placeholder} has an empty or null replacement value.");
                    }
                    else
                    {
                        report.AddWarning($"Optional placeholder {instruction.Placeholder} has an empty replacement value.");
                    }
                    instruction.ValidationState = "Failed";
                    instruction.Ready = false;
                }

                // 3. Maximum text length
                if (instruction.ReplacementValue?.Length > maxTextLength)
                {
                    report.AddError($"Replacement value for {instruction.Placeholder} exceeds maximum length of {maxTextLength} characters.");
                    instruction.ValidationState = "Failed";
                    instruction.Ready = false;
                }

                // 4. Invalid characters (basic check, e.g., unicode control chars)
                if (instruction.ReplacementValue != null && instruction.ReplacementValue.Any(c => char.IsControl(c) && c != '\r' && c != '\n'))
                {
                    report.AddError($"Invalid control characters found in replacement value for {instruction.Placeholder}.");
                    instruction.ValidationState = "Failed";
                    instruction.Ready = false;
                }

                if (instruction.Ready && instruction.ValidationState != "Failed")
                {
                    instruction.ValidationState = "Validated";
                }
            }

            plan.IsValidated = report.Success;
            return report;
        }
    }
}
