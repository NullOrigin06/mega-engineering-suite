using System;
using System.Collections.Generic;

namespace MegaEngineeringSuite.Infrastructure.Cad
{
    public struct CadOperationTimes
    {
        public long ScanTimeMs;
        public long ReplaceTimeMs;
    }

    public interface ICadAdapter : IDisposable
    {
        void AttachToExistingSession(object cadApp, object cadDoc);
        void OpenDrawing(string filePath);
        void SetSystemVariable(string name, object value);
        object GetSystemVariable(string name);
        void SendCommand(string command);
        CadOperationTimes ReplaceAnnotationPlaceholders(Dictionary<string, string> replacements);
        void UpdateTitleBlockAttributes(DrawingInformation info);
        void SaveAs(string newFilePath);
        void Save();
        void CloseDrawing();
        
        CadDocumentIdentity GetDocumentIdentity();
        System.Collections.Generic.List<TubeSheet.PlaceholderDescriptor> DiscoverPlaceholders(DiscoveryMode mode = DiscoveryMode.MetaLayers);
        dynamic GetEntityByHandle(string handle);
        
        // Title Block Specific Methods
        System.Collections.Generic.List<TubeSheet.BlockAttributeDescriptor> DiscoverBlockAttributes(string blockName);
        string UpdateBlockAttribute(string blockHandle, string tag, string newValue);
    }
}
