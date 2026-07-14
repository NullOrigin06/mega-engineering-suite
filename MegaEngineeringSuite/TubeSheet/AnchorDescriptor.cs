namespace MegaEngineeringSuite.TubeSheet
{
    public class AnchorDescriptor
    {
        public string Handle { get; set; } = string.Empty;
        public double InsertionPointX { get; set; }
        public double InsertionPointY { get; set; }
        public double InsertionPointZ { get; set; }
        public double Scale { get; set; }
        public double Rotation { get; set; }
        public string Layer { get; set; } = string.Empty;
        public string Layout { get; set; } = string.Empty;
        public string BlockName { get; set; } = string.Empty;
        public string Module { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
    }
}
