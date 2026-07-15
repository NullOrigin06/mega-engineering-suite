using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using MegaEngineeringSuite.Infrastructure.Cad;

namespace TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Stage 11.4 - COM Search Starting...");

            string templatePath = @"C:\Users\PARTH\source\repos\MegaEngineeringSuite\Templates\FINAL TUBESHEET.dwg";
            string logPath = @"C:\Users\PARTH\source\repos\MegaEngineeringSuite\MegaEngineeringSuite\bin\Debug\net10.0-windows\Logs\Stage11_4_COMSearch.md";
            
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("# Stage 11.4 DWG and COM Identity Audit\n");

            sb.AppendLine("## 1. File Identity Audit: Source Template");
            if (File.Exists(templatePath))
            {
                FileInfo fi = new FileInfo(templatePath);
                sb.AppendLine($"- **Path**: {templatePath}");
                sb.AppendLine($"- **Size**: {fi.Length} bytes");
                sb.AppendLine($"- **Last Modified**: {fi.LastWriteTime}");
                sb.AppendLine($"- **SHA256**: {GetHash(templatePath)}");
            }
            else
            {
                sb.AppendLine("Template DWG not found at expected path!");
            }
            
            string genDir = @"C:\Users\PARTH\source\repos\MegaEngineeringSuite\GeneratedDrawings";
            string generatedPath = "";
            if (Directory.Exists(genDir))
            {
                var latestFile = new DirectoryInfo(genDir).GetFiles("TubeSheet_Output_*.dwg").OrderByDescending(f => f.LastWriteTime).FirstOrDefault();
                if (latestFile != null)
                {
                    generatedPath = latestFile.FullName;
                    sb.AppendLine("\n## 2. File Identity Audit: Latest Generated Output");
                    sb.AppendLine($"- **Path**: {generatedPath}");
                    sb.AppendLine($"- **Size**: {latestFile.Length} bytes");
                    sb.AppendLine($"- **Last Modified**: {latestFile.LastWriteTime}");
                    sb.AppendLine($"- **SHA256**: {GetHash(generatedPath)}");
                }
            }

            sb.AppendLine("\n## 3. Live COM Search Audit");
            try
            {
                dynamic acadApp = CadSessionManager.Instance.GetCadApplication();
                if (acadApp == null)
                {
                    sb.AppendLine("**COM ERROR**: Could not connect to GstarCAD Application.");
                }
                else
                {
                    string[] targets = { "BOM_TS_SIZE", "WHT1", "BOM_BAFFLE_SIZE", "WHT2", "1070", "238" };
                    
                    if (File.Exists(templatePath))
                    {
                        sb.AppendLine("\n### --- SEARCHING TEMPLATE DWG ---");
                        dynamic doc = acadApp.Documents.Open(templatePath);
                        SearchDocument(doc, targets, sb);
                        doc.Close(false);
                    }
                    
                    if (File.Exists(generatedPath))
                    {
                        sb.AppendLine("\n### --- SEARCHING LATEST GENERATED DWG ---");
                        dynamic doc2 = acadApp.Documents.Open(generatedPath);
                        SearchDocument(doc2, targets, sb);
                        doc2.Close(false);
                    }
                }
            }
            catch (Exception ex)
            {
                sb.AppendLine($"\n**COM ERROR**: {ex.Message}");
            }

            File.WriteAllText(logPath, sb.ToString());
            Console.WriteLine($"Audit complete. Report saved to:\n{logPath}");
            Console.WriteLine(sb.ToString());
        }

        static void SearchDocument(dynamic doc, string[] targets, StringBuilder sb)
        {
            sb.AppendLine($"**Active Document**: `{doc.FullName}`");
            
            SearchSpace(doc.ModelSpace, "ModelSpace", targets, sb);
            SearchSpace(doc.PaperSpace, "PaperSpace", targets, sb);
            
            sb.AppendLine("\nSearching inside Block Definitions (Unexploded Blocks)...");
            foreach (dynamic block in doc.Blocks)
            {
                string bName = block.Name;
                if (bName.StartsWith("*Model") || bName.StartsWith("*Paper")) continue;
                SearchSpace(block, $"Block Definition: {bName}", targets, sb);
            }
        }

        static void SearchSpace(dynamic space, string spaceName, string[] targets, StringBuilder sb)
        {
            bool foundAny = false;
            foreach (dynamic entity in space)
            {
                string objName = entity.ObjectName;
                if (objName == "AcDbText" || objName == "AcDbMText")
                {
                    string textString = entity.TextString;
                    foreach (var target in targets)
                    {
                        if (textString.Contains(target))
                        {
                            if (!foundAny)
                            {
                                sb.AppendLine($"\n#### Matches in {spaceName}:");
                                foundAny = true;
                            }
                            sb.AppendLine($"- **Entity**: {objName}");
                            sb.AppendLine($"  - Handle: {entity.Handle}");
                            sb.AppendLine($"  - Layer: {entity.Layer}");
                            sb.AppendLine($"  - Text: `{textString}`");
                        }
                    }
                }
                else if (objName == "AcDbBlockReference")
                {
                    if (entity.HasAttributes)
                    {
                        var atts = entity.GetAttributes();
                        foreach (dynamic att in atts)
                        {
                            string textString = att.TextString;
                            string tagString = att.TagString;
                            foreach (var target in targets)
                            {
                                if (textString.Contains(target) || tagString.Contains(target))
                                {
                                    if (!foundAny)
                                    {
                                        sb.AppendLine($"\n#### Matches in {spaceName}:");
                                        foundAny = true;
                                    }
                                    sb.AppendLine($"- **Entity**: BlockReference Attribute (Block: {entity.Name})");
                                    sb.AppendLine($"  - Handle: {att.Handle}");
                                    sb.AppendLine($"  - Tag: {tagString}");
                                    sb.AppendLine($"  - Text: `{textString}`");
                                }
                            }
                        }
                    }
                }
            }
        }

        static string GetHash(string path)
        {
            if (!File.Exists(path)) return "File not found";
            using (var sha = SHA256.Create())
            {
                using (var stream = File.OpenRead(path))
                {
                    byte[] hash = sha.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToUpperInvariant();
                }
            }
        }
    }
}
