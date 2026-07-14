using System.Collections.Generic;
using System.Linq;

namespace MegaEngineeringSuite.TubeSheet
{
    public class PlaceholderIndex
    {
        private readonly Dictionary<string, PlaceholderDescriptor> _index = new Dictionary<string, PlaceholderDescriptor>();

        public void Add(PlaceholderDescriptor descriptor)
        {
            if (!_index.ContainsKey(descriptor.EntityHandle))
            {
                _index.Add(descriptor.EntityHandle, descriptor);
            }
        }

        public bool Contains(string handle)
        {
            return _index.ContainsKey(handle);
        }

        public IEnumerable<PlaceholderDescriptor> FindByLayer(string layerName)
        {
            return _index.Values.Where(d => d.Layer.Equals(layerName, System.StringComparison.OrdinalIgnoreCase));
        }

        public IEnumerable<PlaceholderDescriptor> FindByPlaceholder(string placeholderName)
        {
            return _index.Values.Where(d => d.PlaceholderName.Equals(placeholderName, System.StringComparison.OrdinalIgnoreCase));
        }

        public IEnumerable<PlaceholderDescriptor> Enumerate()
        {
            return _index.Values;
        }

        public int Count => _index.Count;
    }
}
