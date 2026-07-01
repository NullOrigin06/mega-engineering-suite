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
                { "BF_THK", $"{data.Thickness} THK" }
            };
        }
    }
}
