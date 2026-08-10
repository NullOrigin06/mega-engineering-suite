using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using MegaEngineeringSuite;
using MegaEngineeringSuite.BonnetFlange;
using MegaEngineeringSuite.HeatExchangerFab;
using MegaEngineeringSuite.Infrastructure.Cad;
using MegaEngineeringSuite.Infrastructure.Logging;

namespace TestConsole
{
    public static class ForensicDiagnosticRunner
    {
        public static void RunIntermittentTelemetrySuite()
        {
            Console.WriteLine("==========================================================================================");
            Console.WriteLine("STAGE 20 VERIFICATION: BODY FLANGE, TUBE SHEET & HEAT EXCHANGER GENERATION");
            Console.WriteLine("==========================================================================================");

            // 1. Verify Configuration Path Normalization
            Console.WriteLine("\n>>> 1. CONFIGURATION NORMALIZATION CHECK <<<");
            Console.WriteLine($"  RootFolder:                 \"{AppConfigManager.RootFolder}\"");
            Console.WriteLine($"  BonnetTemplatePath:         \"{AppConfigManager.Current.BonnetTemplatePath}\"");
            Console.WriteLine($"  BonnetOutputFolder:         \"{AppConfigManager.Current.BonnetOutputFolder}\"");
            Console.WriteLine($"  HeatExchangerTemplatePath:  \"{AppConfigManager.Current.HeatExchangerTemplatePath}\"");
            Console.WriteLine($"  HeatExchangerOutputFolder:  \"{AppConfigManager.Current.HeatExchangerOutputFolder}\"");
            Console.WriteLine($"  DwgTemplatePath:            \"{AppConfigManager.Current.DwgTemplatePath}\"");
            Console.WriteLine($"  ExcelTemplatePath:          \"{AppConfigManager.Current.ExcelTemplatePath}\"");

            bool allRooted = Path.IsPathRooted(AppConfigManager.Current.BonnetTemplatePath) &&
                             Path.IsPathRooted(AppConfigManager.Current.BonnetOutputFolder) &&
                             Path.IsPathRooted(AppConfigManager.Current.HeatExchangerTemplatePath) &&
                             Path.IsPathRooted(AppConfigManager.Current.HeatExchangerOutputFolder) &&
                             Path.IsPathRooted(AppConfigManager.Current.DwgTemplatePath) &&
                             Path.IsPathRooted(AppConfigManager.Current.ExcelTemplatePath);

            Console.WriteLine($"  All Paths Fully Rooted/Normalized: {allRooted}");

            // 2. Body Flange Generation Test (End-to-End)
            Console.WriteLine("\n>>> 2. BODY FLANGE GENERATION (END-TO-END) <<<");
            for (int i = 1; i <= 3; i++)
            {
                TestBodyFlangeGeneration($"BF-Run-{i:D2}");
            }

            // 3. Heat Exchanger Fabrication Generation Test (End-to-End)
            Console.WriteLine("\n>>> 3. HEAT EXCHANGER FABRICATION GENERATION (END-TO-END) <<<");
            for (int i = 1; i <= 3; i++)
            {
                TestHeatExchangerGeneration($"HE-Run-{i:D2}");
            }

            // 4. Tube Sheet Verification
            Console.WriteLine("\n>>> 4. TUBE SHEET REGRESSION CHECK <<<");
            TestTubeSheetModule();

            Console.WriteLine("\n==========================================================================================");
            Console.WriteLine("ALL REGRESSION & END-TO-END TESTS COMPLETED");
            Console.WriteLine("==========================================================================================");
        }

        private static void TestBodyFlangeGeneration(string runId)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                var data = new BonnetFlangeData
                {
                    OD = 925,
                    ID = 812,
                    PCD = 875,
                    BoltHoleDia = 24,
                    BoltQty = 20,
                    Thickness = 40,
                    ShellID = 812,
                    LinerOD = 840,
                    LinerID = 812
                };

                var drawInfo = new DrawingInformation
                {
                    Title = "BODY FLANGE VERIFICATION",
                    CustomerName = "RELIANCE QA",
                    ProjectNo = "PRJ-BF-01",
                    DrawingNo = $"DWG-BF-{runId}"
                };

                var generator = new BonnetFlangeGenerator();
                string outputPath = generator.Generate(data, drawInfo);

                sw.Stop();
                bool exists = File.Exists(outputPath);
                long len = exists ? new FileInfo(outputPath).Length : 0;
                Console.WriteLine($"  ✔ [{runId}] SUCCESS in {sw.ElapsedMilliseconds} ms | File: {Path.GetFileName(outputPath)} ({len} bytes)");
            }
            catch (Exception ex)
            {
                sw.Stop();
                Console.WriteLine($"  ✖ [{runId}] FAILED in {sw.ElapsedMilliseconds} ms | Exception: {ex.GetType().Name}: {ex.Message}");
            }
        }

        private static void TestHeatExchangerGeneration(string runId)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                var data = new HeatExchangerFabData();
                var drawInfo = new DrawingInformation
                {
                    Title = "HE FABRICATION VERIFICATION",
                    CustomerName = "RELIANCE QA",
                    ProjectNo = "PRJ-HE-01",
                    DrawingNo = $"DWG-HE-{runId}"
                };

                var generator = new HeatExchangerFabGenerator();
                string outputPath = generator.Generate(data, drawInfo);

                sw.Stop();
                bool exists = File.Exists(outputPath);
                long len = exists ? new FileInfo(outputPath).Length : 0;
                Console.WriteLine($"  ✔ [{runId}] SUCCESS in {sw.ElapsedMilliseconds} ms | File: {Path.GetFileName(outputPath)} ({len} bytes)");
            }
            catch (Exception ex)
            {
                sw.Stop();
                Console.WriteLine($"  ✖ [{runId}] FAILED in {sw.ElapsedMilliseconds} ms | Exception: {ex.GetType().Name}: {ex.Message}");
            }
        }

        private static void TestTubeSheetModule()
        {
            try
            {
                var lookup = new ExcelLookupService();
                var data = lookup.LoadByShellId(168);
                if (data != null)
                {
                    Console.WriteLine($"  ✔ Tube Sheet Excel Lookup: SUCCESS (ShellID: {data.ShellID}, FinishOD: {data.TubeSheetFinishOD})");
                }
                else
                {
                    Console.WriteLine("  ✖ Tube Sheet Excel Lookup: FAILED");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ✖ Tube Sheet Exception: {ex.Message}");
            }
        }
    }
}
