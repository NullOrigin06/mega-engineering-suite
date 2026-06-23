using System;
using System.IO;
using ClosedXML.Excel;

namespace MegaEngineeringSuite
{
    public class ExcelLookupService
    {
        private readonly string excelFilePath = @"C:\Users\PARTH\Downloads\Heat Exchanger BOM Details.xlsx";
        private readonly string sheetName = "Heat Exchanger Data";

        /// <summary>
        /// Reads the Excel file and retrieves standard dimensions for the given Shell ID.
        /// </summary>
        /// <param name="shellId">The calculated Shell ID to look up.</param>
        /// <returns>Populated EngineeringDataModel</returns>
        public EngineeringDataModel LoadByShellId(int shellId)
        {
            if (!File.Exists(excelFilePath))
            {
                throw new FileNotFoundException($"The required engineering data file was not found at {excelFilePath}");
            }

            using (var workbook = new XLWorkbook(excelFilePath))
            {
                var ws = workbook.Worksheet(sheetName);
                if (ws == null)
                {
                    throw new Exception($"Sheet '{sheetName}' not found in the Excel file.");
                }

                var rows = ws.RowsUsed();
                
                // Skip header rows (assuming data starts at Row 3 based on previous analysis)
                foreach (var row in rows)
                {
                    if (row.RowNumber() < 3) continue;

                    // Read Column 3 (Shell I.D.)
                    var cellShellId = row.Cell(3).Value;
                    
                    if (cellShellId.IsNumber && (int)cellShellId.GetNumber() == shellId)
                    {
                        // Found a match, extract all columns based on the mapped indices
                        return new EngineeringDataModel
                        {
                            ShellID = shellId,
                            TubeSheetFinishTHK = GetDoubleValue(row, 7),
                            TubeSheetRawTHK = GetDoubleValue(row, 8),
                            BodyFlangeFinishTHK = GetDoubleValue(row, 9),
                            BodyFlangeRawTHK = GetDoubleValue(row, 10),
                            PartitionPlateTHK = GetDoubleValue(row, 11),
                            BaffleTHK = GetDoubleValue(row, 12),
                            BoltSize = row.Cell(13).GetString(),
                            BoltLength = GetDoubleValue(row, 14),
                            NoOfBolts = (int)GetDoubleValue(row, 15),
                            HoleDia = GetDoubleValue(row, 16),
                            FlangeID = GetDoubleValue(row, 17),
                            BoltPCD = GetDoubleValue(row, 18),
                            TubeSheetFinishOD = GetDoubleValue(row, 19),
                            TubeSheetRawOD = GetDoubleValue(row, 20),
                            TieRodDia = GetDoubleValue(row, 22),
                            TieRodQty = (int)GetDoubleValue(row, 23),
                            SpacerTube = GetDoubleValue(row, 24)
                        };
                    }
                }

                throw new Exception($"Shell ID {shellId} was not found in the '{sheetName}' sheet.");
            }
        }

        private double GetDoubleValue(IXLRow row, int column)
        {
            var cell = row.Cell(column);
            
            // If it's a numeric formula or plain value, return it
            if (cell.HasFormula)
            {
                // Try getting the cached evaluated value first
                var val = cell.CachedValue;
                if (val.IsNumber) return val.GetNumber();
                
                // If it fails, fallback to Value.GetNumber() 
                if (cell.Value.IsNumber) return cell.Value.GetNumber();
                
                return 0.0;
            }
            
            if (cell.Value.IsNumber)
                return cell.Value.GetNumber();
            
            if (cell.Value.IsText && double.TryParse(cell.Value.GetText(), out double result))
                return result;

            return 0.0;
        }
    }
}
