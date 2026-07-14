namespace MegaEngineeringSuite.TubeSheet
{
    public class PlaceholderDescriptor
    {
        public string EntityHandle { get; set; } = string.Empty;
        public string PlaceholderName { get; set; } = string.Empty;
        public string Layer { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public string OwnerBlock { get; set; } = string.Empty;
        public bool PaperSpace { get; set; }
        public object? BoundingBox { get; set; }
        public long ObjectId { get; set; }
    }
}
