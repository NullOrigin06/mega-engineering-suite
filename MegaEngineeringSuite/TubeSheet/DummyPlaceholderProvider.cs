using System.Collections.Generic;

namespace MegaEngineeringSuite.TubeSheet
{
    public class DummyPlaceholderProvider : IPlaceholderProvider
    {
        private readonly Dictionary<string, string> _values = new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase)
        {
            { "<TS_OD>", "1500 mm" },
            { "<CUSTOMER_NAME>", "Acme Corp" },
            { "<DETAIL_A_OD>", "1520 mm" }
        };

        public IReadOnlyDictionary<string, string> GetValues(MigrationProfile profile)
        {
            return _values;
        }

    }
}
