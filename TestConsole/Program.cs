using System;
using System.Collections.Generic;
using System.Linq;

namespace TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("--- COMPARING PARITY DRAWINGS ---");
            string baselinePath = @"C:\Users\PARTH\source\repos\MegaEngineeringSuite\GeneratedDrawings\BaselineParity.dwg";
            string currentPath = @"C:\Users\PARTH\source\repos\MegaEngineeringSuite\GeneratedDrawings\CurrentParity.dwg";
            
            var type = Type.GetTypeFromProgID("GstarCAD.Application");
            dynamic acadApp = Activator.CreateInstance(type);
            acadApp.Visible = true;
            
            Console.WriteLine("Extracting Baseline Data...");
            dynamic docB = acadApp.Documents.Open(baselinePath, true);
            var baselineTexts = ExtractTexts(docB);
            docB.Close(false);
            
            Console.WriteLine("Extracting Current Data...");
            dynamic docC = acadApp.Documents.Open(currentPath, true);
            var currentTexts = ExtractTexts(docC);
            docC.Close(false);

            int diffCount = 0;
            Console.WriteLine("\n--- PARITY RESULTS ---");
            foreach(var bKey in baselineTexts.Keys)
            {
                if (!currentTexts.ContainsKey(bKey))
                {
                    Console.WriteLine($"MISSING IN CURRENT: {bKey}");
                    diffCount++;
                }
                else if (baselineTexts[bKey] != currentTexts[bKey])
                {
                    Console.WriteLine($"MISMATCH: '{bKey}' -> Baseline: '{baselineTexts[bKey]}', Current: '{currentTexts[bKey]}'");
                    diffCount++;
                }
            }
            
            foreach(var cKey in currentTexts.Keys)
            {
                if (!baselineTexts.ContainsKey(cKey))
                {
                    Console.WriteLine($"EXTRA IN CURRENT: {cKey} = {currentTexts[cKey]}");
                    diffCount++;
                }
            }

            Console.WriteLine($"\nTotal Mismatches Found: {diffCount}");
            if (diffCount == 0)
            {
                Console.WriteLine("PARITY CHECK PASSED - 100% MATCH!");
            }
            
            acadApp.Quit();
        }

        static Dictionary<string, string> ExtractTexts(dynamic doc)
        {
            var results = new Dictionary<string, string>();
            
            foreach (dynamic space in new[] { doc.ModelSpace, doc.PaperSpace })
            {
                for (int i = 0; i < space.Count; i++)
                {
                    dynamic entity = space.Item(i);
                    string eType = entity.ObjectName;
                    
                    if (eType == "AcDbText" || eType == "AcDbMText")
                    {
                        string val = entity.TextString;
                        // Use coordinate as a sort of "key" since handles change
                        string key = $"{eType}_X{Math.Round(entity.InsertionPoint[0], 2)}_Y{Math.Round(entity.InsertionPoint[1], 2)}";
                        if (!results.ContainsKey(key))
                        {
                            results[key] = val;
                        }
                    }
                    else if (eType == "AcDbBlockReference" && entity.HasAttributes)
                    {
                        var atts = entity.GetAttributes();
                        foreach (dynamic att in atts)
                        {
                            string key = $"ATTR_{entity.Name}_{att.TagString}";
                            results[key] = att.TextString;
                        }
                    }
                }
            }
            return results;
        }
    }
}
