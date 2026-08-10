using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using ClosedXML.Excel;
using MegaEngineeringSuite;
using MegaEngineeringSuite.HeatExchangerFab;

namespace TestConsole
{
    public static class ExcelAuditRunner
    {
        public static void RunExcelAudit()
        {
            StringBuilder reportLog = new StringBuilder();
            Action<string> LogStr = (msg) =>
            {
                Console.WriteLine(msg);
                reportLog.AppendLine(msg);
            };

            LogStr("=================================================");
            LogStr("HEAT EXCHANGER FABRICATION DATA PIPELINE AUDIT");
            LogStr("=================================================");

            string projectRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\.."));
            string excelPath = Path.Combine(projectRoot, @"Templates\Heat Exchanger BOM Details.xlsx");
            LogStr($"Excel File: {excelPath} | Exists: {File.Exists(excelPath)}");

            if (!File.Exists(excelPath)) return;

            using (var wb = new XLWorkbook(excelPath))
            {
                var ws = wb.Worksheet("Heat Exchanger Data");
                LogStr($"Worksheet: '{ws.Name}'");

                LogStr("\n=== EXCEL COLUMNS & HEADERS (ROW 1 & 2) ===");
                var row1 = ws.Row(1);
                var row2 = ws.Row(2);

                for (int col = 1; col <= 25; col++)
                {
                    string h1 = row1.Cell(col).Value.ToString() ?? "";
                    string h2 = row2.Cell(col).Value.ToString() ?? "";
                    LogStr($"  Column {col} ({GetColumnLetter(col)}): Header1='{h1}' | Header2='{h2}'");
                }

                LogStr("\n=== ALL EXCEL DATA ROWS IN 'Heat Exchanger Data' ===");
                IXLRow? targetRow = null;
                foreach (var row in ws.RowsUsed())
                {
                    if (row.RowNumber() < 3) continue;
                    var cellVal = row.Cell(3).Value;
                    LogStr($"Row {row.RowNumber()}: Cell C = '{cellVal}' (IsNumber: {cellVal.IsNumber})");
                    if (targetRow == null && cellVal.IsNumber)
                    {
                        targetRow = row;
                    }
                }

                if (targetRow != null)
                {
                    LogStr($"\nUsing First Data Row (Row {targetRow.RowNumber()}, ShellID={targetRow.Cell(3).Value}):");
                    for (int c = 1; c <= 25; c++)
                    {
                        var cell = targetRow.Cell(c);
                        string formulaStr = cell.HasFormula ? $" [Formula: {cell.FormulaA1}]" : "";
                        LogStr($"  Col {c} ({GetColumnLetter(c)}): '{cell.Value}'{formulaStr}");
                    }
                }
            }

            // PIPELINE TRACING
            LogStr("\n=== PIPELINE STEP 2: LOAD VIA ExcelLookupService ===");
            AppConfigManager.Current.ExcelTemplatePath = excelPath;
            var lookupService = new ExcelLookupService();
            EngineeringDataModel dataModel;
            try
            {
                dataModel = lookupService.LoadByShellId(914);
                LogStr($"Loaded EngineeringDataModel successfully:");
                LogStr($"  ShellID: {dataModel.ShellID}");
                LogStr($"  TubeSheetFinishTHK: {dataModel.TubeSheetFinishTHK}");
                LogStr($"  TubeSheetRawTHK: {dataModel.TubeSheetRawTHK}");
                LogStr($"  BodyFlangeFinishTHK: {dataModel.BodyFlangeFinishTHK}");
                LogStr($"  BodyFlangeRawTHK: {dataModel.BodyFlangeRawTHK}");
                LogStr($"  PartitionPlateTHK: {dataModel.PartitionPlateTHK}");
                LogStr($"  BaffleTHK: {dataModel.BaffleTHK}");
                LogStr($"  BoltSize: '{dataModel.BoltSize}'");
                LogStr($"  BoltLength: {dataModel.BoltLength}");
                LogStr($"  NoOfBolts: {dataModel.NoOfBolts}");
                LogStr($"  HoleDia: {dataModel.HoleDia}");
                LogStr($"  FlangeID: {dataModel.FlangeID}");
                LogStr($"  BoltPCD: {dataModel.BoltPCD}");
                LogStr($"  TubeSheetFinishOD: {dataModel.TubeSheetFinishOD}");
                LogStr($"  TubeSheetRawOD: {dataModel.TubeSheetRawOD}");
                LogStr($"  LinerGasketOD: {dataModel.LinerGasketOD}");
                LogStr($"  TieRodDia: {dataModel.TieRodDia}");
                LogStr($"  TieRodQty: {dataModel.TieRodQty}");
                LogStr($"  SpacerTube: {dataModel.SpacerTube}");
            }
            catch (Exception ex)
            {
                LogStr($"Error loading via ExcelLookupService: {ex.Message}");
                dataModel = new EngineeringDataModel { ShellID = 914 };
            }

            LogStr("\n=== PIPELINE STEP 3: MAP VIA HeatExchangerFabDataMapper ===");
            var fabData = HeatExchangerFabDataMapper.Map(dataModel);
            LogStr($"Mapped HeatExchangerFabData:");
            LogStr($"  ShellID: {fabData.ShellID}");
            LogStr($"  ShellTHK: {fabData.ShellTHK}");
            LogStr($"  ShellLength: {fabData.ShellLength}");
            LogStr($"  TubeOD: {fabData.TubeOD}");
            LogStr($"  TubeTHK: {fabData.TubeTHK}");
            LogStr($"  TubeLength: {fabData.TubeLength}");
            LogStr($"  TotalTubes: {fabData.TotalTubes}");
            LogStr($"  TubePitch: {fabData.TubePitch}");
            LogStr($"  PitchType: {fabData.PitchType}");
            LogStr($"  TubeSheetOD: {fabData.TubeSheetOD}");
            LogStr($"  TubeSheetTHK: {fabData.TubeSheetTHK}");
            LogStr($"  FlangeOD: {fabData.FlangeOD}");
            LogStr($"  FlangeID: {fabData.FlangeID}");
            LogStr($"  FlangeTHK: {fabData.FlangeTHK}");
            LogStr($"  LinerTHK: {fabData.LinerTHK}");
            LogStr($"  LinerID: {fabData.LinerID}");
            LogStr($"  LinerOD: {fabData.LinerOD}");
            LogStr($"  SerrationID: {fabData.SerrationID}");
            LogStr($"  SerrationOD: {fabData.SerrationOD}");
            LogStr($"  HoleDia: {fabData.HoleDia}");
            LogStr($"  NoOfBolts: {fabData.NoOfBolts}");
            LogStr($"  BoltPCD: {fabData.BoltPCD}");
            LogStr($"  BaffleQty: {fabData.BaffleQty}");
            LogStr($"  BaffleTHK: {fabData.BaffleTHK}");
            LogStr($"  NoOfPasses: {fabData.NoOfPasses}");
            LogStr($"  TieRodQty: {fabData.TieRodQty}");
            LogStr($"  TieRodDia: {fabData.TieRodDia}");

            LogStr("\n=== PIPELINE STEP 4: FORMAT VIA HeatExchangerFabFormatter ===");
            var replacements = HeatExchangerFabFormatter.Format(fabData);
            foreach (var kvp in replacements)
            {
                LogStr($"  Formatter Key: '{kvp.Key}' => Value: '{kvp.Value.Replace("\n", "\\n")}'");
            }

            File.WriteAllText(Path.Combine(projectRoot, "DataPipelineAuditLog.txt"), reportLog.ToString());
        }

        private static string GetColumnLetter(int columnNumber)
        {
            int dividend = columnNumber;
            string columnName = String.Empty;
            int modulo;

            while (dividend > 0)
            {
                modulo = (dividend - 1) % 26;
                columnName = Convert.ToChar(65 + modulo).ToString() + columnName;
                dividend = (int)((dividend - modulo) / 26);
            }

            return columnName;
        }
    }
}
