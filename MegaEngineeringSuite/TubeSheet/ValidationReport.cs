using System.Collections.Generic;

namespace MegaEngineeringSuite.TubeSheet
{
    public class ValidationReport
    {
        public bool Success { get; set; } = true;
        public string ValidationStage { get; set; } = string.Empty;
        public long ExecutionTimeMs { get; set; }

        public List<string> Warnings { get; set; } = new List<string>();
        public List<string> Errors { get; set; } = new List<string>();
        
        public List<string> MissingRequired { get; set; } = new List<string>();
        public List<string> MissingOptional { get; set; } = new List<string>();
        public List<string> Duplicates { get; set; } = new List<string>();
        public List<string> Unexpected { get; set; } = new List<string>();
        public List<string> Malformed { get; set; } = new List<string>();

        public void AddError(string error)
        {
            Errors.Add(error);
            Success = false;
        }

        public void AddWarning(string warning)
        {
            Warnings.Add(warning);
        }
    }
}
