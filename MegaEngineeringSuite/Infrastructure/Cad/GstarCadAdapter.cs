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

        public CadOperationTimes ReplaceDimensionPlaceholders(Dictionary<string, string> replacements)
        {
            if (_cadDoc == null) throw new InvalidOperationException("No drawing is currently open.");

            var scanStopwatch = new Stopwatch();
            var replaceStopwatch = new Stopwatch();

            scanStopwatch.Start();

            dynamic layouts = _cadDoc.Layouts;
            int layoutCount = layouts.Count;
            
            int totalDimensionsScanned = 0;
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

                    if (entityName.Contains("Dimension"))
                    {
                        totalDimensionsScanned++;
                        try
                        {
                            string textOverride = entity.TextOverride;
                            if (replacements.TryGetValue(textOverride, out string? newText) && newText != null)
                            {
                                // Pause scan timer, start replace timer
                                scanStopwatch.Stop();
                                replaceStopwatch.Start();

                                entity.TextOverride = newText;
                                
                                // Pause replace timer, resume scan timer
                                replaceStopwatch.Stop();
                                scanStopwatch.Start();
                                
                                totalReplaced++;
                                
                                SimpleLogger.Log("BonnetFlange", $"Replaced '{textOverride}' -> '{newText}' on Layout: {layoutName}");
                            }
                        }
                        catch
                        {
                            // Swallow unsupported TextOverride exceptions
                        }
                    }
                }
            }
            
            scanStopwatch.Stop();
            
            SimpleLogger.Log("BonnetFlange", $"Completed Scan & Replace. Scanned {totalDimensionsScanned} dimensions, replaced {totalReplaced}.");

            return new CadOperationTimes
            {
                ScanTimeMs = scanStopwatch.ElapsedMilliseconds,
                ReplaceTimeMs = replaceStopwatch.ElapsedMilliseconds
            };
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
