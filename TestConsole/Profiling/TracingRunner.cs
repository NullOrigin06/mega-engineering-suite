using System;
using MegaEngineeringSuite;
using MegaEngineeringSuite.Calculations;

namespace COMTestApp
{
    public static class TracingRunner
    {
        public static async System.Threading.Tasks.Task RunAsync()
        {
            Console.WriteLine("--- STAGE 12.0 RUNTIME PROFILING ---");
            MegaEngineeringSuite.Infrastructure.Cad.GstarCadAdapter.ResetCounters();
            
            var swTotal = System.Diagnostics.Stopwatch.StartNew();
            
            var data = new MegaEngineeringSuite.TubeSheet.TubeSheetData 
            { 
                OutsideDiameter = 1070,
                InsideDiameter = 800,
                StepOutsideDiameter = 1070,
                Thickness = 25,
                TubeSheetFinishOD = 1070, 
                TubeSheetFinishTHK = 25, 
                TubeSheetWeight = 238.1
            };

            var info = new MegaEngineeringSuite.DrawingInformation
            {
                Title = "TUBE SHEET",
                CustomerName = "ACME CORP",
                ProjectNo = "PRJ-123",
                DrawingNo = "DWG-456",
                Revision = "0",
                PreparedBy = "ENG",
                CheckedBy = "MGR",
                ApprovedBy = "DIR",
                Date = DateTime.Now
            };

            var result = new DrawingAutomationResult
            {
                Arguments = @"C:\Users\PARTH\source\repos\MegaEngineeringSuite\GeneratedDrawings\Performance_Test.dwg"
            };

            // Note: We need a real CAD session. We will start one if needed.
            dynamic acadApp = MegaEngineeringSuite.Infrastructure.Cad.CadSessionManager.Instance.GetCadApplication();
            if (acadApp == null)
            {
                Console.WriteLine("Failed to get CAD Application.");
                return;
            }

            string templatePath = @"C:\Users\PARTH\source\repos\MegaEngineeringSuite\Templates\FINAL TUBESHEET.dwg";
            var orchestrator = new MegaEngineeringSuite.TubeSheet.PipelineOrchestrator();

            string logPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs", "StabilityReport.csv");
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(logPath));
            using (var writer = new System.IO.StreamWriter(logPath, false))
            {
                writer.WriteLine("Run,Success,Runtime(ms),GcadRAM(MB),GcadHandles,OD,Thickness");

                for (int i = 1; i <= 10; i++)
                {
                    Console.WriteLine($"\n--- RUN {i} ---");
                    MegaEngineeringSuite.Infrastructure.Cad.GstarCadAdapter.ResetCounters();
                    MegaEngineeringSuite.TubeSheet.RuntimeTraceLogger.Clear();

                    string testPath = $@"C:\Users\PARTH\source\repos\MegaEngineeringSuite\GeneratedDrawings\Stability_Test_{i}.dwg";
                    System.IO.File.Copy(templatePath, testPath, true);

                    var runData = new MegaEngineeringSuite.TubeSheet.TubeSheetData
                    {
                        OutsideDiameter = 1000 + (i * 10),
                        InsideDiameter = 800,
                        StepOutsideDiameter = 1000 + (i * 10),
                        Thickness = 20 + i,
                        TubeSheetFinishOD = 1000 + (i * 10),
                        TubeSheetFinishTHK = 20 + i,
                        TubeSheetWeight = 200 + i
                    };

                    var runInfo = new MegaEngineeringSuite.DrawingInformation
                    {
                        Title = $"TUBE SHEET RUN {i}",
                        CustomerName = "ACME CORP",
                        ProjectNo = $"PRJ-{i}",
                        DrawingNo = $"DWG-{i}",
                        Revision = "0",
                        PreparedBy = "ENG",
                        CheckedBy = "MGR",
                        ApprovedBy = "DIR",
                        Date = DateTime.Now
                    };

                    var resultN = new DrawingAutomationResult
                    {
                        Arguments = testPath,
                        CadApplication = acadApp,
                        CadDocument = acadApp.Documents.Open(testPath)
                    };

                    var swRun = System.Diagnostics.Stopwatch.StartNew();
                    bool success = await orchestrator.RunV2PipelineAsync(resultN, runData, runInfo);
                    swRun.Stop();
                    
                    var gcadProc = System.Diagnostics.Process.GetProcessesByName("gcad");
                    long ramMb = 0;
                    int handles = 0;
                    if (gcadProc.Length > 0)
                    {
                        ramMb = gcadProc[0].WorkingSet64 / (1024 * 1024);
                        handles = gcadProc[0].HandleCount;
                    }

                    writer.WriteLine($"{i},{success},{swRun.ElapsedMilliseconds},{ramMb},{handles},{runData.OutsideDiameter},{runData.Thickness}");
                    Console.WriteLine($"Run {i}: {success}, Time: {swRun.ElapsedMilliseconds} ms, RAM: {ramMb} MB, Handles: {handles}");
                }
            }
            swTotal.Stop();
        }
    }
}
