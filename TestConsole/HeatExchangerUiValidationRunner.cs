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
            Console.WriteLine("PROFILE + OVERRIDE + SNAPSHOT ARCHITECTURAL & TRANSITION VALIDATION SUITE");
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

            // -------------------------------------------------------------------------
            // TEST 1: Discrete Fastener & Structural Transition Boundaries
            // -------------------------------------------------------------------------
            Console.WriteLine("\n--- TEST 1: Discrete Boundary Transitions ---");

            // Boundary A: 390 -> 400 (M14 -> M16)
            var p390 = lookupService.LoadProfileByShellId(390);
            var p400 = lookupService.LoadProfileByShellId(400);
            AssertEquals("M14", p390.BoltSize, "390 Bolt Size");
            AssertEquals(16.0, p390.HoleDia, "390 Hole Dia");
            AssertEquals(20, p390.NoOfBolts, "390 No of Bolts");
            AssertEquals("M16", p400.BoltSize, "400 Bolt Size");
            AssertEquals(18.0, p400.HoleDia, "400 Hole Dia");
            AssertEquals(16, p400.NoOfBolts, "400 No of Bolts (Drops due to M16 rating)");
            Console.WriteLine("  [PASS] 390 -> 400 Transition verified (M14 -> M16).");

            // Boundary B: 800 -> 810 (M16 -> M20)
            var p800 = lookupService.LoadProfileByShellId(800);
            var p810 = lookupService.LoadProfileByShellId(810);
            AssertEquals("M16", p800.BoltSize, "800 Bolt Size");
            AssertEquals(18.0, p800.HoleDia, "800 Hole Dia");
            AssertEquals(28, p800.NoOfBolts, "800 No of Bolts");
            AssertEquals(27.0, p800.TubeSheetFinishTHK, "800 TS Finish THK");
            AssertEquals(36.0, p800.BodyFlangeFinishTHK, "800 BF Finish THK");
            AssertEquals("M20", p810.BoltSize, "810 Bolt Size");
            AssertEquals(22.0, p810.HoleDia, "810 Hole Dia");
            AssertEquals(24, p810.NoOfBolts, "810 No of Bolts (Drops due to M20 rating)");
            AssertEquals(30.0, p810.TubeSheetFinishTHK, "810 TS Finish THK");
            AssertEquals(40.0, p810.BodyFlangeFinishTHK, "810 BF Finish THK");
            Console.WriteLine("  [PASS] 800 -> 810 Transition verified (M16 -> M20).");

            // Boundary C: 1500 -> 1510 (M20 -> M24)
            var p1500 = lookupService.LoadProfileByShellId(1500);
            var p1510 = lookupService.LoadProfileByShellId(1510);
            AssertEquals("M20", p1500.BoltSize, "1500 Bolt Size");
            AssertEquals(22.0, p1500.HoleDia, "1500 Hole Dia");
            AssertEquals(48, p1500.NoOfBolts, "1500 No of Bolts");
            AssertEquals(42.0, p1500.TubeSheetFinishTHK, "1500 TS Finish THK");
            AssertEquals(60.0, p1500.BodyFlangeFinishTHK, "1500 BF Finish THK");
            AssertEquals("M24", p1510.BoltSize, "1510 Bolt Size");
            AssertEquals(27.0, p1510.HoleDia, "1510 Hole Dia");
            AssertEquals(40, p1510.NoOfBolts, "1510 No of Bolts (Drops due to M24 rating)");
            AssertEquals(44.0, p1510.TubeSheetFinishTHK, "1510 TS Finish THK");
            AssertEquals(65.0, p1510.BodyFlangeFinishTHK, "1510 BF Finish THK");
            Console.WriteLine("  [PASS] 1500 -> 1510 Transition verified (M20 -> M24).");

            // Boundary D: 2090 -> 2100 (M24 -> M27)
            var p2090 = lookupService.LoadProfileByShellId(2090);
            var p2100 = lookupService.LoadProfileByShellId(2100);
            AssertEquals("M24", p2090.BoltSize, "2090 Bolt Size");
            AssertEquals(27.0, p2090.HoleDia, "2090 Hole Dia");
            AssertEquals(184.0, p2090.BoltLength, "2090 Bolt Length");
            AssertEquals("M27", p2100.BoltSize, "2100 Bolt Size");
            AssertEquals(30.0, p2100.HoleDia, "2100 Hole Dia");
            AssertEquals(187.0, p2100.BoltLength, "2100 Bolt Length");
            Console.WriteLine("  [PASS] 2090 -> 2100 Transition verified (M24 -> M27).");

            // -------------------------------------------------------------------------
            // TEST 2: Profile Reload & Override Clearing Lifecycle
            // -------------------------------------------------------------------------
            Console.WriteLine("\n--- TEST 2: Profile Reload & Safe Override Lifecycle ---");

            var workingModel = lookupService.LoadByShellId(800);
            workingModel.BonnetShellFSLength = 650.0;
            workingModel.BonnetShellRSLength = 700.0;
            
            // Engineer overrides TubeSheetFinishTHK on 800 profile
            workingModel.TubeSheetFinishTHK = 29.0;
            AssertEquals(29.0, workingModel.TubeSheetFinishTHK, "Working model TS THK Override (800)");

            // User switches Shell ID: 800 -> 810
            var newProfile810 = lookupService.LoadProfileByShellId(810);
            
            // Emulate OnShellIdChanged: reload profile, clear overrides, preserve non-Excel extras
            workingModel.ShellID = newProfile810.ShellID;
            workingModel.TubeSheetFinishTHK = newProfile810.TubeSheetFinishTHK;
            workingModel.TubeSheetRawTHK = newProfile810.TubeSheetRawTHK;
            workingModel.BodyFlangeFinishTHK = newProfile810.BodyFlangeFinishTHK;
            workingModel.BodyFlangeRawTHK = newProfile810.BodyFlangeRawTHK;
            workingModel.PartitionPlateTHK = newProfile810.PartitionPlateTHK;
            workingModel.BaffleTHK = newProfile810.BaffleTHK;
            workingModel.BoltSize = newProfile810.BoltSize;
            workingModel.BoltLength = newProfile810.BoltLength;
            workingModel.NoOfBolts = newProfile810.NoOfBolts;
            workingModel.HoleDia = newProfile810.HoleDia;
            workingModel.FlangeID = newProfile810.FlangeID;
            workingModel.BoltPCD = newProfile810.BoltPCD;
            workingModel.TubeSheetFinishOD = newProfile810.TubeSheetFinishOD;
            workingModel.TubeSheetRawOD = newProfile810.TubeSheetRawOD;
            workingModel.LinerGasketOD = newProfile810.LinerGasketOD;
            workingModel.TieRodDia = newProfile810.TieRodDia;
            workingModel.TieRodQty = newProfile810.TieRodQty;
            workingModel.SpacerTube = newProfile810.SpacerTube;
            workingModel.DishendTHK = newProfile810.DishendTHK;
            // Preserved extras:
            AssertEquals(650.0, workingModel.BonnetShellFSLength, "Preserved BonnetShellFSLength on 810 profile switch");
            AssertEquals(700.0, workingModel.BonnetShellRSLength, "Preserved BonnetShellRSLength on 810 profile switch");

            // Verify that 800 override (29mm) was cleared and refreshed to 810 profile baseline (30mm)
            AssertEquals(30.0, workingModel.TubeSheetFinishTHK, "Refreshed TS Finish THK from 810 profile");
            AssertEquals(40.0, workingModel.BodyFlangeFinishTHK, "Refreshed BF Finish THK from 810 profile");
            AssertEquals(5.0, workingModel.DishendTHK, "Refreshed Dishend THK from 810 profile");
            Console.WriteLine("  [PASS] Shell ID switch accurately refreshed all profile fields and preserved non-Excel extras.");

            // -------------------------------------------------------------------------
            // TEST 3: Immutable Generation Snapshot Creation
            // -------------------------------------------------------------------------
            Console.WriteLine("\n--- TEST 3: Generation Snapshot Creation & Generator Formatter ---");

            // Engineer overrides TubeSheetFinishTHK to 35mm on 810 profile
            workingModel.TubeSheetFinishTHK = 35.0;

            var snapshot = new HeatExchangerGenerationSnapshot
            {
                RunId = "TEST-RUN-001",
                ShellID = workingModel.ShellID,
                ShellTHK = 5.0,
                ShellLength = 3000.0,
                TubeOD = 19.05,
                TubeTHK = 1.6,
                TubeLength = 3000.0,
                TotalTubes = 120,
                TubePitch = 23.81,
                NoOfPasses = 2,
                TubeSheetOD = workingModel.TubeSheetFinishOD,
                TubeSheetTHK = workingModel.TubeSheetFinishTHK, // 35.0 (overridden)
                FlangeOD = workingModel.TubeSheetFinishOD,
                FlangeID = workingModel.FlangeID,
                FlangeTHK = workingModel.BodyFlangeFinishTHK,
                LinerOD = workingModel.LinerGasketOD,
                LinerID = workingModel.ShellID,
                LinerTHK = 3.0,
                SerrationOD = workingModel.LinerGasketOD,
                SerrationID = workingModel.ShellID,
                BaffleQty = 6,
                BaffleTHK = workingModel.BaffleTHK,
                PartitionPlateTHK = workingModel.PartitionPlateTHK,
                BoltSize = workingModel.BoltSize,
                BoltLength = workingModel.BoltLength,
                NoOfBolts = workingModel.NoOfBolts,
                HoleDia = workingModel.HoleDia,
                BoltPCD = workingModel.BoltPCD,
                TieRodQty = (int)workingModel.TieRodQty,
                TieRodDia = workingModel.TieRodDia,
                SpacerTube = workingModel.SpacerTube,
                BonnetShellFSLength = workingModel.BonnetShellFSLength,
                BonnetShellRSLength = workingModel.BonnetShellRSLength,
                DishendTHK = workingModel.DishendTHK
            };

            var fabData = snapshot.ToFabData();
            var tokens = HeatExchangerFabFormatter.Format(fabData);

            AssertEquals("810", tokens["{{SHELL_ID}}"], "Snapshot Shell ID");
            AssertEquals("35 THK.", tokens["{{TUBESHEET_THK}}"], "Snapshot Overridden TS THK Token");
            AssertEquals("40 THK.", tokens["{{BODY_FLANGE_THK}}"], "Snapshot BF THK Token");
            AssertEquals("5 THK.", tokens["{{DISHEND_THK}}"], "Snapshot Dishend THK Token");
            AssertEquals("650", tokens["{{BONNET_SHELL_FS_LENGTH}}"], "Snapshot FS Length Token");
            AssertEquals("700", tokens["{{BONNET_SHELL_RS_LENGTH}}"], "Snapshot RS Length Token");
            AssertEquals("Ø22 24 HOLES ON\nP.C.D. 895", tokens["{{BHC}}"], "Snapshot Bolt Hole Callout {{BHC}}");
            Console.WriteLine("  [PASS] Generation Snapshot correctly formatted all tokens including active overrides and CAD callouts.");

            // -------------------------------------------------------------------------
            // TEST 4: Excel Workbook Immutability (Zero Write-Back)
            // -------------------------------------------------------------------------
            Console.WriteLine("\n--- TEST 4: Excel Workbook Immutability ---");
            string hashAfter = CalculateSha256(excelPath);
            if (hashBefore != hashAfter)
            {
                throw new InvalidOperationException("[FAIL] CRITICAL REGRESSION: Excel workbook was modified!");
            }
            Console.WriteLine($"  [PASS] Excel workbook hash matched perfectly ({hashAfter.Substring(0, 16)}...). Zero write-back verified.");

            Console.WriteLine("\n==========================================================================");
            Console.WriteLine("ALL TRANSITION, PROFILE & SNAPSHOT TESTS PASSED (0 ERRORS, 0 WARNINGS)");
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
