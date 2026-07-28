using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Win32;

namespace MegaEngineeringSuite.Infrastructure.Cad
{
    public class CadInstallationDescriptor
    {
        public string VersionName { get; set; } = "";
        public string ExecutablePath { get; set; } = "";
        public string DiscoverySource { get; set; } = "";

        public override string ToString()
        {
            return $"{VersionName} ({ExecutablePath})";
        }
    }

    public static class CadDiscoveryService
    {
        /// <summary>
        /// Scans system registry and local disk directories dynamically to discover all installed GstarCAD/AutoCAD instances.
        /// </summary>
        public static List<CadInstallationDescriptor> DiscoverInstalledCadExecutables()
        {
            var results = new List<CadInstallationDescriptor>();

            // 1. Scan Gstarsoft Vendor Registry Keys (HKLM\SOFTWARE\Gstarsoft\GstarCAD)
            ScanGstarsoftRegistry(RegistryView.Registry64, results);
            ScanGstarsoftRegistry(RegistryView.Registry32, results);

            // 2. Scan Standard App Paths
            ScanAppPathsRegistry(RegistryView.Registry64, results);
            ScanAppPathsRegistry(RegistryView.Registry32, results);

            // 3. Scan Shell Open Commands
            ScanShellOpenCommands(results);

            // 4. Dynamic Directory Scanning (Program Files)
            ScanDirectories(results);

            // Deduplicate by normalized path
            var uniqueResults = results
                .Where(r => !string.IsNullOrWhiteSpace(r.ExecutablePath) && File.Exists(r.ExecutablePath))
                .GroupBy(r => r.ExecutablePath.Trim().ToLowerInvariant())
                .Select(g => g.First())
                .OrderByDescending(r => ExtractNumericVersion(r.VersionName))
                .ToList();

            return uniqueResults;
        }

        private static void ScanGstarsoftRegistry(RegistryView view, List<CadInstallationDescriptor> results)
        {
            try
            {
                using var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view);
                using var gstarKey = baseKey.OpenSubKey(@"SOFTWARE\Gstarsoft\GstarCAD");
                if (gstarKey == null) return;

                foreach (var verSubKeyName in gstarKey.GetSubKeyNames())
                {
                    using var verKey = gstarKey.OpenSubKey(verSubKeyName);
                    if (verKey == null) continue;

                    foreach (var localeSubKeyName in verKey.GetSubKeyNames())
                    {
                        using var localeKey = verKey.OpenSubKey(localeSubKeyName);
                        if (localeKey == null) continue;

                        string? location = localeKey.GetValue("LOCATION") as string 
                                        ?? localeKey.GetValue("Path") as string;

                        string? prodName = localeKey.GetValue("ProductName") as string 
                                        ?? $"GstarCAD {verSubKeyName}";

                        if (!string.IsNullOrEmpty(location))
                        {
                            string exePath = Path.Combine(location, "gcad.exe");
                            if (File.Exists(exePath))
                            {
                                results.Add(new CadInstallationDescriptor
                                {
                                    VersionName = prodName,
                                    ExecutablePath = exePath,
                                    DiscoverySource = $"Registry ({view} Gstarsoft)"
                                });
                            }
                        }
                    }
                }
            }
            catch { }
        }

