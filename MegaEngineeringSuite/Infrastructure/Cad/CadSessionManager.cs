using System;
using System.Runtime.InteropServices;
using MegaEngineeringSuite.Infrastructure.Logging;

namespace MegaEngineeringSuite.Infrastructure.Cad
{
    public class CadSessionManager
    {
        private static CadSessionManager? _instance;
        private static readonly object _lock = new object();
        private dynamic? _cadApp;

        private CadSessionManager() { }

        public static CadSessionManager Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new CadSessionManager();
                    }
                    return _instance;
                }
            }
        }

        [DllImport("oleaut32.dll", PreserveSig = false)]
        static extern void GetActiveObject(ref Guid rclsid, IntPtr pvReserved, [MarshalAs(UnmanagedType.IUnknown)] out object ppunk);

        private object? GetActiveCOMObject(string progId)
        {
            Type? type = Type.GetTypeFromProgID(progId);
            if (type == null) return null;
            Guid clsid = type.GUID;
            try
            {
                object obj;
                GetActiveObject(ref clsid, IntPtr.Zero, out obj);
                return obj;
            }
            catch
            {
                return null;
            }
        }

        public dynamic GetCadApplication()
        {
            lock (_lock)
            {
                if (_cadApp != null)
                {
                    try
                    {
                        // Lightweight COM health check
                        string test = _cadApp.Name;
                        return _cadApp;
                    }
                    catch (Exception ex)
                    {
                        SimpleLogger.Log("CadSessionManager", $"Cached COM session is stale ({ex.Message}). Evicting stale RCW.");
                        ReleaseCadApplication();
                    }
                }

                // 1. Try to attach to existing GstarCAD instance from Running Object Table (ROT)
                _cadApp = GetActiveCOMObject("GstarCAD.Application");
                if (_cadApp != null)
                {
                    SimpleLogger.Log("CadSessionManager", "Reusing running GstarCAD instance from ROT.");
                    return _cadApp;
                }

                // 2. Try to attach to existing AutoCAD instance from Running Object Table (ROT)
                _cadApp = GetActiveCOMObject("AutoCAD.Application");
                if (_cadApp != null)
                {
                    SimpleLogger.Log("CadSessionManager", "Reusing running AutoCAD instance from ROT.");
                    return _cadApp;
                }

                // 3. Fallback to starting a new instance
                SimpleLogger.Log("CadSessionManager", "No running CAD instance found in ROT. Starting a new COM instance...");
                try
                {
                    Type? type = Type.GetTypeFromProgID("GstarCAD.Application");
                    if (type != null)
                    {
                        _cadApp = Activator.CreateInstance(type);
                        return _cadApp;
                    }
                }
                catch (Exception ex)
                {
                    SimpleLogger.Log("CadSessionManager", $"Failed to start GstarCAD COM instance: {ex.Message}");
                }

                try
                {
                    Type? type = Type.GetTypeFromProgID("AutoCAD.Application");
                    if (type != null)
                    {
                        _cadApp = Activator.CreateInstance(type);
                        return _cadApp;
                    }
                }
                catch (Exception ex)
                {
                    SimpleLogger.Log("CadSessionManager", $"Failed to start AutoCAD COM instance: {ex.Message}");
                }

                throw new InvalidOperationException("Could not find or start any supported CAD application.");
            }
        }

        public void ReleaseCadApplication()
        {
            lock (_lock)
            {
                if (_cadApp != null)
                {
                    try
                    {
                        if (Marshal.IsComObject(_cadApp))
                        {
                            Marshal.FinalReleaseComObject(_cadApp);
                        }
                    }
                    catch { }
                    finally
                    {
                        _cadApp = null;
                    }
                }
            }
        }
    }
}
