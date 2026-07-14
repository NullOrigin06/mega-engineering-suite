namespace MegaEngineeringSuite.TubeSheet
{
    public class BlockAttributeDescriptor
    {
        public string BlockHandle { get; set; } = string.Empty;
        public string AttributeHandle { get; set; } = string.Empty;
        public string Tag { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string Layout { get; set; } = string.Empty;
        public string BlockName { get; set; } = string.Empty;
        public bool IsConstant { get; set; }
        public bool IsInvisible { get; set; }
    }
}

