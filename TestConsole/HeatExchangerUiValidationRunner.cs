using System;
using System.IO;
using System.Security.Cryptography;
using MegaEngineeringSuite;
using MegaEngineeringSuite.BonnetFlange;
using MegaEngineeringSuite.HeatExchangerFab;

namespace TestConsole
{
    public static class HeatExchangerUiValidationRunner
    {
        public static void RunAllTests()
        {
            Console.WriteLine("===============================================================");
            Console.WriteLine("HEAT EXCHANGER UI & EXTRAS ARCHITECTURAL VALIDATION SUITE");
            Console.WriteLine("===============================================================");

            string excelPath = AppConfigManager.Current.ExcelTemplatePath;
            if (string.IsNullOrEmpty(excelPath) || !File.Exists(excelPath))
            {
                // Fallback to relative path if not configured
                excelPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../../Templates/Heat Exchanger BOM Details.xlsx"));
                AppConfigManager.Current.ExcelTemplatePath = excelPath;
            }

            Console.WriteLine($"[INFO] Target Excel Template: {excelPath}");
            string hashBefore = CalculateSha256(excelPath);

            var lookupService = new ExcelLookupService();

            // 1. Initial Load Test for Shell ID = 273 (Row 5)
            Console.WriteLine("\n--- TEST 1: Initial Excel Read-Only Population ---");
            var data = lookupService.LoadByShellId(273);
            AssertEquals(273, data.ShellID, "Shell ID Initial");
            AssertEquals(4.0, data.DishendTHK, "Dishend THK Initial (Col 6)");
            AssertEquals(19.0, data.TubeSheetFinishTHK, "Tube Sheet Finish THK Initial");
            AssertEquals(16.0, data.HoleDia, "Hidden Hole Dia (Initial)");
            AssertEquals(335.0, data.BoltPCD, "Hidden Bolt PCD (Initial)");
            AssertEquals(315.0, data.LinerGasketOD, "Hidden Liner/Gasket OD (Initial)");
            Console.WriteLine("[PASS] Test 1: Initial Excel defaults correctly loaded.");

            // 2. Override Test: Modify Shell ID, TubeSheetFinishTHK, DishendTHK, Extras
            Console.WriteLine("\n--- TEST 2: UI Overrides & Active Value Snapshot ---");
            data.ShellID = 800;
            data.TubeSheetFinishTHK = 30.0;
            data.DishendTHK = 8.0;
            data.BonnetShellFSLength = 650.0;
            data.BonnetShellRSLength = 700.0;

            var mappedFab = HeatExchangerFabDataMapper.Map(data);
            AssertEquals(800.0, mappedFab.ShellID, "Mapped Shell ID Override");
            AssertEquals(30.0, mappedFab.TubeSheetTHK, "Mapped TubeSheetTHK Override");
            AssertEquals(8.0, mappedFab.DishendTHK, "Mapped DishendTHK Override");
            AssertEquals(650.0, mappedFab.BonnetShellFSLength, "Mapped BonnetShellFSLength Override");
            AssertEquals(700.0, mappedFab.BonnetShellRSLength, "Mapped BonnetShellRSLength Override");

            // Verify hidden parameters remained from Excel loaded values
            AssertEquals(16.0, mappedFab.HoleDia, "Retained Hidden Hole Dia");
            AssertEquals(335.0, mappedFab.BoltPCD, "Retained Hidden Bolt PCD");
            AssertEquals(315.0, mappedFab.LinerOD, "Retained Hidden Liner OD");
            Console.WriteLine("[PASS] Test 2: Active UI values properly override defaults while hidden parameters persist.");

            // 3. Formatter Token Verification
            Console.WriteLine("\n--- TEST 3: Formatter Token Replacement ---");
            var tokens = HeatExchangerFabFormatter.Format(mappedFab);
            AssertEquals("800", tokens["{{SHELL_ID}}"], "Token {{SHELL_ID}}");
            AssertEquals("30 THK.", tokens["{{TUBESHEET_THK}}"], "Token {{TUBESHEET_THK}}");
            AssertEquals("8 THK.", tokens["{{DISHEND_THK}}"], "Token {{DISHEND_THK}}");
            AssertEquals("650", tokens["{{BONNET_SHELL_FS_LENGTH}}"], "Token {{BONNET_SHELL_FS_LENGTH}}");
            AssertEquals("700", tokens["{{BONNET_SHELL_RS_LENGTH}}"], "Token {{BONNET_SHELL_RS_LENGTH}}");
            Console.WriteLine("[PASS] Test 3: Formatter tokens successfully generated from active UI overrides.");

            // 4. Body Flange & Tube Sheet Module Parity Check
            Console.WriteLine("\n--- TEST 4: Body Flange & Tube Sheet Regression Check ---");
            var mappedBonnet = BonnetFlangeDataMapper.Map(data);
            AssertEquals(22.0, mappedBonnet.Thickness, "Body Flange Thickness Parity");
            AssertEquals(800.0, mappedBonnet.ShellID, "Body Flange ShellID Parity");
            Console.WriteLine("[PASS] Test 4: Body Flange mapper remains 100% functional and compatible.");

            // 5. Excel File Integrity / Zero Write-Back Verification
            Console.WriteLine("\n--- TEST 5: Excel Immutability & Zero Write-Back Check ---");
            string hashAfter = CalculateSha256(excelPath);
            if (hashBefore != hashAfter)
            {
                throw new InvalidOperationException("[FAIL] CRITICAL REGRESSION: Excel workbook was modified!");
            }
            Console.WriteLine($"[PASS] Test 5: Excel hash matched perfectly ({hashAfter.Substring(0, 16)}...). Zero write-back guaranteed.");

            Console.WriteLine("\n===============================================================");
            Console.WriteLine("ALL ACCEPTANCE & INTEGRITY TESTS PASSED (0 ERRORS, 0 WARNINGS)");
            Console.WriteLine("===============================================================");
        }

        private static void AssertEquals(object expected, object actual, string label)
        {
            if (!Equals(expected, actual))
            {
                throw new InvalidOperationException($"Assertion Failed for '{label}': Expected '{expected}', Got '{actual}'");
            }
        }

        private static string CalculateSha256(string filePath)
        {
            using (var sha256 = SHA256.Create())
            using (var stream = File.OpenRead(filePath))
            {
                byte[] hash = sha256.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }
    }
}
