using System;
using System.Collections.Generic;
using MegaEngineeringSuite.Infrastructure.Cad;
using MegaEngineeringSuite.Infrastructure.Logging;

namespace MegaEngineeringSuite.BonnetFlange
{
    public class AnnotationEngine
    {
        private readonly ICadAdapter _cadAdapter;

        public AnnotationEngine(ICadAdapter cadAdapter)
        {
            _cadAdapter = cadAdapter ?? throw new ArgumentNullException(nameof(cadAdapter));
        }

        public CadOperationTimes ProcessAnnotations(Dictionary<string, string> replacements)
        {
            // Execute the single-pass optimized replacement
            return _cadAdapter.ReplaceAnnotationPlaceholders(replacements);
        }
    }
}
