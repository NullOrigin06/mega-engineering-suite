using System.Collections.Generic;

namespace MegaEngineeringSuite.BonnetFlange
{
    public static class AnnotationFormatter
    {
        public static Dictionary<string, string> Format(BonnetFlangeData data)
        {
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
                { "BF_THRD", (data.Thickness - 10).ToString() }
            };
        }
    }
}
