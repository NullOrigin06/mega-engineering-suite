using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using MegaEngineeringSuite.Infrastructure.Logging;

namespace MegaEngineeringSuite.Infrastructure.Cad
{
    public class GstarCadAdapter : ICadAdapter
    {
        private dynamic? _cadApp;
        private dynamic? _cadDoc;
        private bool _disposedValue;

        public GstarCadAdapter()
        {
            // Acquire CAD instance centrally (reusing if possible)
            _cadApp = CadSessionManager.Instance.GetCadApplication();
        }

        public void OpenDrawing(string filePath)
        {
            if (_cadApp == null) throw new InvalidOperationException("CAD application is not initialized.");
            
            _cadDoc = _cadApp.Documents.Open(filePath);
        }

        public CadOperationTimes ReplaceAnnotationPlaceholders(Dictionary<string, string> replacements)
        {
            if (_cadDoc == null) throw new InvalidOperationException("No drawing is currently open.");

            var scanStopwatch = new Stopwatch();
            var replaceStopwatch = new Stopwatch();

            scanStopwatch.Start();

            dynamic layouts = _cadDoc.Layouts;
            int layoutCount = layouts.Count;
            
            int totalAnnotationsScanned = 0;
            int totalReplaced = 0;

            for (int l = 0; l < layoutCount; l++)
            {
                dynamic layout = layouts.Item(l);
                string layoutName = layout.Name;
                SimpleLogger.Log("BonnetFlange", $"Scanning Layout : {layoutName}");
                
                dynamic block = layout.Block;
                int count = block.Count;

                for (int i = 0; i < count; i++)
                {
                    dynamic entity = block.Item(i);
                    string entityName = entity.EntityName;

                    string? currentText = null;
                    string? propertyName = null;
                    
                    try
                    {
                        if (entityName.Contains("Dimension"))
                        {
                            currentText = entity.TextOverride;
                            propertyName = "TextOverride";
                        }
                        else if (entityName.Contains("MText") || entityName.Contains("Text") || entityName.Contains("MLeader"))
                        {
                            currentText = entity.TextString;
                            propertyName = "TextString";
                        }
                    }
                    catch
                    {
                        // Swallow unsupported property exceptions
                    }

                    if (!string.IsNullOrEmpty(currentText))
                    {
                        totalAnnotationsScanned++;
                        
                        string newText = currentText;
                        bool modified = false;
                        string matchedKey = "";

                        foreach (var kvp in replacements)
                        {
                            if (newText.Contains(kvp.Key))
                            {
                                newText = newText.Replace(kvp.Key, kvp.Value);
                                modified = true;
                                matchedKey = kvp.Key;
                            }
                        }

                        if (modified)
                        {
                            // Pause scan timer, start replace timer
                            scanStopwatch.Stop();
                            replaceStopwatch.Start();

                            if (propertyName == "TextOverride") entity.TextOverride = newText;
                            else if (propertyName == "TextString") entity.TextString = newText;
                            
                            // Pause replace timer, resume scan timer
                            replaceStopwatch.Stop();
                            scanStopwatch.Start();
                            
                            totalReplaced++;
                            
                            SimpleLogger.Log("BonnetFlange", $"Found placeholder {matchedKey} in {entityName}\nReplaced successfully");
                        }
                    }
                }
            }
            
            scanStopwatch.Stop();
            
            SimpleLogger.Log("BonnetFlange", $"Completed Scan & Replace. Scanned {totalAnnotationsScanned} annotations, replaced {totalReplaced}.");

            return new CadOperationTimes
            {
                ScanTimeMs = scanStopwatch.ElapsedMilliseconds,
                ReplaceTimeMs = replaceStopwatch.ElapsedMilliseconds
            };
        }

