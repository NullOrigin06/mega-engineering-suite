using System.Collections.Generic;

namespace MegaEngineeringSuite.TubeSheet
{
    public class TubeSheetPlaceholderSchema : IPlaceholderSchema
    {
        public IEnumerable<SchemaDefinition> GetDefinitions()
        {
            return new List<SchemaDefinition>
            {
                new SchemaDefinition
                {
                    PlaceholderName = "TS_OD",
                    Style = IdentifierStyle.Flat,
                    Required = true,
                    AllowedLayers = new List<string> { "META_DETAIL_A", "DIM" },
                    AllowedEntityTypes = new List<string> { "Text", "MText", "AcDbText", "AcDbMText", "RotatedDimension", "AcDbRotatedDimension" },
                    WritableProperty = "TextOverride",
                    OwnerModule = "DetailA"
                },
                new SchemaDefinition
                {
                    PlaceholderName = "TS_ID",
                    Style = IdentifierStyle.Flat,
                    Required = true,
                    AllowedLayers = new List<string> { "META_DETAIL_A", "DIM" },
                    AllowedEntityTypes = new List<string> { "Text", "MText", "AcDbText", "AcDbMText", "RotatedDimension", "AcDbRotatedDimension" },
                    WritableProperty = "TextOverride",
                    OwnerModule = "DetailA"
                },
                new SchemaDefinition
                {
                    PlaceholderName = "TS_STEP_OD",
                    Style = IdentifierStyle.Flat,
                    Required = true,
                    AllowedLayers = new List<string> { "META_DETAIL_A", "DIM" },
                    AllowedEntityTypes = new List<string> { "Text", "MText", "AcDbText", "AcDbMText", "RotatedDimension", "AcDbRotatedDimension" },
                    WritableProperty = "TextOverride",
                    OwnerModule = "DetailA"
                },
                new SchemaDefinition
                {
                    PlaceholderName = "TS_THK",
                    Style = IdentifierStyle.Flat,
                    Required = true,
                    AllowedLayers = new List<string> { "META_DETAIL_A", "DIM" },
                    AllowedEntityTypes = new List<string> { "Text", "MText", "AcDbText", "AcDbMText", "RotatedDimension", "AcDbRotatedDimension" },
                    WritableProperty = "TextOverride",
                    OwnerModule = "DetailA"
                },
                // Keep the legacy <TS_OD> versions if needed for other profiles, 
                // but the prompt says: "Update TubeSheetPlaceholderSchema so that the Stage8_DetailADimensions profile contains exactly TS_OD... Retain all existing schema definitions for future migration stages."
                new SchemaDefinition
                {
                    PlaceholderName = "<TS_OD>",
                    Style = IdentifierStyle.Bracketed,
                    Required = true,
                    AllowedLayers = new List<string> { "META_DETAIL_A" },
                    AllowedEntityTypes = new List<string> { "Text", "MText", "AcDbText", "AcDbMText" },
                    WritableProperty = "TextString",
                    OwnerModule = "DetailA"
                },
                new SchemaDefinition
                {
                    PlaceholderName = "<TS_ID>",
                    Style = IdentifierStyle.Bracketed,
                    Required = true,
                    AllowedLayers = new List<string> { "META_DETAIL_A" },
                    AllowedEntityTypes = new List<string> { "Text", "MText", "AcDbText", "AcDbMText" },
                    WritableProperty = "TextString",
                    OwnerModule = "DetailA"
                },
                new SchemaDefinition
                {
                    PlaceholderName = "<TS_STEP_OD>",
                    Style = IdentifierStyle.Bracketed,
                    Required = true,
                    AllowedLayers = new List<string> { "META_DETAIL_A" },
                    AllowedEntityTypes = new List<string> { "Text", "MText", "AcDbText", "AcDbMText" },
                    WritableProperty = "TextString",
                    OwnerModule = "DetailA"
                },
                new SchemaDefinition
                {
                    PlaceholderName = "<TS_STEP_ID>",
                    Style = IdentifierStyle.Bracketed,
                    Required = true,
                    AllowedLayers = new List<string> { "META_DETAIL_A" },
                    AllowedEntityTypes = new List<string> { "Text", "MText", "AcDbText", "AcDbMText" },
                    WritableProperty = "TextString",
                    OwnerModule = "DetailA"
                },
                new SchemaDefinition
                {
                    PlaceholderName = "<CUSTOMER_NAME>",
                    Style = IdentifierStyle.Bracketed,
                    Required = true,
                    AllowedLayers = new List<string> { "META_TITLE" },
                    AllowedEntityTypes = new List<string> { "Attribute", "Block Attribute", "AttributeReference" },
                    WritableProperty = "TextString",
                    OwnerModule = "TitleBlock"
                },
                new SchemaDefinition
                {
                    PlaceholderName = "<DETAIL_A_OD>",
                    Style = IdentifierStyle.Bracketed,
                    Required = false,
                    AllowedLayers = new List<string> { "META_DETAIL_A" },
                    AllowedEntityTypes = new List<string> { "Text", "MText", "AcDbText", "AcDbMText" },
                    WritableProperty = "TextString",
                    OwnerModule = "Legacy"
                }
            };
        }

        public IEnumerable<SchemaDefinition> GetActiveProfileDefinitions(MigrationProfile profile)
        {
            var all = GetDefinitions();
            if (profile == MigrationProfile.Stage8_DetailADimensions)
            {
                var active = new List<SchemaDefinition>();
                foreach (var def in all)
                {
                    if (def.PlaceholderName == "TS_OD" ||
                        def.PlaceholderName == "TS_ID" ||
                        def.PlaceholderName == "TS_STEP_OD" ||
                        def.PlaceholderName == "TS_THK")
                    {
                        active.Add(def);
                    }
                }
                return active;
            }
            else if (profile == MigrationProfile.Production)
            {
                return all;
            }
            
            return new List<SchemaDefinition>();
        }
    }
}
