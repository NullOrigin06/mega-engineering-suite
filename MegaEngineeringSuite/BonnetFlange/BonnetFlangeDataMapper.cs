using System;
using System.Text;
using MegaEngineeringSuite.Infrastructure.Logging;

namespace MegaEngineeringSuite.BonnetFlange
{
    public static class BonnetFlangeDataMapper
    {
        public static BonnetFlangeData Map(EngineeringDataModel currentData)
        {
            if (currentData == null)
            {
                throw new ArgumentNullException(nameof(currentData), "EngineeringDataModel cannot be null.");
            }

            // Pre-generation validation
            ValidateRequiredFields(currentData);

            // Execute the mapping (current assumptions, to be verified by HOD)
            var data = new BonnetFlangeData
            {
                OD = currentData.TubeSheetFinishOD,
                ID = currentData.FlangeID,
                Thickness = currentData.BodyFlangeFinishTHK,
                LinerOD = currentData.LinerGasketOD,
                LinerID = currentData.ShellID,
                ShellID = currentData.ShellID,
                PCD = currentData.BoltPCD,
                BoltQty = currentData.NoOfBolts,
                BoltHoleDia = currentData.HoleDia
            };

            // Log the mapping exactly as requested to make debugging easy
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("EngineeringData mapped to BonnetFlangeData");
            sb.AppendLine($"OD = {data.OD}");
            sb.AppendLine($"ID = {data.ID}");
            sb.AppendLine($"THK = {data.Thickness}");
            sb.AppendLine($"LINER_OD = {data.LinerOD}");
            sb.AppendLine($"LINER_ID = {data.LinerID}");
            sb.AppendLine($"SHELL_ID = {data.ShellID}");
            sb.AppendLine($"PCD = {data.PCD}");
            sb.AppendLine($"BOLT_QTY = {data.BoltQty}");
            sb.AppendLine($"BOLT_HOLE_DIA = {data.BoltHoleDia}");
            SimpleLogger.Log("Workflow", sb.ToString());

            return data;
        }

        private static void ValidateRequiredFields(EngineeringDataModel data)
        {
            StringBuilder missingFields = new StringBuilder();

            if (data.TubeSheetFinishOD <= 0)
                missingFields.AppendLine("• TubeSheetFinishOD (mapped to OD) is missing or zero.");
            
            if (data.FlangeID <= 0)
                missingFields.AppendLine("• FlangeID (mapped to ID) is missing or zero.");
            
            if (data.BodyFlangeFinishTHK <= 0)
                missingFields.AppendLine("• BodyFlangeFinishTHK (mapped to Thickness) is missing or zero.");
                
            if (data.LinerGasketOD <= 0)
                missingFields.AppendLine("• LinerGasketOD (mapped to Liner OD) is missing or zero.");
                
            if (data.ShellID <= 0)
                missingFields.AppendLine("• ShellID (mapped to Liner ID & Shell ID) is missing or zero.");

            if (data.BoltPCD <= 0)
                missingFields.AppendLine("• BoltPCD (mapped to PCD) is missing or zero.");

            if (data.NoOfBolts <= 0)
                missingFields.AppendLine("• NoOfBolts (mapped to BoltQty) is missing or zero.");

            if (data.HoleDia <= 0)
                missingFields.AppendLine("• HoleDia (mapped to BoltHoleDia) is missing or zero.");

            if (missingFields.Length > 0)
            {
                throw new InvalidOperationException($"Cannot generate Body Flange. Required engineering fields are missing or zero:\n\n{missingFields}");
            }
        }
    }
}
