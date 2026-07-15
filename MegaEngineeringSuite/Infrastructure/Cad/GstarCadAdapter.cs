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

        // --- Performance Profiling Counters ---
        public static int CounterGetObjectByHandle = 0;
        public static int CounterEntityReads = 0;
        public static int CounterEntityWrites = 0;
        public static int CounterLayoutScans = 0;
        public static int CounterModelSpaceIterations = 0;
        public static int CounterRegens = 0;
        public static int CounterSaves = 0;

        public static void ResetCounters()
        {
            CounterGetObjectByHandle = 0;
            CounterEntityReads = 0;
            CounterEntityWrites = 0;
            CounterLayoutScans = 0;
            CounterModelSpaceIterations = 0;
            CounterRegens = 0;
            CounterSaves = 0;
        }

        public GstarCadAdapter()
        {
            // Acquire CAD instance centrally (reusing if possible)
            _cadApp = CadSessionManager.Instance.GetCadApplication();
        }

        public void AttachToExistingSession(object cadApp, object cadDoc)
        {
            _cadApp = cadApp;
            _cadDoc = cadDoc;
            SimpleLogger.Log("GstarCadAdapter", "Attached to existing CAD session and document.");
        }

        public void OpenDrawing(string filePath)
        {
            if (_cadApp == null) throw new InvalidOperationException("CAD application is not initialized.");
            
            _cadDoc = _cadApp.Documents.Open(filePath);
            
            if (_cadDoc == null)
            {
                throw new InvalidOperationException($"Failed to open document: {filePath}");
            }
            
            string actualPath = _cadDoc.FullName;
            if (!actualPath.Equals(filePath, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"CAD opened unexpected document. Expected: {filePath}, Actual: {actualPath}");
            }
            
            SimpleLogger.Log("GstarCadAdapter", $"Successfully verified and opened: {actualPath}");
        }

        public void SetSystemVariable(string name, object value)
        {
            if (_cadDoc == null) throw new InvalidOperationException("No drawing is currently open.");
            _cadDoc.SetVariable(name, value);
        }

        public object GetSystemVariable(string name)
        {
            if (_cadDoc == null) throw new InvalidOperationException("No drawing is currently open.");
            return _cadDoc.GetVariable(name);
        }

        public void SendCommand(string command)
        {
            if (_cadDoc == null) throw new InvalidOperationException("No drawing is currently open.");
            if (command.IndexOf("REGEN", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                CounterRegens++;
            }
            _cadDoc.SendCommand(command);
        }

        public CadDocumentIdentity GetDocumentIdentity()
        {
            if (_cadDoc == null) throw new InvalidOperationException("No drawing is currently open.");
            
            var identity = new CadDocumentIdentity();
            try
            {
                identity.DocumentName = _cadDoc.Name;
                identity.FullPath = _cadDoc.FullName;
                identity.IsActiveDocument = _cadDoc.Name == _cadApp.ActiveDocument.Name;
                
                // Attempt to get database/pointer details if possible via COM
                try { identity.DatabaseHandle = _cadDoc.Database?.GetHashCode().ToString() ?? "N/A"; } catch { identity.DatabaseHandle = "N/A"; }
                
                try 
                { 
                    dynamic activeLayout = _cadDoc.ActiveLayout;
                    identity.LayoutName = activeLayout.Name;
                    dynamic modelSpace = _cadDoc.ModelSpace;
                    identity.ModelSpaceCount = modelSpace.Count;
                } 
                catch 
                { 
                    identity.LayoutName = "Unknown"; 
                }
            }
            catch (Exception ex)
            {
                identity.DocumentName = "ERROR: " + ex.Message;
            }
            return identity;
        }

        public dynamic GetEntityByHandle(string handle)
        {
            CounterGetObjectByHandle++;
            if (_cadDoc == null) throw new InvalidOperationException("No drawing is currently open.");
            return _cadDoc.HandleToObject(handle);
        }

        public System.Collections.Generic.List<MegaEngineeringSuite.TubeSheet.PlaceholderDescriptor> DiscoverPlaceholders(DiscoveryMode mode = DiscoveryMode.MetaLayers, System.Collections.Generic.Dictionary<string, dynamic>? entityCache = null, System.Collections.Generic.Dictionary<string, dynamic>? titleBlockCache = null)
        {
            if (_cadDoc == null) throw new InvalidOperationException("No drawing is currently open.");

            var descriptors = new List<MegaEngineeringSuite.TubeSheet.PlaceholderDescriptor>();
            dynamic layouts = _cadDoc.Layouts;
            int layoutCount = layouts.Count;

            for (int l = 0; l < layoutCount; l++)
            {
                CounterLayoutScans++;
                dynamic layout = layouts.Item(l);
                bool isPaperSpace = layout.ModelType == false;
                
                dynamic block = layout.Block;
                int count = block.Count;

                for (int i = 0; i < count; i++)
                {
                    CounterModelSpaceIterations++;
                    dynamic entity = block.Item(i);
                    CounterEntityReads++;
                    string layer = entity.Layer;

                    bool shouldProcess = mode == DiscoveryMode.All || layer.StartsWith("META_", StringComparison.OrdinalIgnoreCase);

                    if (shouldProcess)
                    {
                        string entityName = entity.EntityName;
                        string? currentText = null;
                        
                        try
                        {
                            if (entityName.Contains("Dimension"))
                            {
                                currentText = entity.TextOverride;
                            }
                            else if (entityName.Contains("MText") || entityName.Contains("Text") || entityName.Contains("MLeader"))
                            {
                                currentText = entity.TextString;
                            }
                            else if (entityName == "AcDbBlockReference" && entity.HasAttributes)
                            {
                                dynamic attributes = entity.GetAttributes();
                                foreach (dynamic attr in attributes)
                                {
                                    var desc = new MegaEngineeringSuite.TubeSheet.PlaceholderDescriptor
                                    {
                                        EntityHandle = attr.Handle,
                                        PlaceholderName = attr.TextString,
                                        Layer = layer,
                                        EntityType = "AttributeReference",
                                        OwnerBlock = entity.Name,
                                        PaperSpace = isPaperSpace,
                                        ObjectId = attr.ObjectID
                                    };
                                    descriptors.Add(desc);
                                    if (entityCache != null)
                                    {
                                        entityCache[attr.Handle] = attr;
                                    }
                                    if (titleBlockCache != null)
                                    {
                                        string tagUpper = attr.TagString.ToUpper();
                                        titleBlockCache[tagUpper] = attr;
                                    }
                                }
                                continue;
                            }
                        }
                        catch
                        {
                            // Swallow unsupported property exceptions
                        }

                        if (!string.IsNullOrEmpty(currentText))
                        {
                            var desc = new MegaEngineeringSuite.TubeSheet.PlaceholderDescriptor
                            {
                                EntityHandle = entity.Handle,
                                PlaceholderName = currentText,
                                Layer = layer,
                                EntityType = entityName.Replace("AcDb", ""),
                                OwnerBlock = layout.Name,
                                PaperSpace = isPaperSpace,
                                ObjectId = entity.ObjectID
                            };
                            descriptors.Add(desc);
                            if (entityCache != null)
                            {
                                entityCache[entity.Handle] = entity;
                            }
                        }
                    }
                }
            }

            return descriptors;
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

        public void UpdateTitleBlockAttributes(DrawingInformation info, System.Collections.Generic.Dictionary<string, dynamic>? titleBlockCache = null)
        {
            if (_cadDoc == null) throw new InvalidOperationException("No drawing is currently open.");
            
            SimpleLogger.Log("BonnetFlange", "Updating Title Block Attributes...");
            
            bool titleUpdated = false;
            bool customerUpdated = false;
            bool projectUpdated = false;
            bool drawingNoUpdated = false;
            bool revUpdated = false;
            bool drawnUpdated = false;
            bool checkedUpdated = false;
            bool approvedUpdated = false;
            bool dateUpdated = false;

            string safeTitle = info.Title?.Trim() ?? "";
            string fullTitle = safeTitle;
            string dateStr = info.Date.ToString("dd-MM-yyyy");

            if (titleBlockCache != null && titleBlockCache.Count > 0)
            {
                // Fast path: use cache
                foreach (var kvp in titleBlockCache)
                {
                    string tagUpper = kvp.Key;
                    dynamic attr = kvp.Value;
                    
                    if (tagUpper == "TITLE" || tagUpper == "TITLE1" || tagUpper == "TITLE2" || tagUpper == "DWG_TITLE" || tagUpper == "DRAWING_TITLE") 
                    { 
                        if (attr.TextString != fullTitle) { attr.TextString = fullTitle; }
                        titleUpdated = true; 
                        SimpleLogger.Log("BonnetFlange", $"Updated attribute [{tagUpper}] to: {fullTitle.Replace(Environment.NewLine, "\\n")}");
                    }
                    else if (tagUpper == "CUSTOMER" || tagUpper == "CLIENT" || tagUpper == "CUST") 
                    { 
                        if (attr.TextString != info.CustomerName) { attr.TextString = info.CustomerName; }
                        customerUpdated = true; 
                        SimpleLogger.Log("BonnetFlange", $"Updated attribute [{tagUpper}] to: {info.CustomerName}");
                    }
                    else if (tagUpper == "PROJECT" || tagUpper == "PROJECTNO" || tagUpper == "PROJECT_NO" || tagUpper == "PROJECTNUMBER" || tagUpper == "PROJ") 
                    { 
                        if (attr.TextString != info.ProjectNo) { attr.TextString = info.ProjectNo; }
                        projectUpdated = true; 
                        SimpleLogger.Log("BonnetFlange", $"Updated attribute [{tagUpper}] to: {info.ProjectNo}");
                    }
                    else if (tagUpper == "DRAWINGNO" || tagUpper == "DRAWING_NO" || tagUpper == "DWGNO" || tagUpper == "DWG_NO" || tagUpper == "DRG_NO" || tagUpper == "DWG") 
                    { 
                        if (attr.TextString != info.DrawingNo) { attr.TextString = info.DrawingNo; }
                        drawingNoUpdated = true; 
                        SimpleLogger.Log("BonnetFlange", $"Updated attribute [{tagUpper}] to: {info.DrawingNo}");
                    }
                    else if (tagUpper == "REV" || tagUpper == "REVISION" || tagUpper == "REV_NO" || tagUpper == "REV." || tagUpper == "0") 
                    { 
                        if (attr.TextString != info.Revision) { attr.TextString = info.Revision; }
                        revUpdated = true; 
                        SimpleLogger.Log("BonnetFlange", $"Updated attribute [{tagUpper}] to: {info.Revision}");
                    }
                    else if (tagUpper == "DRAWN" || tagUpper == "DRAWN_BY" || tagUpper == "DRAWNBY" || tagUpper == "PREPARED_BY" || tagUpper == "DRN") 
                    { 
                        if (attr.TextString != info.PreparedBy) { attr.TextString = info.PreparedBy; }
                        drawnUpdated = true; 
                        SimpleLogger.Log("BonnetFlange", $"Updated attribute [{tagUpper}] to: {info.PreparedBy}");
                    }
                    else if (tagUpper == "CHECKED" || tagUpper == "CHECKED_BY" || tagUpper == "CHECKEDBY" || tagUpper == "CHK") 
                    { 
                        if (attr.TextString != info.CheckedBy) { attr.TextString = info.CheckedBy; }
                        checkedUpdated = true; 
                        SimpleLogger.Log("BonnetFlange", $"Updated attribute [{tagUpper}] to: {info.CheckedBy}");
                    }
                    else if (tagUpper == "APPROVED" || tagUpper == "APPROVED_BY" || tagUpper == "APPROVEDBY" || tagUpper == "APP") 
                    { 
                        if (attr.TextString != info.ApprovedBy) { attr.TextString = info.ApprovedBy; }
                        approvedUpdated = true; 
                        SimpleLogger.Log("BonnetFlange", $"Updated attribute [{tagUpper}] to: {info.ApprovedBy}");
                    }
                    else if (tagUpper == "DATE" || tagUpper == "ISSUEDATE" || tagUpper == "ISSUE_DATE") 
                    { 
                        if (attr.TextString != dateStr) { attr.TextString = dateStr; }
                        dateUpdated = true; 
                        SimpleLogger.Log("BonnetFlange", $"Updated attribute [{tagUpper}] to: {dateStr}");
                    }
                }
                return; // End fast path
            }
            
            // Slow path: Full ModelSpace scan
            dynamic layouts = _cadDoc.Layouts;
            int layoutCount = layouts.Count;

            SimpleLogger.Log("BonnetFlange", "Updating Title Block Attributes (Slow path)...");

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

                            // Clean up Title for single-line
                            // Variables safeTitle, fullTitle, dateStr already defined in outer scope

                            if (tagUpper == "TITLE" || tagUpper == "TITLE1" || tagUpper == "TITLE2" || tagUpper == "DWG_TITLE" || tagUpper == "DRAWING_TITLE") 
                            { 
                                if (attr.TextString != fullTitle) { attr.TextString = fullTitle; }
                                titleUpdated = true; 
                                SimpleLogger.Log("BonnetFlange", $"Updated attribute [{tagUpper}] to: {fullTitle.Replace(Environment.NewLine, "\\n")}");
                            }
                            else if (tagUpper == "CUSTOMER" || tagUpper == "CLIENT" || tagUpper == "CUST") 
                            { 
                                if (attr.TextString != info.CustomerName) { attr.TextString = info.CustomerName; }
                                customerUpdated = true; 
                                SimpleLogger.Log("BonnetFlange", $"Updated attribute [{tagUpper}] to: {info.CustomerName}");
                            }
                            else if (tagUpper == "PROJECT" || tagUpper == "PROJECTNO" || tagUpper == "PROJECT_NO" || tagUpper == "PROJECTNUMBER" || tagUpper == "PROJ") 
                            { 
                                if (attr.TextString != info.ProjectNo) { attr.TextString = info.ProjectNo; }
                                projectUpdated = true; 
                                SimpleLogger.Log("BonnetFlange", $"Updated attribute [{tagUpper}] to: {info.ProjectNo}");
                            }
                            else if (tagUpper == "DRAWINGNO" || tagUpper == "DRAWING_NO" || tagUpper == "DWGNO" || tagUpper == "DWG_NO" || tagUpper == "DRG_NO" || tagUpper == "DWG") 
                            { 
                                if (attr.TextString != info.DrawingNo) { attr.TextString = info.DrawingNo; }
                                drawingNoUpdated = true; 
                                SimpleLogger.Log("BonnetFlange", $"Updated attribute [{tagUpper}] to: {info.DrawingNo}");
                            }
                            else if (tagUpper == "REV" || tagUpper == "REVISION" || tagUpper == "REV_NO" || tagUpper == "REV." || tagUpper == "0") 
                            { 
                                if (attr.TextString != info.Revision) { attr.TextString = info.Revision; }
                                revUpdated = true; 
                                SimpleLogger.Log("BonnetFlange", $"Updated attribute [{tagUpper}] to: {info.Revision}");
                            }
                            else if (tagUpper == "DRAWN" || tagUpper == "DRAWN_BY" || tagUpper == "DRAWNBY" || tagUpper == "PREPARED_BY" || tagUpper == "DRN") 
                            { 
                                if (attr.TextString != info.PreparedBy) { attr.TextString = info.PreparedBy; }
                                drawnUpdated = true; 
                                SimpleLogger.Log("BonnetFlange", $"Updated attribute [{tagUpper}] to: {info.PreparedBy}");
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
            CounterSaves++;
            // Explicitly saving directly without any Visual Refresh operations
            _cadDoc.SaveAs(newFilePath);
        }

        public void Save()
        {
            if (_cadDoc == null) throw new InvalidOperationException("No drawing is currently open.");
            CounterSaves++;
            _cadDoc.Save();
        }

        public void ReleaseDocumentReference()
        {
            if (_cadDoc != null)
            {
                try
                {
                    Marshal.ReleaseComObject(_cadDoc);
                }
                finally
                {
                    _cadDoc = null;
                }
            }
        }

        public void CloseDrawing()
        {
            if (_cadDoc != null)
            {
                _cadDoc.Close(false); // Do not save changes to the template itself
                ReleaseDocumentReference();
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
        public List<MegaEngineeringSuite.TubeSheet.BlockAttributeDescriptor> DiscoverBlockAttributes(string blockName)
        {
            if (_cadDoc == null) throw new InvalidOperationException("No drawing is currently open.");
            var descriptors = new List<MegaEngineeringSuite.TubeSheet.BlockAttributeDescriptor>();
            dynamic layouts = _cadDoc.Layouts;
            int layoutCount = layouts.Count;

            for (int l = 0; l < layoutCount; l++)
            {
                dynamic layout = layouts.Item(l);
                dynamic block = layout.Block;
                int count = block.Count;

                for (int i = 0; i < count; i++)
                {
                    dynamic entity = block.Item(i);
                    string entityName = entity.EntityName;

                    if (entityName == "AcDbBlockReference" && entity.Name.Equals(blockName, StringComparison.OrdinalIgnoreCase) && entity.HasAttributes)
                    {
                        dynamic attributes = entity.GetAttributes();
                        foreach (dynamic attr in attributes)
                        {
                            descriptors.Add(new MegaEngineeringSuite.TubeSheet.BlockAttributeDescriptor
                            {
                                BlockHandle = entity.Handle,
                                AttributeHandle = attr.Handle,
                                Tag = attr.TagString,
                                Value = attr.TextString,
                                Layout = layout.Name,
                                BlockName = entity.Name,
                                IsConstant = attr.Constant,
                                IsInvisible = attr.Invisible
                            });
                        }
                    }
                }
            }
            return descriptors;
        }

        public string UpdateBlockAttribute(string blockHandle, string tag, string newValue)
        {
            if (_cadDoc == null) throw new InvalidOperationException("No drawing is currently open.");
            
            dynamic blockEntity = _cadDoc.HandleToObject(blockHandle);
            if (blockEntity.EntityName != "AcDbBlockReference" || !blockEntity.HasAttributes)
            {
                throw new InvalidOperationException($"Entity {blockHandle} is not a valid BlockReference with attributes.");
            }

            dynamic attributes = blockEntity.GetAttributes();
            foreach (dynamic attr in attributes)
            {
                if (attr.TagString.Equals(tag, StringComparison.OrdinalIgnoreCase))
                {
                    attr.TextString = newValue;
                    CounterEntityWrites++;
                    return attr.TextString; // Read-back
                }
            }

            throw new InvalidOperationException($"Attribute with tag '{tag}' not found in block '{blockHandle}'.");
        }
    }
}
