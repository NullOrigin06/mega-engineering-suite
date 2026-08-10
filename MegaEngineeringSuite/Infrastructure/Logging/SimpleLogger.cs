using System;
using System.IO;

namespace MegaEngineeringSuite.Infrastructure.Logging
{
    public static class SimpleLogger
    {
        public static string LogDirectory => Path.Combine(AppConfigManager.UserDataFolder, "Logs");

        public static void Log(string module, string message)
        {
            WriteLog("Runtime", module, message, null, null);
        }

        public static void LogGeneration(string module, string message)
        {
            WriteLog("Generation", module, message, null, null);
        }

        public static void LogCad(string module, string message)
        {
            WriteLog("CAD", module, message, null, null);
        }

        public static void LogError(string module, string errorCode, string message, Exception? ex = null)
        {
            WriteLog("Errors", module, message, errorCode, ex);
        }

        private static void WriteLog(string category, string module, string message, string? errorCode, Exception? ex)
        {
            try
            {
                string categoryDir = Path.Combine(LogDirectory, category);
                if (!Directory.Exists(categoryDir))
                {
                    Directory.CreateDirectory(categoryDir);
                }

                string logFilePath = Path.Combine(categoryDir, $"{module}_{DateTime.Now:yyyyMMdd}.log");
                
                string runId = RunContext.CurrentRunId;
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                
                var sb = new System.Text.StringBuilder();
                sb.AppendLine("------------------------------------------------------------");
                sb.AppendLine($"[TIME] {timestamp}");
                sb.AppendLine($"[RUN ID] {runId}");
                sb.AppendLine($"[MODULE] {module}");
                if (!string.IsNullOrEmpty(errorCode))
                    sb.AppendLine($"[ERROR CODE] {errorCode}");
                sb.AppendLine($"[MESSAGE] {message}");
                
                if (ex != null)
                {
                    sb.AppendLine($"[EXCEPTION] {ex.GetType().Name}");
                    sb.AppendLine($"[DETAILS] {ex.Message}");
                    sb.AppendLine($"[STACKTRACE]");
                    sb.AppendLine(ex.StackTrace);
                }

                File.AppendAllText(logFilePath, sb.ToString());
            }
            catch
            {
                // Ignore logging errors so we don't crash the application
            }
        }
    }
}
