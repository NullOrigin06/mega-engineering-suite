using System;
using MegaEngineeringSuite.BonnetFlange;

namespace MegaEngineeringSuite.DebugTools
{
    public class BonnetFlangeTestRunner
    {
        public static void RunTest()
        {
            var data = new BonnetFlangeData
            {
                OD = 1070,
                ID = 932,
                LinerOD = 984,
                LinerID = 920,
                Thickness = 36
            };
            
            var generator = new BonnetFlangeGenerator();
            generator.Generate(data, new DrawingInformation());
        }
    }
}
