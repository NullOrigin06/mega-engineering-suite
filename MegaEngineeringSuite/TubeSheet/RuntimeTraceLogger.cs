using System;
using System.Text;
using System.IO;

namespace MegaEngineeringSuite.TubeSheet
{
    public static class RuntimeTraceLogger
    {
        private static StringBuilder _trace = new StringBuilder();

        public static void Log(string message)
        {
            _trace.AppendLine(message);
        }

        public static void LogPhaseStart(string phaseName)
        {
            Log($"[{DateTime.Now:HH:mm:ss.fff}] START PHASE: {phaseName}");
        }

        public static void LogPhaseEnd(string phaseName, string result, long durationMs)
        {
            Log($"[{DateTime.Now:HH:mm:ss.fff}] END PHASE: {phaseName} ........ {result} (Duration: {durationMs}ms)\n");
        }

        public static void DumpTrace()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs", "Stage8_7_RuntimeTrace.md");
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, _trace.ToString());
            _trace.Clear();
        }
    }
}
