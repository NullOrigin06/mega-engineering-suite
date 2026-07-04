using System.Collections.Generic;

namespace MegaEngineeringSuite.BonnetFlange
{
    public static class AnnotationFormatter
    {
        public static Dictionary<string, string> Format(BonnetFlangeData data)
        {
            double serrationThickness = 5.0; // Constant as requested
            
            // BOM1 (Surretion Ring)
            double bom1Wt = 0.7854 * (System.Math.Pow(data.LinerOD, 2) - System.Math.Pow(data.ShellID, 2)) * serrationThickness * 8e-6 * 2;
            string bom1Size = $"OD.{data.LinerOD} x ID.{data.ShellID} x {serrationThickness} THK.";
            
            // BOM2 (Bonnet Flange)
            double bom2Wt = 0.7854 * (System.Math.Pow(data.OD, 2) - System.Math.Pow(data.ID, 2)) * data.Thickness * 7.85e-6 * 2;
            string bom2Size = $"OD.{data.OD} x ID.{data.ID} x {data.Thickness} THK.";

            return new Dictionary<string, string>
            {
                { "BF_OD", $"O.D. {data.OD}" },
                { "BF_ID", $"I.D. {data.ID}" },
                { "BF_LINER_OD", $"LINER O.D. {data.LinerOD}" },
                { "BF_LINER_ID", $"LINER I.D. {data.LinerID}" },
                { "BF_THK", $"{data.Thickness} THK" },
                { "BF_PCD", data.PCD.ToString() },
                { "BF_SHELL_ID", data.ShellID.ToString() },
                { "BF_BOLT1", $"Ø{data.BoltHoleDia}, {data.BoltQty} NOS." },
                { "BF_BOLT2", $"{data.PCD} P.C.D." },
                { "BF_THRD", (data.Thickness - 10).ToString() },
                { "BOM1_SIZE", bom1Size },
                { "BOM1_WT", bom1Wt.ToString("F1") },
                { "BOM2_SIZE", bom2Size },
                { "BOM2_WT", bom2Wt.ToString("F1") }
            };
        }
    }
}
