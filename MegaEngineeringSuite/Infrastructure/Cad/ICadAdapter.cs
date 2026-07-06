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
        void OpenDrawing(string filePath);
        CadOperationTimes ReplaceAnnotationPlaceholders(Dictionary<string, string> replacements);
        void UpdateTitleBlockAttributes(DrawingInformation info);
        void SaveAs(string newFilePath);
        void Save();
        void CloseDrawing();
    }
}
