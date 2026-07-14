using System.Collections.Generic;

namespace MegaEngineeringSuite.TubeSheet
{
    public interface IPlaceholderProvider
    {
        IReadOnlyDictionary<string, string> GetValues(MigrationProfile profile);
    }
}
