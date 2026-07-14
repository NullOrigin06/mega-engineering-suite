using System.Collections.Generic;

namespace MegaEngineeringSuite.TubeSheet
{
    public interface IAnchorResolver
    {
        IEnumerable<AnchorDescriptor> FindAnchors(string blockName);
        AnchorDescriptor FindAnchorByModule(string blockName, string moduleName);
    }
}
