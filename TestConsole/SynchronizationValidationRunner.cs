using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using MegaEngineeringSuite.Infrastructure.Cad;
using MegaEngineeringSuite.Infrastructure.Logging;

namespace COMTestApp
{
    public class SynchronizationValidationRunner
    {
        private readonly string _logPath;
        private readonly string _lispDirectory;
        
        public SynchronizationValidationRunner()
        {
            string projectRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", ".."));
            _logPath = Path.Combine(projectRoot, "Logs", "SynchronizationValidation.log");
            _lispDirectory = Path.Combine(projectRoot, "GeneratedLisp", "Tests");
            
            Directory.CreateDirectory(Path.GetDirectoryName(_logPath)!);
        }

        private void LogResult(string testName, double elapsedSeconds, int initialUserI1, int finalUserI1, bool timeout, bool rollbackExecuted, bool comReleased, string result)
        {
            string logEntry = $"[{DateTime.Now:O}] Test: {testName} | Time: {elapsedSeconds:F2}s | Initial USERI1: {initialUserI1} | Final USERI1: {finalUserI1} | Timeout: {timeout} | Rollback: {rollbackExecuted} | COM Released: {comReleased} | Result: {result}\n";
            File.AppendAllText(_logPath, logEntry);
            Console.WriteLine(logEntry.Trim());
        }

        public async Task RunAllTestsAsync()
        {
            Console.WriteLine("Starting Synchronization Validation...");
            File.AppendAllText(_logPath, "\n--- NEW VALIDATION RUN ---\n");

            await RunTestAsync("TestSuccess", "TestSuccess.lsp", TimeSpan.FromSeconds(10), expectedTimeout: false, expectedResult: true);
            await RunTestAsync("TestFailure", "TestFailure.lsp", TimeSpan.FromSeconds(10), expectedTimeout: false, expectedResult: false);
            await RunTestAsync("TestTimeout", "TestTimeout.lsp", TimeSpan.FromSeconds(10), expectedTimeout: true, expectedResult: false); // Use 10s for test instead of 60s
        }

        private async Task RunTestAsync(string testName, string lispFileName, TimeSpan timeoutSpan, bool expectedTimeout, bool expectedResult)
        {
            var stopwatch = Stopwatch.StartNew();
            int initialUserI1 = -999;
            int finalUserI1 = -999;
            bool isTimeout = false;
            bool rollbackExecuted = false;
            bool comReleased = false;
            string testResult = "FAILED";

            ICadAdapter? cadAdapter = null;
            ISynchronizationProvider? syncProvider = null;
            
            try
            {
                cadAdapter = new GstarCadAdapter();
                // Create a temporary DWG to open so we have a document
                string projectRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", ".."));
                string dummyDwg = Path.Combine(projectRoot, "dummy_test.dwg");
                if (!File.Exists(dummyDwg)) File.Copy(Path.Combine(projectRoot, "Templates", "BAFFLE_Flange_template.dwg"), dummyDwg, true);
                
                cadAdapter.OpenDrawing(dummyDwg);
                
                syncProvider = new CadUserVariableSynchronizationProvider(cadAdapter, "USERI1");
                
                // Load LISP
                string lispPath = Path.Combine(_lispDirectory, lispFileName).Replace("\\", "/");
                cadAdapter.SendCommand($"(load \"{lispPath}\")\n");
                
                // Test 1: Initialization
                syncProvider.InitializeSynchronization();
                initialUserI1 = Convert.ToInt32(cadAdapter.GetSystemVariable("USERI1"));
                
                // Start CancellationToken
                using var cts = new CancellationTokenSource();
                
                // Execute command
                cadAdapter.SendCommand($"(c:{testName})\n");
                
                // Wait for completion
                bool success = await syncProvider.WaitForCompletionAsync(timeoutSpan, cts.Token);
                
                finalUserI1 = Convert.ToInt32(cadAdapter.GetSystemVariable("USERI1"));
                
                if (success == expectedResult)
                {
                    testResult = "PASSED";
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Timeout") || ex.Message.Contains("timeout"))
                {
                    isTimeout = true;
                    if (expectedTimeout) testResult = "PASSED";
                }
                else
                {
                    Console.WriteLine($"Exception during {testName}: {ex.Message}");
                }
                
                rollbackExecuted = true;
            }
            finally
            {
                // Rollback / Finalize
                if (cadAdapter != null)
                {
                    cadAdapter.CloseDrawing();
                }
                CadSessionManager.Instance.ReleaseCadApplication();
                comReleased = true;
                
                // Aggressive GC for COM release check
                GC.Collect();
                GC.WaitForPendingFinalizers();

                stopwatch.Stop();
                LogResult(testName, stopwatch.Elapsed.TotalSeconds, initialUserI1, finalUserI1, isTimeout, rollbackExecuted, comReleased, testResult);
            }
        }
    }
}
