using System.Collections.Generic;

namespace MegaEngineeringSuite.TubeSheet
{
    public interface IAnnotationEngine
    {
        void ReplacePlaceholders(Dictionary<string, string> placeholders);
    }
}