        public void UpdateTitleBlockAttributes(DrawingInformation info)
        {
            if (_cadDoc == null) throw new InvalidOperationException("No drawing is currently open.");
            
            dynamic layouts = _cadDoc.Layouts;
            int layoutCount = layouts.Count;
            
            bool titleUpdated = false;
            bool customerUpdated = false;
            bool projectUpdated = false;
            bool drawingNoUpdated = false;
            bool revUpdated = false;
            bool drawnUpdated = false;
            bool checkedUpdated = false;
            bool approvedUpdated = false;
            bool dateUpdated = false;

            SimpleLogger.Log("BonnetFlange", "Updating Title Block Attributes...");

            for (int l = 0; l < layoutCount; l++)
            {
                dynamic layout = layouts.Item(l);
                dynamic block = layout.Block;
                int count = block.Count;

                for (int i = 0; i < count; i++)
                {
                    dynamic entity = block.Item(i);
                    string entityName = entity.EntityName;

                    if (entityName == "AcDbBlockReference" && entity.HasAttributes)
                    {
                        dynamic attributes = entity.GetAttributes();
                        foreach (dynamic attr in attributes)
                        {
                            string tag = attr.TagString;
                            string tagUpper = tag.ToUpper();

                            // Clean up Title for single-line if needed
                            string safeTitle = info.Title?.Replace("\r\n", " ").Replace("\n", " ").Trim() ?? "";
                            string dateStr = info.Date.ToString("dd-MM-yyyy");

                            if (tagUpper == "TITLE" || tagUpper == "TITLE1" || tagUpper == "TITLE2" || tagUpper == "DWG_TITLE" || tagUpper == "DRAWING_TITLE") 
                            { 
                                attr.TextString = safeTitle; 
                                titleUpdated = true; 
                                SimpleLogger.Log("BonnetFlange", $"Updated attribute [{tag}] to: {safeTitle}");
                            }
                            else if (tagUpper == "CUSTOMER" || tagUpper == "CLIENT" || tagUpper == "CUST") 
                            { 
                                attr.TextString = info.CustomerName; 
                                customerUpdated = true; 
                                SimpleLogger.Log("BonnetFlange", $"Updated attribute [{tag}] to: {info.CustomerName}");
                            }
                            else if (tagUpper == "PROJECT" || tagUpper == "PROJECTNO" || tagUpper == "PROJECT_NO" || tagUpper == "PROJECTNUMBER" || tagUpper == "PROJ") 
                            { 
                                attr.TextString = info.ProjectNo; 
                                projectUpdated = true; 
                                SimpleLogger.Log("BonnetFlange", $"Updated attribute [{tag}] to: {info.ProjectNo}");
                            }
                            else if (tagUpper == "DRAWINGNO" || tagUpper == "DRAWING_NO" || tagUpper == "DWGNO" || tagUpper == "DWG_NO" || tagUpper == "DRG_NO" || tagUpper == "DWG") 
                            { 
                                attr.TextString = info.DrawingNo; 
                                drawingNoUpdated = true; 
                                SimpleLogger.Log("BonnetFlange", $"Updated attribute [{tag}] to: {info.DrawingNo}");
                            }
                            else if (tagUpper == "REV" || tagUpper == "REVISION" || tagUpper == "REV_NO" || tagUpper == "REV." || tagUpper == "0") 
                            { 
                                attr.TextString = info.Revision; 
                                revUpdated = true; 
                                SimpleLogger.Log("BonnetFlange", $"Updated attribute [{tag}] to: {info.Revision}");
                            }
                            else if (tagUpper == "DRAWN" || tagUpper == "DRAWN_BY" || tagUpper == "DRAWNBY" || tagUpper == "PREPARED_BY" || tagUpper == "DRN") 
                            { 
                                attr.TextString = info.PreparedBy; 
                                drawnUpdated = true; 
                                SimpleLogger.Log("BonnetFlange", $"Updated attribute [{tag}] to: {info.PreparedBy}");
                            }
                            else if (tagUpper == "CHECKED" || tagUpper == "CHECKED_BY" || tagUpper == "CHECKEDBY" || tagUpper == "CHK" || tagUpper == "CHKD") 
                            { 
                                attr.TextString = info.CheckedBy; 
                                checkedUpdated = true; 
                                SimpleLogger.Log("BonnetFlange", $"Updated attribute [{tag}] to: {info.CheckedBy}");
                            }
                            else if (tagUpper == "APPROVED" || tagUpper == "APPROVED_BY" || tagUpper == "APPROVEDBY" || tagUpper == "APP" || tagUpper == "APPD") 
                            { 
                                attr.TextString = info.ApprovedBy; 
                                approvedUpdated = true; 
                                SimpleLogger.Log("BonnetFlange", $"Updated attribute [{tag}] to: {info.ApprovedBy}");
                            }
                            else if (tagUpper == "DATE" || tagUpper == "DWG_DATE" || tagUpper == "DRAWING_DATE") 
                            { 
                                attr.TextString = dateStr; 
                                dateUpdated = true; 
                                SimpleLogger.Log("BonnetFlange", $"Updated attribute [{tag}] to: {dateStr}");
                            }
                            else
                            {
                                SimpleLogger.Log("BonnetFlange", $"Ignored unknown attribute tag: [{tag}]");
                            }
                        }
                    }
                }
            }

            if (!titleUpdated) SimpleLogger.Log("BonnetFlange", "Warning: TITLE attribute not found in Title Block.");
            if (!customerUpdated) SimpleLogger.Log("BonnetFlange", "Warning: CUSTOMER attribute not found in Title Block.");
            if (!projectUpdated) SimpleLogger.Log("BonnetFlange", "Warning: PROJECTNO attribute not found in Title Block.");
            if (!drawingNoUpdated) SimpleLogger.Log("BonnetFlange", "Warning: DWG attribute not found in Title Block.");
            if (!revUpdated) SimpleLogger.Log("BonnetFlange", "Warning: REV attribute not found in Title Block.");
            if (!drawnUpdated) SimpleLogger.Log("BonnetFlange", "Warning: DRAWN attribute not found in Title Block.");
            if (!checkedUpdated) SimpleLogger.Log("BonnetFlange", "Warning: CHECKED attribute not found in Title Block.");
            if (!approvedUpdated) SimpleLogger.Log("BonnetFlange", "Warning: APPROVED attribute not found in Title Block.");
            if (!dateUpdated) SimpleLogger.Log("BonnetFlange", "Warning: DATE attribute not found in Title Block.");
            
            SimpleLogger.Log("BonnetFlange", "Title Block Attributes Update Complete.");
        }

        public void SaveAs(string newFilePath)
        {
            if (_cadDoc == null) throw new InvalidOperationException("No drawing is currently open.");
            
            // Explicitly saving directly without any Visual Refresh operations
            _cadDoc.SaveAs(newFilePath);
        }

        public void CloseDrawing()
        {
            if (_cadDoc != null)
            {
                _cadDoc.Close(false); // Do not save changes to the template itself
                Marshal.ReleaseComObject(_cadDoc);
                _cadDoc = null;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // Dispose managed state (managed objects)
                }

                // Free unmanaged resources (unmanaged objects) and override finalizer
                CloseDrawing();
                
                // Do NOT dispose _cadApp, it is managed globally by CadSessionManager.

                _disposedValue = true;
            }
        }

        ~GstarCadAdapter()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