        private static void ScanAppPathsRegistry(RegistryView view, List<CadInstallationDescriptor> results)
        {
            try
            {
                using var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view);
                using var appPathKey = baseKey.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\gcad.exe");
                if (appPathKey != null)
                {
                    string? path = appPathKey.GetValue("") as string;
                    if (!string.IsNullOrEmpty(path) && File.Exists(path))
                    {
                        string folder = Path.GetFileName(Path.GetDirectoryName(path) ?? "");
                        results.Add(new CadInstallationDescriptor
                        {
                            VersionName = !string.IsNullOrEmpty(folder) ? folder : "GstarCAD",
                            ExecutablePath = path,
                            DiscoverySource = $"Registry App Paths ({view})"
                        });
                    }
                }
            }
            catch { }
        }

        private static void ScanShellOpenCommands(List<CadInstallationDescriptor> results)
        {
            string[] keysToTest = { @"gcad.exe\shell\open\command", @"GstarCAD.Drawing\shell\open\command" };
            foreach (var keyPath in keysToTest)
            {
                try
                {
                    using var key = Registry.ClassesRoot.OpenSubKey(keyPath);
                    if (key != null)
                    {
                        string? rawCommand = key.GetValue("") as string;
                        if (!string.IsNullOrEmpty(rawCommand))
                        {
                            string parsedPath = ExtractPathFromCommandLine(rawCommand);
                            if (!string.IsNullOrEmpty(parsedPath) && File.Exists(parsedPath))
                            {
                                results.Add(new CadInstallationDescriptor
                                {
                                    VersionName = Path.GetFileName(Path.GetDirectoryName(parsedPath) ?? "") ?? "GstarCAD",
                                    ExecutablePath = parsedPath,
                                    DiscoverySource = "Registry Shell Command"
                                });
                            }
                        }
                    }
                }
                catch { }
            }
        }

        private static void ScanDirectories(List<CadInstallationDescriptor> results)
        {
            string[] programFilesRoots = {
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)
            };

            foreach (var root in programFilesRoots)
            {
                if (string.IsNullOrEmpty(root) || !Directory.Exists(root)) continue;

                // Scan Gstarsoft directory
                string gstarDir = Path.Combine(root, "Gstarsoft");
                if (Directory.Exists(gstarDir))
                {
                    try
                    {
                        foreach (var subDir in Directory.GetDirectories(gstarDir, "GstarCAD*"))
                        {
                            string exePath = Path.Combine(subDir, "gcad.exe");
                            if (File.Exists(exePath))
                            {
                                string folderName = Path.GetFileName(subDir);
                                results.Add(new CadInstallationDescriptor
                                {
                                    VersionName = folderName,
                                    ExecutablePath = exePath,
                                    DiscoverySource = "Disk Scan (Gstarsoft)"
                                });
                            }
                        }
                    }
                    catch { }
                }

                // Scan Autodesk directory
                string autodeskDir = Path.Combine(root, "Autodesk");
                if (Directory.Exists(autodeskDir))
                {
                    try
                    {
                        foreach (var subDir in Directory.GetDirectories(autodeskDir, "AutoCAD*"))
                        {
                            string exePath = Path.Combine(subDir, "acad.exe");
                            if (File.Exists(exePath))
                            {
                                string folderName = Path.GetFileName(subDir);
                                results.Add(new CadInstallationDescriptor
                                {
                                    VersionName = folderName,
                                    ExecutablePath = exePath,
                                    DiscoverySource = "Disk Scan (Autodesk)"
                                });
                            }
                        }
                    }
                    catch { }
                }
            }
        }

        private static string ExtractPathFromCommandLine(string commandLine)
        {
            if (string.IsNullOrWhiteSpace(commandLine)) return "";

            string trimmed = commandLine.Trim();

            // Quoted path extraction
            if (trimmed.StartsWith("\""))
            {
                int nextQuote = trimmed.IndexOf('"', 1);
                if (nextQuote > 1)
                {
                    return trimmed.Substring(1, nextQuote - 1);
                }
            }

            // Unquoted path extraction up to first space or file extension
            int exeIndex = trimmed.IndexOf(".exe", StringComparison.OrdinalIgnoreCase);
            if (exeIndex > 0)
            {
                return trimmed.Substring(0, exeIndex + 4).Trim('"');
            }

            return trimmed;
        }

        private static int ExtractNumericVersion(string versionName)
        {
            if (string.IsNullOrEmpty(versionName)) return 0;
            
            var digits = new string(versionName.Where(char.IsDigit).ToArray());
            if (int.TryParse(digits, out int version) && version > 1990 && version < 2100)
            {
                return version;
            }
            return 0;
        }
    }
}
