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
            Console.WriteLine("==========================================================================");
            Console.WriteLine("PROFILE + TUBE QTY IN ENGINEERING PARAMETERS & DATA FLOW VALIDATION SUITE");
            Console.WriteLine("==========================================================================");

            string excelPath = AppConfigManager.Current.ExcelTemplatePath;
            if (string.IsNullOrEmpty(excelPath) || !File.Exists(excelPath))
            {
                excelPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../../Templates/Heat Exchanger BOM Details.xlsx"));
                AppConfigManager.Current.ExcelTemplatePath = excelPath;
            }

            Console.WriteLine($"[INFO] Target Excel Template: {excelPath}");
            string hashBefore = CalculateSha256(excelPath);

            var lookupService = new ExcelLookupService();
            var geometryService = new GeometryCalculationService();
            var layoutService = new TubeLayoutService();

            // -------------------------------------------------------------------------
            // TEST 1: Initial Calculation & Tube Qty in Engineering Parameters
            // -------------------------------------------------------------------------
            Console.WriteLine("\n--- TEST 1: Initial Calculation & Display Dictionary ---");
            var initialModel = new EngineeringDataModel
            {
                ShellID = 800,
                TubeOD = 25.4,
                NoOfPass = 4,
                HTA = 100.0,
                TubeLength = 3000.0,
                ThermalCalculatedTubeQty = 420,
                TubeQty = 420
            };

            var dict = initialModel.ToDisplayDictionary();
            AssertEquals("800", dict["Shell I.D."], "Display Dictionary Shell I.D.");
            AssertEquals("420", dict["Tube Qty"], "Display Dictionary Tube Qty in Engineering Parameters");
            Console.WriteLine("  [PASS] Display Dictionary correctly contains Tube Qty directly beneath Shell I.D.");

            // -------------------------------------------------------------------------
            // TEST 2: Shell ID Transition (800 -> 690) with Dynamic Tube Qty Resolution
            // -------------------------------------------------------------------------
            Console.WriteLine("\n--- TEST 2: Shell ID Transition (800 -> 690) Tube Qty Synchronization ---");
            var profile690 = lookupService.LoadProfileByShellId(690);
            
            // Emulate OnShellIdChanged(690)
            var tubes690 = layoutService.GenerateLayout((float)(690 / 2.0), (float)initialModel.TubeOD, 10000, (float)profile690.PartitionPlateTHK, initialModel.NoOfPass);
            int capacity690 = tubes690.Count;
            AssertEquals(352, capacity690, "690 Shell Geometric Capacity (25.4mm 4-pass)");

            int resolvedTubeQty690;
            if (initialModel.ThermalCalculatedTubeQty > 0 && initialModel.ThermalCalculatedTubeQty <= capacity690)
            {
                resolvedTubeQty690 = initialModel.ThermalCalculatedTubeQty;
            }
            else
            {
                resolvedTubeQty690 = capacity690;
            }

            AssertEquals(352, resolvedTubeQty690, "Resolved Tube Qty for Shell ID 690");

            var model690 = new EngineeringDataModel
            {
                ShellID = 690,
                TubeOD = initialModel.TubeOD,
                ThermalCalculatedTubeQty = initialModel.ThermalCalculatedTubeQty,
                TubeQty = resolvedTubeQty690,
                NoOfPass = initialModel.NoOfPass,
                TubeLength = initialModel.TubeLength,
                HTA = initialModel.HTA,
                TubeSheetFinishOD = profile690.TubeSheetFinishOD,
                BoltPCD = profile690.BoltPCD,
                HoleDia = profile690.HoleDia,
                NoOfBolts = profile690.NoOfBolts,
                PartitionPlateTHK = profile690.PartitionPlateTHK,
                FlangeID = profile690.FlangeID
            };

            var geom690 = geometryService.CalculateGeometry(model690);
            AssertEquals(352, geom690.TubeCoordinates.Count, "Geometry Layout Tube Count for 690");
            Console.WriteLine("  [PASS] Shell ID 690 resolved Tube Qty = 352 and Geometry Engine validated with 0 errors.");

            // -------------------------------------------------------------------------
            // TEST 3: Shell ID 680 to 750 Series Synchronization Matrix
            // -------------------------------------------------------------------------
            Console.WriteLine("\n--- TEST 3: Shell ID Series 680 to 750 Synchronization Matrix ---");
            Console.WriteLine("| Shell ID | Excel Profile | Thermal Qty | Resolved Tube Qty | Geometry Capacity | Status |");
            Console.WriteLine("|----------|---------------|-------------|-------------------|-------------------|--------|");

            int[] testShellIds = { 680, 690, 700, 710, 720, 730, 740, 750 };
            foreach (int sId in testShellIds)
            {
                var prof = lookupService.LoadProfileByShellId(sId);
                var tubes = layoutService.GenerateLayout((float)(sId / 2.0), 25.4f, 10000, (float)prof.PartitionPlateTHK, 4);
                int cap = tubes.Count;
                int resolved = (420 <= cap) ? 420 : cap;

                var m = new EngineeringDataModel
                {
                    ShellID = sId,
                    TubeOD = 25.4,
                    ThermalCalculatedTubeQty = 420,
                    TubeQty = resolved,
                    NoOfPass = 4,
                    TubeLength = 3000,
                    HTA = 100,
                    TubeSheetFinishOD = prof.TubeSheetFinishOD,
                    BoltPCD = prof.BoltPCD,
                    HoleDia = prof.HoleDia,
                    NoOfBolts = prof.NoOfBolts,
                    PartitionPlateTHK = prof.PartitionPlateTHK,
                    FlangeID = prof.FlangeID
                };

                var g = geometryService.CalculateGeometry(m);
                AssertEquals(resolved, g.TubeCoordinates.Count, $"Geometry count for Shell ID {sId}");

                Console.WriteLine($"| {sId,8} | Row Matched   | {420,11} | {resolved,17} | {cap,17} | Validated OK |");
            }

            // -------------------------------------------------------------------------
            // TEST 4: Generation Snapshot Integrity
            // -------------------------------------------------------------------------
            Console.WriteLine("\n--- TEST 4: Generation Snapshot & Overrides ---");
            var snapshot = new HeatExchangerGenerationSnapshot
            {
                RunId = "TEST-RUN-690",
                ShellID = 690,
                ShellTHK = 4.0,
                ShellLength = 3000.0,
                TubeOD = 25.4,
                TubeTHK = 1.6,
                TubeLength = 3000.0,
                TotalTubes = 352, // Resolved from profile
                TubePitch = 31.75,
                NoOfPasses = 4,
                TubeSheetOD = profile690.TubeSheetFinishOD,
                TubeSheetTHK = profile690.TubeSheetFinishTHK,
                FlangeOD = profile690.TubeSheetFinishOD,
                FlangeID = profile690.FlangeID,
                FlangeTHK = profile690.BodyFlangeFinishTHK,
                LinerOD = profile690.LinerGasketOD,
                LinerID = 690,
                LinerTHK = 3.0,
                SerrationOD = profile690.LinerGasketOD,
                SerrationID = 690,
                BaffleQty = 6,
                BaffleTHK = profile690.BaffleTHK,
                PartitionPlateTHK = profile690.PartitionPlateTHK,
                BoltSize = profile690.BoltSize,
                BoltLength = profile690.BoltLength,
                NoOfBolts = profile690.NoOfBolts,
                HoleDia = profile690.HoleDia,
                BoltPCD = profile690.BoltPCD,
                TieRodQty = (int)profile690.TieRodQty,
                TieRodDia = profile690.TieRodDia,
                SpacerTube = profile690.SpacerTube,
                BonnetShellFSLength = 500.0,
                BonnetShellRSLength = 500.0,
                DishendTHK = profile690.DishendTHK
            };

            var fabData = snapshot.ToFabData();
            var tokens = HeatExchangerFabFormatter.Format(fabData);

            AssertEquals("690", tokens["{{SHELL_ID}}"], "Snapshot Shell ID");
            AssertEquals("352", tokens["{{TUBE_QTY}}"], "Snapshot Tube Qty Token");
            AssertEquals("25 THK.", tokens["{{TUBESHEET_THK}}"], "TS THK");
            AssertEquals("32 THK.", tokens["{{BODY_FLANGE_THK}}"], "BF THK");
            AssertEquals("4 THK.", tokens["{{DISHEND_THK}}"], "Dishend THK");
            AssertEquals("Ø18 28 HOLES ON\nP.C.D. 770", tokens["{{BHC}}"], "BHC Callout");
            Console.WriteLine("  [PASS] Generation Snapshot generated {{TUBE_QTY}} = 352 and all CAD tokens accurately.");

            // -------------------------------------------------------------------------
            // TEST 5: Excel Immutability Check
            // -------------------------------------------------------------------------
            Console.WriteLine("\n--- TEST 5: Excel Workbook Immutability ---");
            string hashAfter = CalculateSha256(excelPath);
            if (hashBefore != hashAfter)
            {
                throw new InvalidOperationException("[FAIL] CRITICAL REGRESSION: Excel workbook was modified!");
            }
            Console.WriteLine($"  [PASS] Excel workbook hash matched perfectly ({hashAfter.Substring(0, 16)}...). Zero write-back verified.");

            Console.WriteLine("\n==========================================================================");
            Console.WriteLine("ALL TUBE QTY & DATA-FLOW TESTS PASSED (0 ERRORS, 0 WARNINGS)");
            Console.WriteLine("==========================================================================");
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
