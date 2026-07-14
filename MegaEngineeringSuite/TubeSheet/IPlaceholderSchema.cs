using System.Collections.Generic;

namespace MegaEngineeringSuite.TubeSheet
{
    public enum IdentifierStyle
    {
        Bracketed,
        Flat
    }

    public class SchemaDefinition
    {
        public string PlaceholderName { get; set; } = string.Empty;
        public IdentifierStyle Style { get; set; } = IdentifierStyle.Bracketed;
        public bool Required { get; set; }
        public List<string> AllowedLayers { get; set; } = new List<string>();
        public List<string> AllowedEntityTypes { get; set; } = new List<string>();
        public string WritableProperty { get; set; } = "TextString";
        public string OwnerModule { get; set; } = string.Empty;
        public List<string> Aliases { get; set; } = new List<string>();
    }

    public interface IPlaceholderSchema
    {
        IEnumerable<SchemaDefinition> GetDefinitions();
        IEnumerable<SchemaDefinition> GetActiveProfileDefinitions(MigrationProfile profile);
    }
}
