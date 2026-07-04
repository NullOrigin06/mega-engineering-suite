using System;
using System.IO;

namespace MegaEngineeringSuite.Infrastructure.Logging
{
    public static class SimpleLogger
    {
        private static readonly string LogDirectory = AppConfigManager.LogsFolder;

        public static void Log(string module, string message)
        {
            try
            {
                if (!Directory.Exists(LogDirectory))
                {
                    Directory.CreateDirectory(LogDirectory);
                }

                string logFilePath = Path.Combine(LogDirectory, $"{module}.log");
                string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}";
                
                File.AppendAllText(logFilePath, logEntry);
            }
            catch
            {
                // Ignore logging errors so we don't crash the application
            }
        }
    }
}
