namespace MegaEngineeringSuite.Infrastructure.Cad
{
    public class CadDocumentIdentity
    {
        public string DocumentName { get; set; } = string.Empty;
        public string FullPath { get; set; } = string.Empty;
        public bool IsActiveDocument { get; set; }
        public string DatabaseHandle { get; set; } = string.Empty;
        public string DocumentPointer { get; set; } = string.Empty;
        public int ModelSpaceCount { get; set; }
        public string LayoutName { get; set; } = string.Empty;
    }
}
