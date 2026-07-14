using System.Collections.Generic;

namespace MegaEngineeringSuite.TubeSheet
{
    public class ReplacementInstruction
    {
        public string Handle { get; set; } = string.Empty;
        public string Placeholder { get; set; } = string.Empty;
        public string CurrentValue { get; set; } = string.Empty;
        public string ReplacementValue { get; set; } = string.Empty;
        public string ExpectedEntityType { get; set; } = string.Empty;
        public string Layer { get; set; } = string.Empty;
        public string WritableProperty { get; set; } = "TextString";
        public bool Ready { get; set; }
        public string ValidationState { get; set; } = "Pending";
        public int ExecutionOrder { get; set; }
    }

    public class ReplacementPlan
    {
        private readonly List<ReplacementInstruction> _instructions = new List<ReplacementInstruction>();
        public bool IsValidated { get; set; }
        public bool IsLocked { get; set; }

        public void AddInstruction(ReplacementInstruction instruction)
        {
            if (IsLocked) throw new System.InvalidOperationException("Cannot add instructions to a locked replacement plan.");
            _instructions.Add(instruction);
        }

        public IReadOnlyList<ReplacementInstruction> Instructions => _instructions.AsReadOnly();
        
        public void LockPlan()
        {
            IsLocked = true;
        }
    }
}
