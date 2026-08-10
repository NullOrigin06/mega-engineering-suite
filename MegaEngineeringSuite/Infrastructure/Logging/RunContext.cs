using System;

namespace MegaEngineeringSuite.Infrastructure.Logging
{
    public static class RunContext
    {
        private static string _currentRunId = "N/A";

        public static string CurrentRunId => _currentRunId;

        public static string GenerateNewRunId()
        {
            string datePart = DateTime.Now.ToString("yyyyMMdd-HHmmss");
            string hashPart = Guid.NewGuid().ToString("N").Substring(0, 4).ToUpper();
            _currentRunId = $"RUN-{datePart}-{hashPart}";
            return _currentRunId;
        }

        public static void Clear()
        {
            _currentRunId = "N/A";
        }
    }
}
